using System;
using System.Threading;
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
        private int _timeToGetLocation = 2;
        private int _minPrice;

        public Taxameter()
        {

        }

        public bool IsActive { get; private set; } = false;

        public async Task<string> Start()
        {
            if(_isLaunched == false)
            {
                _isLaunched = true;

                JsonPrice price = await DataBaseApi.GetActualPrice();

                _lastLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(_timeToGetLocation)), new CancellationTokenSource().Token);

                _interval = (60 / price.PricePerMin - _timeToGetLocation) * 1000;
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
            while (IsActive)
            {
                _price += 1;

                _nowLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(_timeToGetLocation)), new CancellationTokenSource().Token);

                _metrs += Xamarin.Essentials.Location.CalculateDistance(_lastLocation, _nowLocation, DistanceUnits.Kilometers) * 1000;

                if ((Math.Floor(_metrs / 1000) - _usedKm) > 0)
                {
                    _usedKm += Convert.ToInt32(_usedKm - Math.Floor(_metrs / 1000));
                    _price += _rubelPerKm;
                }

                _lastLocation = _nowLocation;

                await Task.Delay(_interval);
            }
        }
    }
}