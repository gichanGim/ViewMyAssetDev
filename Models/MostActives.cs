namespace ViewMyAssetDev.Models
{
    public class MostActives
    {
        public string Symbol {  get; set; }

        public string ShortName { get; set; }
        
        public decimal RegularMarketPrice { get; set; }

        public string Currency {get; set;}

        public string RegularMarketChangePercent { get; set;}

        public bool IsUp {  get; set; }

        public string Domain { get; set; }
    }
}
