using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Registro de cada operação feita na conta
class Transacao
{
    public string Tipo { get; }
    public decimal Valor { get; }
    public DateTime DataHora { get; }

    public Transacao(string tipo, decimal valor)
    {
        Tipo = tipo;
        Valor = valor;
        DataHora = DateTime.Now;
    }
}

class ContaBancaria
{
    // Campos privados: ninguém de fora acessa o PIN nem a lista diretamente
    private readonly string pin;
    private readonly List<Transacao> extrato = new List<Transacao>();

    public string Titular { get; }
    public int Numero { get; }
    public decimal Saldo { get; private set; }

    // Só leitura: dá pra consultar o extrato, mas não alterar de fora
    public IReadOnlyList<Transacao> Extrato => extrato;

    public ContaBancaria(string titular, int numero, string pin)
    {
        Titular = titular;
        Numero = numero;
        this.pin = pin;
    }

    public bool ValidarPin(string tentativa)
    {
        return tentativa == pin;
    }

    // Só aceita valores positivos
    public void Depositar(decimal valor)
    {
        if (valor > 0)
        {
            Saldo += valor;
            extrato.Add(new Transacao("Depósito", valor));
            Console.WriteLine($"Depósito de R$ {valor:F2} realizado.");
        }
        else
        {
            Console.WriteLine("Valor inválido. O depósito deve ser positivo.");
        }
    }

    // Devolve true se tinha saldo, false se não tinha
    public bool Sacar(decimal valor)
    {
        if (valor > 0 && valor <= Saldo)
        {
            Saldo -= valor;
            extrato.Add(new Transacao("Saque", valor));
            return true;
        }

        return false;
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8; // para o emoji aparecer

        // ---------- Criação da conta ----------
        Console.Write("Nome do titular: ");
        string titular = Console.ReadLine();

        string pin;
        while (true)
        {
            Console.Write("Crie um PIN de 4 números: ");
            pin = LerSenha();

            if (pin.Length == 4 && pin.All(char.IsDigit))
            {
                break;
            }

            Console.WriteLine("PIN inválido. Use exatamente 4 números.");
        }

        int numeroConta = new Random().Next(10000, 99999);
        ContaBancaria conta = new ContaBancaria(titular, numeroConta, pin);

        // ---------- Menu ----------
        bool saldoVisivel = false; // começa escondido
        int opcao;

        do
        {
            Console.Clear();

            string icone = saldoVisivel ? "👁" : "🙈";
            string saldoTexto = saldoVisivel ? $"R$ {conta.Saldo:F2}" : "R$ ******";

            Console.WriteLine("===== CONTA BANCÁRIA =====");
            Console.WriteLine($"Titular: {conta.Titular}  |  Conta: {conta.Numero}");
            Console.WriteLine($"Saldo: {saldoTexto}  {icone}");
            Console.WriteLine("--------------------------");
            Console.WriteLine("1 - Depositar");
            Console.WriteLine("2 - Sacar");
            Console.WriteLine("3 - Mostrar/ocultar saldo");
            Console.WriteLine("4 - Ver extrato");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha: ");

            if (!int.TryParse(Console.ReadLine(), out opcao))
            {
                opcao = -1;
            }

            Console.WriteLine();

            switch (opcao)
            {
                case 1:
                    Console.Write("Valor do depósito: R$ ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal deposito))
                    {
                        conta.Depositar(deposito);
                    }
                    else
                    {
                        Console.WriteLine("Digite um número válido.");
                    }
                    break;

                case 2:
                    Console.Write("Valor do saque: R$ ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal saque))
                    {
                        bool sacou = conta.Sacar(saque);
                        Console.WriteLine(sacou ? "Saque realizado!" : "Saldo insuficiente.");
                    }
                    else
                    {
                        Console.WriteLine("Digite um número válido.");
                    }
                    break;

                case 3:
                    if (saldoVisivel)
                    {
                        saldoVisivel = false; // esconder não precisa de PIN
                    }
                    else if (PedirPin(conta))
                    {
                        saldoVisivel = true;
                    }
                    break;

                case 4:
                    if (!saldoVisivel)
                    {
                        Console.WriteLine("Ative o olhinho (opção 3) para ver o extrato.");
                    }
                    else
                    {
                        MostrarExtrato(conta);
                    }
                    break;

                case 0:
                    Console.WriteLine("Encerrando...");
                    break;

                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }

            if (opcao != 0)
            {
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey(true);
            }

        } while (opcao != 0);
    }

    // Pede o PIN (3 tentativas). Devolve true se acertou.
    static bool PedirPin(ContaBancaria conta)
    {
        for (int tentativa = 1; tentativa <= 3; tentativa++)
        {
            Console.Write("Digite seu PIN: ");
            string digitado = LerSenha();

            if (conta.ValidarPin(digitado))
            {
                return true;
            }

            Console.WriteLine($"PIN incorreto. Tentativa {tentativa} de 3.");
        }

        Console.WriteLine("Número de tentativas excedido.");
        return false;
    }

    // Lê o que a pessoa digita mostrando * no lugar das teclas
    static string LerSenha()
    {
        StringBuilder senha = new StringBuilder();

        while (true)
        {
            ConsoleKeyInfo tecla = Console.ReadKey(true);

            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return senha.ToString();
            }

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (senha.Length > 0)
                {
                    senha.Remove(senha.Length - 1, 1);
                    Console.Write("\b \b"); // apaga o * da tela
                }
            }
            else if (!char.IsControl(tecla.KeyChar))
            {
                senha.Append(tecla.KeyChar);
                Console.Write("*");
            }
        }
    }

    static void MostrarExtrato(ContaBancaria conta)
    {
        Console.WriteLine("========== EXTRATO ==========");

        if (conta.Extrato.Count == 0)
        {
            Console.WriteLine("Nenhuma movimentação ainda.");
            return;
        }

        foreach (Transacao t in conta.Extrato)
        {
            string sinal = t.Tipo == "Depósito" ? "+" : "-";
            Console.WriteLine($"{t.DataHora:dd/MM/yyyy HH:mm}  {t.Tipo,-9}  {sinal} R$ {t.Valor,10:F2}");
        }

        Console.WriteLine("-----------------------------");
        Console.WriteLine($"Saldo atual: R$ {conta.Saldo:F2}");
    }
}