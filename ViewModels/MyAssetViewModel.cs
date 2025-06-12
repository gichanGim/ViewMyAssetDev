using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ViewMyAssetDev.ViewModels
{
    public class MyAssetViewModel
    {
        public string Symbol {  get; set; }
        public string ShortName {  get; set; }
        public int Amount {  get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal CurrentPrice { get; set; }

        // ex) USD, KRW....
        public string FinancialCurrency {  get; set; }

        // Equity(주식), Futures(선물 옵션), Forex(환율), Indexs(지수), Mutual Fund(뮤추얼 펀드), ETF, Cryptocurrencies(가상 화폐), Combination(복합 옵션)
        public string Type { get; set; }

        // 해당 통화에 대한 환율
        public decimal ExchangeRate {  get; set; }
    }
}
