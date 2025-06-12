using ViewMyAssetDev.Models;

namespace ViewMyAssetDev.ViewModels
{
    public class MainViewModel
    {
        public List<MostActives> MostActives { get; set; }

        public List<Article> Articles { get; set; }

        public EconomicData EconomicData { get; set; }
    }
}
