using System.Globalization;

namespace DotnetTransferBatch;

public class Program
{
    private static string? cachedFilePath;
    private static long cachedFileSize;
    private static string[]? cachedLines;
    private static DateTime cachedLastWriteTimeUtc;

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

        var fileInfo = new FileInfo(path);
        string[] lines;

        // The cache could be used if file has the same path and was the same size than current in cache
        if (cachedLines != null && 
            cachedFilePath == path && 
            cachedFileSize == fileInfo.Length &&
            cachedLastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
        {
            lines = cachedLines;
            Console.WriteLine("using cache memory");
        }
        else
        {
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
            cachedLines = lines;
            cachedFilePath = path;
            cachedFileSize = fileInfo.Length;
            cachedLastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
        }

        var data = new Dictionary<string, List<decimal>>();
        decimal globalMax = decimal.MinValue;
        string? globalMaxAccount = null;

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length != 3 || 
                !decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                continue;

            var accountId = parts[0];
            if (!data.TryGetValue(accountId, out List<decimal>? transfers))
            {
                transfers = [];
                data[accountId] = transfers;
            }
            transfers.Add(amount);

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