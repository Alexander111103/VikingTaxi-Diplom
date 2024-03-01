using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonPrice
    {
        public JsonPrice(string startPrice, string minPrice, string perKmPrice, string perMinPrice)
        {
            StartPrice = Convert.ToInt32(startPrice);
            MinPrice = Convert.ToInt32(minPrice);
            PricePerKm = Convert.ToInt32(perKmPrice);
            PricePerMin = Convert.ToInt32(perMinPrice);
        }

        public int StartPrice { get; private set; }
        public int MinPrice { get; private set; }
        public int PricePerKm { get; private set; }
        public int PricePerMin { get; private set; }
    }
}
