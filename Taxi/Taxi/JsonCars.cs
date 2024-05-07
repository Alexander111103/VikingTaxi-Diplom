using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonCar
    {
        public JsonCar(string id_car, string owner_car, string brand_car, string mark_car, string color_car, string numer_car, string rate_car, string isChildSeet_car, string img_car)
        {
            Id = Convert.ToInt32(id_car);
            Owner = Convert.ToInt32(owner_car);
            Brand = brand_car;
            Mark = mark_car;
            Color = color_car;
            Number = numer_car;
            Rate = rate_car;
            IsChildSeet = isChildSeet_car;
            Img = img_car;
        }

        public int Id { get; private set; }
        public int Owner { get; private set; }
        public string Brand { get; private set; }
        public string Mark { get; private set; }
        public string Color { get; private set; }
        public string Number { get; private set; }
        public string Rate { get; private set; }
        public string IsChildSeet { get; private set; }
        public string Img { get; private set; }
        
    }

    public class JsonCars
    {
        public JsonCars(List<JsonCar> cars)
        {
            Cars = cars;
        }

        public List<JsonCar> Cars { get; private set; }
    }
}
