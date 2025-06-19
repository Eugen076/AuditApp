using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Globalization;

public class ExchangeRatesController : Controller
{
    private readonly HttpClient _httpClient;

    public ExchangeRatesController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index(string selectedCurrency = "EUR", string? date = null, string range = "7")
    {
        var parsedDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

        string convertFrom = Request.Query["convertFrom"];
        string convertTo = Request.Query["convertTo"];
        string amountStr = Request.Query["amount"];
        decimal? convertResult = null;

        if (!string.IsNullOrEmpty(convertFrom) && !string.IsNullOrEmpty(convertTo) && decimal.TryParse(amountStr, out decimal amount))
        {
            var convertUrl = $"https://api.frankfurter.app/latest?from={convertFrom}&to={convertTo}";
            var convertResp = await _httpClient.GetStringAsync(convertUrl);
            var convertJson = JObject.Parse(convertResp);
            var rate = convertJson["rates"]?[convertTo]?.Value<decimal>() ?? 0;

            convertResult = rate * amount;
        }

        ViewBag.ConvertFrom = convertFrom;
        ViewBag.ConvertTo = convertTo;
        ViewBag.Amount = amountStr;
        ViewBag.ConvertResult = convertResult;

        int days = int.TryParse(range, out var r) ? r : 7;
        var startDate = parsedDate.AddDays(-days + 1);

        string dateStr = parsedDate.ToString("yyyy-MM-dd");
        string startStr = startDate.ToString("yyyy-MM-dd");

        var symbolsUrl = "https://api.frankfurter.app/currencies";
        var symbolsResponse = await _httpClient.GetStringAsync(symbolsUrl);
        var symbolsJson = JObject.Parse(symbolsResponse);

        var rateHistory = new SortedDictionary<string, decimal>();

        if (selectedCurrency != "RON")
        {
            var historyUrl = $"https://api.frankfurter.app/{startStr}..{dateStr}?from=RON&to={selectedCurrency}";

            try
            {
                var historyResponse = await _httpClient.GetStringAsync(historyUrl);
                var historyJson = JObject.Parse(historyResponse);

                foreach (var item in historyJson["rates"])
                {
                    var data = item.Path.Replace("rates.", "");
                    var valoare = item.First?[selectedCurrency]?.Value<decimal>() ?? 0;

                    if (valoare > 0)
                        rateHistory[data] = valoare;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la istoricul graficului: " + ex.Message);
            }
        }

        var allRatesUrl = $"https://api.frankfurter.app/{dateStr}?from=RON";
        var allRatesResponse = await _httpClient.GetStringAsync(allRatesUrl);
        var allRatesJson = JObject.Parse(allRatesResponse);

        var cursuri = new List<(string Symbol, string Nume, decimal Rate)>();
        foreach (var token in symbolsJson)
        {
            var symbol = token.Key;
            var name = token.Value?.ToString() ?? symbol;

            if (symbol == "RON") continue;

            var rate = allRatesJson["rates"]?[symbol]?.Value<decimal>() ?? 0;
            if (rate > 0)
                cursuri.Add((symbol, name, rate));
        }

        ViewBag.Date = dateStr;
        ViewBag.SelectedCurrency = selectedCurrency;
        ViewBag.CurrencyName = symbolsJson[selectedCurrency]?.ToString();
        ViewBag.History = rateHistory;
        ViewBag.Symbols = symbolsJson;
        ViewBag.TabelCursuri = cursuri;
        ViewBag.Range = range;

        return View();
    }

}
