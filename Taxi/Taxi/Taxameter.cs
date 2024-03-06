using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Taxi
{
    public class Taxameter
    {
        private bool _isLaunched = false;
        private int _interval;
        private float _rubelPerKm;
        private float _price = 0;
        private int _usedKm = 0;
        private double _metrs = 0;
        private Xamarin.Essentials.Location _nowLocation;
        private Xamarin.Essentials.Location _lastLocation;
        private int _minPrice;
        private int _idOrder;

        public Taxameter(int idOrder)
        {
            _idOrder = idOrder;
        }

        public bool IsActive { get; private set; } = false;

        public async Task<string> Start()
        {
            if(_isLaunched == false)
            {
                _isLaunched = true;

                JsonPrice price = await DataBaseApi.GetActualPrice();

                _lastLocation = GetLocationByCoorders(await DataBaseApi.GetDriverCoordersByIdOrder(_idOrder));

                _interval = 60 / price.PricePerMin;
                _rubelPerKm = price.PricePerKm;
                _minPrice = price.MinPrice;

                _price = price.StartPrice;
            }

            IsActive = true;
            CountPrice();

            return null;
        }

        public void Stop()
        {
            IsActive = false;
        }

        public void End()
        {
            IsActive = false;

            _price = Math.Max(_price, _minPrice);
        }

        public int GetNowPrice()
        {
            return Convert.ToInt32(Math.Floor(_price));
        }

        private async void CountPrice()
        {
            DateTime timeWithInterval = DateTime.Now.Add(new TimeSpan(0, 0, _interval));

            while (IsActive)
            {
                if (timeWithInterval.Subtract(DateTime.Now).Seconds <= 0)
                {
                    _price += 1;

                    _nowLocation = GetLocationByCoorders(await DataBaseApi.GetDriverCoordersByIdOrder(_idOrder));

                    _metrs += Xamarin.Essentials.Location.CalculateDistance(_lastLocation, _nowLocation, DistanceUnits.Kilometers) * 1000;

                    if (((int)Math.Floor(_metrs / 1000) - _usedKm) > 0)
                    {
                        _price += _rubelPerKm * ((int)Math.Floor(_metrs / 1000) - _usedKm);
                        _usedKm += (int)Math.Floor(_metrs / 1000) - _usedKm;
                    }

                    _lastLocation = _nowLocation;

                    timeWithInterval = DateTime.Now.Add(new TimeSpan(0, 0, _interval));
                }
            }
        }

        private Location GetLocationByCoorders(string coorders)
        {
            string[] coordersArray = coorders.Split(',');

            return new Location(Convert.ToDouble(coordersArray[0].Trim().Replace('.', ',')), Convert.ToDouble(coordersArray[1].Trim().Replace('.', ',')));
        }
    }
}