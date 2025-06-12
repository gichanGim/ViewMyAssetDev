using Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using MatthiWare.FinancialModelingPrep;
using static Azure.Core.HttpHeader;

namespace ViewMyAssetDev.Models
{
    public class YahooFinanceRepository
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// 해당 종목 코드에 대한 상세 정보 Json으로 return
        /// </summary>
        /// <param name="code">검색 할 코드명</param>
        /// <returns></returns>
        public static async Task<string> GetFinancialData(string code)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/stock/modules?ticker={code}&module=financial-data"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(); // 해당 코드에 대한 상세 정보

                return json;
            }
        }

        /// <summary>
        /// 입력값과 관련된 모든 종목 코드 Json으로 return
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static async Task<string> SearchByCodeName(string str)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/search?search={str}"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return json;
            }
        }

        // string[0] = Symbol, string[1] = ShortName
        public static async Task<string> GetStockInfos(List<string> sybmols)
        {
            string uri = string.Join("%2C", sybmols);
            uri = MakeUri(uri);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/stock/quotes?ticker={uri}"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return json;
            }
        }

        public static async Task<string> GetChartDataByType(string symbol, string type)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/stock/history?symbol={symbol}&interval={type}&diffandsplits=false"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return json;
            }
        }

        public static async Task<string> GetLogoBySymbol(string symbol)
        {
            var apiKey = "FZzGxtp2BrGO43awD93Gih3TrUjPUPGt";
            var url = $"https://financialmodelingprep.com/stable/profile?symbol={symbol}&apikey={apiKey}";
                
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(url);
                var jArray = JArray.Parse(response);

                if (jArray.Count == 0)
                    return "null";

                var body = jArray[0];

                var image = body["image"]?.ToString();

                    return image;
            }
        }

        public static async Task<List<string>> GetLogoBySymbolList(List<string> symbols)
        {
            var apiKey = "FZzGxtp2BrGO43awD93Gih3TrUjPUPGt";
            var joinedSymbols = string.Join(",", symbols);
            var url = $"https://financialmodelingprep.com/api/v3/profile/symbol={joinedSymbols}?apikey={apiKey}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            var jArray = JArray.Parse(response);

            List<string> profiles = new List<string>();

            foreach (var item in jArray)
                profiles.Add(item["image"]?.ToString());
            

            if (profiles == null)
                return null;

            return profiles;
        }

        public static async Task<List<Article>> GetArticles()
        {
            var apiKey = "FZzGxtp2BrGO43awD93Gih3TrUjPUPGt";
            var url = $"https://financialmodelingprep.com/stable/news/general-latest?page=0&limit=20&apikey={apiKey}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(url);
                var jArray = JArray.Parse(response);

                List<Article> articles = new List<Article>();

                foreach (var p in jArray.Take(5))
                {
                    articles.Add(new Article
                    {
                        PublishedDate = p["publishedDate"]?.ToString(),
                        Publisher = p["publisher"]?.ToString(),
                        Title = p["title"]  ?.ToString(),
                        ImageUrl = p["image"]?.ToString(),
                        ArticleUrl = p["url"]?.ToString()
                    });
                }

                return articles;
            }
        }

        public static async Task<List<MostActives>> GetMostActives()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://yahoo-finance15.p.rapidapi.com/api/v1/markets/screener?list=most_actives"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var jObject = JObject.Parse(json)["body"];

                List<MostActives> list = new List<MostActives>();

                foreach (var p in jObject.Take(12))
                {
                    list.Add(new MostActives
                    {
                        Symbol = p["symbol"]?.ToString(),
                        ShortName = p["shortName"]?.ToString(),
                        RegularMarketPrice = Math.Round(Convert.ToDecimal(p["regularMarketPrice"]), 2),
                        Currency = p["currency"]?.ToString(),
                        RegularMarketChangePercent = Math.Round(Convert.ToDecimal(p["regularMarketChangePercent"]), 2).ToString(),
                        IsUp = Convert.ToDecimal(p["regularMarketChangePercent"]) >= 0 ? true : false,
                        Domain = await GetLogoBySymbol(p["symbol"]?.ToString())
                    });
                }

                return list;
            }
        }

        /// <summary>
        /// 주가 상승 or 하락 정보 가져오기(RealTime)
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetFluctationDataRealTime(string symbol)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/quote?ticker={symbol}&type=STOCKS"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                
                return json;
            }
        }

        /// <summary>
        /// 주가 상승 or 하락 정보 가져오기(SnapShots)
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetFluctationDataSnapShots(string symbol)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yahoo-finance15.p.rapidapi.com/api/v1/markets/stock/quotes?ticker={symbol}"),
                Headers =
                {
                    { "x-rapidapi-key", "32603ea9f9mshe73eee6d6a3264ap12e988jsnad9da7aa51f8" },
                    { "x-rapidapi-host", "yahoo-finance15.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return json;
            }
        }

        public static async Task<EconomicData> GetEconomicData()
        {
            var indicates = new[] { "realGDP", "unemploymentRate", "inflationRate", "federalFunds", "totalNonfarmPayroll", "consumerSentiment", "industrialProductionTotalIndex", "retailSales" };

            var tasks = indicates.Select(x => GetDataAsync(x)).ToArray();

            var results = await Task.WhenAll(tasks);

            var data = new EconomicData
            {
                RealGDP = ExtractFirstValue(results[0]),
                UnemploymentRate = ExtractFirstValue(results[1]),
                InflationRate = ExtractFirstValue(results[2]),
                FederalFunds = ExtractFirstValue(results[3]),
                TotalNonfarmPayroll = ExtractFirstValue(results[4]),
                ConsumerSentiment = ExtractFirstValue(results[5]),
                IndustrialProductionTotalIndex = ExtractFirstValue(results[6]),
                RetailSales = ExtractFirstValue(results[7])

            };

            return data;
        }

        public static decimal ExtractFirstValue(string json)
        {
            var array = JArray.Parse(json);
            return array.Count > 0 ? array[0]["value"].Value<decimal>() : 0m;
        }

        public static async Task<string> GetDataAsync(string indicate)
        {
            using (HttpClient client = new HttpClient())
            {
                string apikey = "FZzGxtp2BrGO43awD93Gih3TrUjPUPGt";

                string url = $"https://financialmodelingprep.com/stable/economic-indicators?name={indicate}&apikey={apikey}";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static string MakeUri(string uri)
        {
            uri.Replace("^", "%5E");
            uri.Replace("=", "%3D");

            return uri;
        }
    }
}

//var response = await _httpClient.GetStringAsync(url);
//var json = JObject.Parse(response);
//
//var results = json["quotes"] // API 응답 구조에 맞게 수정
//    .Select(q => new
//    {
//        Symbol = q["symbol"]?.ToString(),
//        Name = q["shortname"]?.ToString()
//    })
//    .ToList();
//
//return Json(results);
