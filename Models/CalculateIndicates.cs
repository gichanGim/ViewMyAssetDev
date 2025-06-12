using ViewMyAssetDev.Models;

namespace ViewMyAssetDev.Models
{
    public class CalculateIndicates
    {
        public static decimal Normalize(decimal value, decimal min, decimal max, bool higherIsBetter = true)
        {
            if (higherIsBetter)
                return Math.Clamp((value - min) / (max - min), 0, 1);
            else
                return Math.Clamp((max - value) / (max - min), 0, 1);
        }

        public static decimal NormalizeMidOptimal(decimal value, decimal optimal, decimal maxDelta)
        {
            var delta = Math.Abs(value - optimal);
            return Math.Max(0m, 1 - (delta / maxDelta));
        }

        public static List<decimal> GetScores(EconomicData data)
        {
            var scores = new List<decimal>
            {
                Normalize(data.RealGDP, 20000, 26000), // Trillions
                Normalize(data.UnemploymentRate, 3.0m, 10.0m, higherIsBetter: false),
                NormalizeMidOptimal(data.InflationRate, 2.0m, 6.0m),
                NormalizeMidOptimal(data.FederalFunds, 3.0m, 3.0m),
                Normalize(data.ConsumerSentiment, 50, 100),
                Normalize(data.IndustrialProductionTotalIndex, 90, 110),
                Normalize(data.TotalNonfarmPayroll, 140000, 160000), // in Thousands
                Normalize(data.RetailSales, 500000, 700000) // in Million USD
            };

            return scores;
        }

        public static decimal GetTotalScore(List<decimal> scores)
        {
            decimal totalScore = 0;

            totalScore += scores[0] / 4;
            totalScore += scores[1] / 5;
            totalScore += scores[2] / 5;
            totalScore += scores[3] / 10;
            totalScore += scores[4] / 10;
            totalScore += scores[5] / 10;
            totalScore += scores[6] / 20;

            return totalScore;
        }

        public static string GetCurrentState(decimal totalScore)
        {
            if (totalScore < 0.2m)
                return "위기";
            else if (totalScore < 0.4m)
                return "둔화";
            else if (totalScore < 0.6m)
                return "안정";
            else if (totalScore < 0.8m)
                return "성장";
            else
                return "과열";
        }
    }
}
