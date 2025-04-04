using System.Globalization;

namespace DotnetTransferBatch;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: TransferBatch <path_to_transfers_file>");
            Environment.Exit(1);
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File not found: {path}");
            Environment.Exit(1);
        }

        // Carrega o arquivo inteiro em memória (cache) como array de bytes
        byte[] fileBytes = File.ReadAllBytes(path);

        // Utiliza MemoryStream para ler as linhas do arquivo carregado em memória
        List<string> lines = [];
        using (var ms = new MemoryStream(fileBytes))
        {
            using var sr = new StreamReader(ms);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }
        }

        var data = new Dictionary<string, List<decimal>>();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length != 3 || !decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                continue;

            var accountId = parts[0];
            if (!data.TryGetValue(accountId, out List<decimal>? value))
            {
                value = [];
                data[accountId] = value;
            }
            value.Add(amount);
        }

        var outputLines = new List<string>();
        foreach (var account in data.Keys.OrderBy(x => x))
        {
            var transfers = data[account];
            var total = transfers.Sum();

            if (transfers.Count > 1)
            {
                total -= transfers.Max();
            }

            var commission = Math.Round(total * 0.10m, 2);
            var output = commission % 1 == 0
                ? commission.ToString("F0", CultureInfo.InvariantCulture)
                : commission.ToString("F2", CultureInfo.InvariantCulture);

            outputLines.Add($"{account},{output}");
        }

        var result = string.Join(Environment.NewLine, outputLines) + Environment.NewLine;

        Console.Write(result);
    }
}