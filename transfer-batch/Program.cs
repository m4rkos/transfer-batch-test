using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace DotnetTransferBatch;

public class Program
{
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

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

        string fullPath = Path.GetFullPath(path);
        var fileInfo = new FileInfo(fullPath);

        // The cache could be used if file has the same path and was the same size than current in cache
        // Cria uma chave de cache única com base no caminho, tamanho e data de modificação
        string cacheKey = $"filecache_{fullPath}_{fileInfo.Length}_{fileInfo.LastWriteTimeUtc.Ticks}";

        // Tenta obter as linhas do arquivo a partir do cache
        if (!_cache.TryGetValue(cacheKey, out string[]? lines))
        {
            // Se não estiver no cache, lê o arquivo e armazena no cache
            byte[] fileBytes = File.ReadAllBytes(path);

            List<string> linesList = [];
            using (var ms = new MemoryStream(fileBytes))
            {
                using var sr = new StreamReader(ms);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        linesList.Add(line);
                    }
                }
            }

            lines = [.. linesList];

            // Configura as opções de expiração do cache (por exemplo, 5 minutos)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, lines, cacheEntryOptions);
        }

        var data = new Dictionary<string, List<decimal>>();

        decimal globalMax = decimal.MinValue;
        string? globalMaxAccount = null;

        foreach (var parts in from line in lines
                              let parts = line.Split(',')
                              select parts)
        {
            if (parts.Length != 3 || !decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                continue;
            var accountId = parts[0];
            if (!data.TryGetValue(accountId, out List<decimal>? value))
            {
                value = [];
                data[accountId] = value;
            }

            value.Add(amount);
            
            // updating the transaction with more value
            if (amount > globalMax)
            {
                globalMax = amount;
                globalMaxAccount = accountId;
            }
        }

        var outputLines = new List<string>();
        foreach (var account in data.Keys.OrderBy(x => x))
        {
            var transfers = data[account];
            var total = transfers.Sum();

            if (account == globalMaxAccount)
            {
                total -= globalMax;
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