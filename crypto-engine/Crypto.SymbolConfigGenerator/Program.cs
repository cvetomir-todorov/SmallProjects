namespace Crypto.SymbolConfigGenerator;

public static class Program
{
    public static void Main()
    {
        // source: https://kurzovnilistek.eu/binance-usdt-eur-btc-eth-pairs-list/
        using StreamReader reader = new(Path.Combine("Files", "all-usdt-plain.txt"));
        const char currencySymbol = '$';
        const string currency = "USDT";
        int count = 128;

        while (!reader.EndOfStream)
        {
            if (count <= 0)
            {
                break;
            }

            string symbol = reader.ReadLine()!;
            int usdtIndex = symbol.IndexOf(currency, StringComparison.InvariantCulture);
            if (usdtIndex == 0)
            {
                continue;
            }
            string other = symbol.Substring(0, usdtIndex);

            Console.WriteLine(
                "{{ \"Symbol\": \"{0}\", \"DisplayText\": \"{1}/{2}\", \"CurrencySymbol\": \"{3}\", \"VolumeSpike\": {{ \"Length\": 10, \"Interval\": \"00:05:00.0\", \"Ratio\": 5.0 }} }},",
                symbol, other, currency, currencySymbol);
            count--;
        }
    }
}
