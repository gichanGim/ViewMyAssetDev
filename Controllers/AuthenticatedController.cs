using Microsoft.AspNetCore.Mvc;
using ViewMyAssetDev.ViewModels;
using ViewMyAssetDev.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace ViewMyAssetDev.Controllers
{
    public class AuthenticatedController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly UserManager<Users> _userManager;
        private readonly DBConnectionRepository _dbrepo;

        public AuthenticatedController(ICompositeViewEngine viewEngine, UserManager<Users> userManager, DBConnectionRepository dbconnectionrepo)
        {
            _viewEngine = viewEngine;
            _userManager = userManager;
            _dbrepo = dbconnectionrepo;
        }

        public async Task<IActionResult> Main()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("UnAuthenticatedMain", "UnAuthenticated");

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
                ViewData["UserName"] = user.UserId;

            var model = new MainViewModel();
            model.MostActives = await YahooFinanceRepository.GetMostActives();

            model.Articles = await YahooFinanceRepository.GetArticles();
            model.EconomicData = await YahooFinanceRepository.GetEconomicData();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Main(FinanceDataViewModel model)
        {
            return View();
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> FinanceData(string symbol)
        {
            var model = await GetFinanceDataModel(symbol);

            string? domain = await YahooFinanceRepository.GetLogoBySymbol(symbol);
            model.Domain = domain;

            return View(model);
        }

        public async Task<FinanceDataViewModel> GetFinanceDataModel(string symbol)
        {
            symbol = symbol.Trim();

            // ----------SearchByCodeName----------
            string json = await YahooFinanceRepository.SearchByCodeName(symbol);
            var jobject = JObject.Parse(json);
            var model = new FinanceDataViewModel();


            foreach (var q in jobject["body"]) // API 응답 구조에 맞게 수정
            {
                if (q["symbol"]?.ToString() == symbol) // symbol이 code와 일치하는 경우만 처리
                {
                    model.Symbol = q["symbol"]?.ToString();

                    if (q is JObject qObj && qObj.TryGetValue("longname", out JToken longnameToken))
                    {
                        model.LongName = longnameToken.ToString();
                    }
                    else
                    {
                        model.LongName = q["shortname"]?.ToString();
                    }

                    model.ExchDisp = q["exchDisp"]?.ToString();
                    model.Type = q["quoteType"]?.ToString();
                }
            }

            //----------GetFluctationDataSnapShots----------(RealTime 데이터 조회 불가능한 유형)
            if (model.Type != "EQUITY")
            {
                json = await YahooFinanceRepository.GetFluctationDataSnapShots(symbol);
                jobject = JObject.Parse(json);
                var k = jobject["body"][0];

                model.NetChange = Convert.ToDecimal(k["regularMarketChange"]).ToString("F2");
                model.PercentageChange = Convert.ToDecimal(k["regularMarketChangePercent"]).ToString("F2");
                model.CurrentPrice = Convert.ToDecimal(k["regularMarketPrice"]);
                model.FinancialCurrency = k["currency"]?.ToString();

                if (Convert.ToDecimal(model.PercentageChange) > 0)
                {
                    model.UpOrDown = "up";
                    model.NetChange = "+" + model.NetChange;
                    model.PercentageChange = "+" + model.PercentageChange + "%";
                }
                else
                {
                    model.UpOrDown = "down";
                    model.NetChange = "-" + model.NetChange;
                    model.PercentageChange = "-" + model.PercentageChange + "%";
                }
            }
            // ----------GetFluctationDataRealTime---------- (RealTime 데이터 조회 가능한 유형)
            else
            {
                json = await YahooFinanceRepository.GetFluctationDataRealTime(symbol);
                jobject = JObject.Parse(json);

                if (!jobject.HasValues) // Equity이면서 RealTime 조회 안되는 경우
                {
                    json = await YahooFinanceRepository.GetFluctationDataSnapShots(symbol);
                    jobject = JObject.Parse(json);
                    var k = jobject["body"][0];

                    model.NetChange = Convert.ToDecimal(k["regularMarketChange"]).ToString("F2");
                    model.PercentageChange = Convert.ToDecimal(k["regularMarketChangePercent"]).ToString("F2");
                    model.CurrentPrice = Convert.ToDecimal(k["regularMarketPrice"]);
                    model.FinancialCurrency = k["currency"]?.ToString();

                    if (Convert.ToDecimal(model.PercentageChange) > 0)
                    {
                        model.UpOrDown = "up";
                        model.NetChange = "+" + model.NetChange;
                        model.PercentageChange = "+" + model.PercentageChange + "%";
                    }
                    else
                    {
                        model.UpOrDown = "down";
                        model.NetChange = "-" + model.NetChange;
                        model.PercentageChange = "-" + model.PercentageChange + "%";
                    }
                }
                else // Equity이면서 RealTime 조회 가능한 경우
                {
                    var k = jobject["body"]["secondaryData"];

                    model.NetChange = k["netChange"]?.ToString();
                    model.PercentageChange = k["percentageChange"]?.ToString();
                    model.UpOrDown = k["deltaIndicator"]?.ToString();
                    string temp = k["lastSalePrice"].ToString().Substring(1);
                    model.CurrentPrice = Convert.ToDecimal(temp);

                    json = await YahooFinanceRepository.GetFluctationDataSnapShots(symbol);
                    jobject = JObject.Parse(json);
                    k = jobject["body"][0];
                    model.FinancialCurrency = k["currency"]?.ToString();
                }
                
            }

            //----------현재 보유 수량----------
            var user = await _userManager.GetUserAsync(User);

            model.MyAmount = _dbrepo.GetStockAmountByIdSymbol(user.Id, symbol);

            HttpContext.Session.SetString("FinanceDataModel", JsonConvert.SerializeObject(model));

            return model;
        }
       
        // autocomplete에서 ajax로 보낸 symbol값을 FinanceData에 보냄 + success 요청 응답
        [HttpPost]
        public async Task<IActionResult> ReturnSuccessAjax(string symbol)
        {
            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("FinanceData", new {symbol = symbol})
            });
        } 

        [HttpGet]
        public async Task<JsonResult> ReturnCodesJson(string code)
        {
            string json = await YahooFinanceRepository.SearchByCodeName(code);   
            var jobject = JObject.Parse(json);

            var results = jobject["body"] // API 응답 구조에 맞게 수정
                .Select(q => new
                {
                    Symbol = q["symbol"]?.ToString(),
                    Name = q["shortname"]?.ToString(),
                    ExchDisp = q["exchDisp"]?.ToString()
                })
                .ToList();

            return Json(results);
        }

        public async Task<IActionResult> MyAsset()
        {
            var user = await _userManager.GetUserAsync(User);

            var stocks = _dbrepo.GetUserStockById(user.Id); // returning List<UserStocks> 

            var symbols = stocks.Select(x => x.Symbol).ToList();
            string json = await YahooFinanceRepository.GetStockInfos(symbols);

            var jobject = JObject.Parse(json);
            var p = jobject["body"];

            HashSet<string> currencies = new HashSet<string>();

            foreach (var obj in p)
                currencies.Add(obj["currency"]?.ToString());

            Dictionary<string, decimal> exchangeRates = await GetExchangeRate(currencies);

            List<MyAssetViewModel> models = new List<MyAssetViewModel>();

            for (int i = 0; i < stocks.Count; i++)
            {
                models.Add(new MyAssetViewModel
                {
                    Symbol = stocks[i].Symbol,
                    Amount = stocks[i].Amount,
                    PurchasePrice = Convert.ToDecimal(stocks[i].TotalPurchasePrice),
                    CurrentPrice = Convert.ToDecimal(p[i]["regularMarketPrice"]) * stocks[i].Amount,
                    ShortName = p[i]["shortName"]?.ToString(),
                    FinancialCurrency = p[i]["currency"]?.ToString(),
                    Type = stocks[i].Type?.ToString(),
                    ExchangeRate = exchangeRates[p[i]["currency"]?.ToString()]
                });
            }

            return View(models);
        }

        

        [HttpPost]
        public async Task<IActionResult> BuyStock(FinanceDataViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            Transaction trans = new Transaction()
            {
                Id = user.Id,
                Symbol = model.Symbol,
                Amount = model.Amount,
                Price = model.PurchasedPrice,
                Type = model.Type,
                TransactionType = "BUY"
            };


            await _dbrepo.BuyTransaction(trans);

            return RedirectToAction("FinanceData", new { symbol = model.Symbol, UporDown = model.UpOrDown});
        }

        [HttpPost]
        public async Task<IActionResult> SellStock(FinanceDataViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!_dbrepo.SellAvailable(user.Id, model.Symbol, model.Amount))
            {
                ModelState.AddModelError("", "보유 수량이 매도 수량보다 적거나 없습니다");

                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            } 

            Transaction trans = new Transaction()
            {
                Id = user.Id,
                Symbol = model.Symbol,
                Amount = model.Amount,
                Price = model.PurchasedPrice,
                Type = model.Type,
                TransactionType = "SELL"
            };

            await _dbrepo .SellTransaction(trans);

            return Json(new { success = true, redirectUrl = Url.Action("FinanceData", new { symbol = model.Symbol, UporDown = model.UpOrDown }) });
        }

        /// <summary>
        /// 기간 별 차트 데이터를 chart.js에 전달
        /// </summary>
        /// <param name="type">조회 할 기간 단위</param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> GetChartData(string type)
        {
            var sessionModel = HttpContext.Session.GetString("FinanceDataModel");
            var model = JsonConvert.DeserializeObject<FinanceDataViewModel>(HttpContext.Session.GetString("FinanceDataModel"));

            string json = await YahooFinanceRepository.GetChartDataByType(model.Symbol, type);
            var data = JObject.Parse(json)["body"];

            List<ChartData> chartdata = new List<ChartData>();

            foreach (var key in data.Children<JProperty>())
            {
                var entry = key.Value;
                 
                chartdata.Add(new ChartData
                {
                    Date = ChangeDate(entry["date"]?.ToString()),
                    High = Convert.ToDecimal(entry["high"]).ToString("F2"),
                    Low = Convert.ToDecimal(entry["low"]).ToString("F2"),
                    Close = Convert.ToDecimal(entry["close"]).ToString("F2"),
                }); 
            }

            return Json(chartdata);
        }

        /// <summary>
        /// 입력 통화 -> KRW 환율 반환
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, decimal>> GetExchangeRate(HashSet<string> currencies)
        {
            List<string> exchanges = new List<string>();
            Dictionary<string, decimal> exchangerates = new Dictionary<string, decimal>();

            foreach (string str in currencies)
            {
                string temp = str + "KRW=X";
                exchanges.Add(temp);
            }

            string json = await YahooFinanceRepository.GetStockInfos(exchanges);
            var p = JObject.Parse(json)["body"];

            int i = 0;

            foreach (var item in currencies)
            {
                var temp = p[i]["regularMarketPrice"];
                decimal rate = Convert.ToDecimal(p[i]["regularMarketPrice"]);

                exchangerates.Add(item, rate);

                i++;
            }

            return exchangerates;
        }

        public string ChangeDate(string date)
        {
            string[] temp = date.Split('-');
            string[] result = new string[3];

            for (int i = 0; i < 3; i++)
                result[i] = temp[2 - i];
            
            return string.Join("-", result);
        }
    }
}
// autocomplete 요소 누르는 동시에 2개의 api endpoint 호출(search, financial-data)하여 viewmodel에 종목 정보 저장 후 cmhtml로 model 전달
