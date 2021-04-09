namespace EnergyMap.Classes
{
    public class RegionData
    {
        public RegionData() { }

        //название региона
        public string Name { get; set; }

        //объем выработки
        public double ProdVolume { get; set; }

        //себестоимость выработки
        public double ProdPrice { get; set; }

        //объем потребления
        public double ConsVolume { get; set; }

        //разница между выработкой и потреблением
        public double ProdConsDif { get; set; }
    }
}