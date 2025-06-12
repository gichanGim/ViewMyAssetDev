namespace ViewMyAssetDev.Models
{
    public class UserStock
    {
        public string Id { get; set; } // User Id (암호화)

        public string Symbol { get; set; }

        public int Amount { get; set; }

        public double TotalPurchasePrice { get; set; }

        // Equity(주식), Futures(선물 옵션), Forex(환율), Indexs(지수), Mutual Fund(뮤추얼 펀드), ETF, Cryptocurrencies(가상 화폐), Combination(복합 옵션)
        public string Type { get; set; }
    }
}
