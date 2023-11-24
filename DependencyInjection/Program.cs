// Apka jest dla 10 najsilniejszych walut swiata i polskiej zlotowki
// Aby uzyc api wejdz na strone appcurrencyapi.com, stworz konto i wygeneruj klucz api
// Wklej go zamiast napisu tutaj i podmien string "xxxx" w 82 linijce
// https://api.currencyapi.com/v3/latest?apikey=TUTAJ&currencies=EUR%2CUSD%2CCHF%2CKYD%2CGBP%2CGIP%2CKWD%2CBHD%2COMR%2CJOD%2CPLN&base_currency=PLN
// Nie ma obslugi dla zlych danych wprowadzonych przez usera w konsoli
// Wszystkie dzialajace dane sa wyswietlane na biezaca w konsoli wiec prosze je wpisywac
// Inaczej wywali apke :)

using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DependencyInjection
{
    public class ApiResponse
    {
        public MetaData? Meta { get; set; }
        public Dictionary<string, Currency>? Data { get; set; }
    }

    public class MetaData
    {
        public DateTime LastUpdatedAt { get; set; }
    }

    public class Currency
    {
        public string? Code { get; set; }
        public decimal Value { get; set; }
    }

    public interface ICurrencyConverter
    {
        decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency);
    }

    public class FixedRateCurrencyConverter : ICurrencyConverter
    {
        static List<KeyValuePair<string, decimal>> StaticCurrencies = new List<KeyValuePair<string, decimal>>()
        {
            new KeyValuePair<string, decimal>("BHD", 0.094m), // Bahraini dinar
            new KeyValuePair<string, decimal>("CHF", 0.221m), // Swiss franc
            new KeyValuePair<string, decimal>("EUR", 0.229m), // Euro
            new KeyValuePair<string, decimal>("GBP", 0.199m), // British pound
            new KeyValuePair<string, decimal>("GIP", 0.199m), // Gibraltar pound
            new KeyValuePair<string, decimal>("JOD", 0.177m), // Jordanian dinar
            new KeyValuePair<string, decimal>("KWD", 0.077m), // Kuwaiti dinar
            new KeyValuePair<string, decimal>("KYD", 0.208m), // Cayman Islands dollar
            new KeyValuePair<string, decimal>("OMR", 0.096m), // Omani rial
            new KeyValuePair<string, decimal>("USD", 0.249m), // US dollar
            new KeyValuePair<string, decimal>("PLN", 1m) // Polish zloty
        };

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            var ExchangeFrom = StaticCurrencies.Single(kvp => kvp.Key == fromCurrency);
            var ExchangeTo = StaticCurrencies.Single(kvp => kvp.Key == toCurrency);

            if (fromCurrency != "PLN" && toCurrency != "PLN")
            {
                var PolishZloty = StaticCurrencies.Single(kvp => kvp.Key == "PLN");
                return Math.Round(PolishZloty.Value/ExchangeFrom.Value * amount * ExchangeTo.Value, 2);
            }

            if(fromCurrency == "PLN")
            {
                return Math.Round(amount * ExchangeTo.Value, 2);
            }

            return Math.Round(ExchangeTo.Value / ExchangeFrom.Value * amount, 2);
        }
    }

    public class LiveCurrencyConverter : ICurrencyConverter
    {

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "xxxx");
            var response = httpClient.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var responseBody = reader.ReadToEnd();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);

            var ExchangeFrom = apiResponse.Data.Single(kvp => kvp.Key == fromCurrency);
            var ExchangeTo = apiResponse.Data.Single(kvp => kvp.Key == toCurrency);

            if (fromCurrency != "PLN" && toCurrency != "PLN")
            {
                var PolishZloty = apiResponse.Data.Single(kvp => kvp.Key == "PLN");
                return Math.Round(PolishZloty.Value.Value / ExchangeFrom.Value.Value * amount * ExchangeTo.Value.Value, 2);
            }

            if (fromCurrency == "PLN")
            {
                return Math.Round(amount * ExchangeTo.Value.Value, 2);
            }

            return Math.Round(ExchangeTo.Value.Value / ExchangeFrom.Value.Value * amount, 2);
        }

    }

    public class CurrencyConversionApp
    {
        private readonly ICurrencyConverter currencyConverter;

        public CurrencyConversionApp(ICurrencyConverter converter)
        {
            currencyConverter = converter;
        }

        public decimal ConvertAmount(decimal amount, string fromCurrency, string toCurrency)
        {
            return currencyConverter.ConvertCurrency(amount, fromCurrency, toCurrency);
        }
    }

    public class ConsoleFunction
    {
        public void ConsoleAppFunctionality()
        {
            Console.Write("Type '1' to use fixed rate for converting currencies and '2' for a live rate: ");
            int chooseTypeOfConverter = int.Parse(Console.ReadLine());
            if (chooseTypeOfConverter == 1)
            {
                CurrencyConversionApp FixedApp = new CurrencyConversionApp(new FixedRateCurrencyConverter());

                Console.Write("Which currency would you like to exchange (EUR,USD,CHF,KYD,GBP,GIP,KWD,BHD,OMR,JOD,PLN)?: ");
                string exchangeFrom = Console.ReadLine();
                Console.Write("To which currency would you like to exchange (EUR,USD,CHF,KYD,GBP,GIP,KWD,BHD,OMR,JOD,PLN)?: ");
                string exchangeTo = Console.ReadLine();
                Console.Write("How much money would you like to exchange: ");
                int amount = int.Parse(Console.ReadLine());

                Console.WriteLine(" ");
                Console.WriteLine($"You exchanged {amount} {exchangeFrom} into {FixedApp.ConvertAmount(amount, exchangeFrom, exchangeTo)} {exchangeTo}");
            }
            else
            {
                CurrencyConversionApp LiveApp = new CurrencyConversionApp(new LiveCurrencyConverter());

                Console.Write("Which currency would you like to exchange (EUR,USD,CHF,KYD,GBP,GIP,KWD,BHD,OMR,JOD,PLN)?: ");
                string exchangeFromLive = Console.ReadLine();
                Console.Write("To which currency would you like to exchange (EUR,USD,CHF,KYD,GBP,GIP,KWD,BHD,OMR,JOD,PLN)?: ");
                string exchangeToLive = Console.ReadLine();
                Console.Write("How much money would you like to exchange: ");
                int amountLive = int.Parse(Console.ReadLine());

                Console.WriteLine(" ");
                Console.WriteLine($"You exchanged {amountLive} {exchangeFromLive} into {LiveApp.ConvertAmount(amountLive, exchangeFromLive, exchangeToLive)} {exchangeToLive}");

            }


            Console.WriteLine(" ");
            Console.Write("Type '1' to go back to the start, type '2' to kill app: ");
            int choose = int.Parse(Console.ReadLine());
            Console.WriteLine(" ");
            if(choose == 1)
            {
                ConsoleAppFunctionality();
            } else
            {
                Environment.Exit(0);
            }
        }
    }

    public class Program
    {
        static void Main()
        {
            ConsoleFunction con = new();
            con.ConsoleAppFunctionality();
        }
    }
}