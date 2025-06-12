namespace ViewMyAssetDev.Models
{
    public class EconomicData
    {
        // 실질 GDP 성장률
        public decimal RealGDP { get; set; }    

        // 실업률
        public decimal UnemploymentRate { get; set; }

        // 인플레이션율
        public decimal InflationRate { get; set; }

        // 기준 금리
        public decimal FederalFunds { get; set; }

        // 비농업 신규 고용자지수
        public decimal TotalNonfarmPayroll { get; set; }

        // 소비자 신뢰지수
        public decimal ConsumerSentiment { get; set; }

        // 산업생산지수
        public decimal IndustrialProductionTotalIndex { get; set; }

        // 소매 판매액
        public decimal RetailSales { get; set; }
    }
}
