using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonTaxiInfo
    {
        public JsonTaxiInfo(string brand, string mark, string color, string numer, string img, string rating, string experience, string name, string lastName, string phone) 
        { 
            Brand = brand;
            Mark = mark;
            Color = color;
            Numer = numer;
            Image = img;
            Rating = string.Format("{0:f2}", Convert.ToDouble(rating.Replace(".", ",")));
            Experience = experience;
            Name = name;
            LastName = lastName;
            Phone = phone;
        }

        public string Brand { get; private set; }
        public string Mark { get; private set; }
        public string Color { get; private set; }
        public string Numer { get; private set; }
        public string Image { get; private set; }
        public string Rating { get; private set; }
        public string Experience { get; private set; }
        public string Name { get; private set; }
        public string LastName { get; private set; }
        public string Phone { get; private set; }
    }
}
