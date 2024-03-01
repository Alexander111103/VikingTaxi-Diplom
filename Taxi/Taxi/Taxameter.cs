using Java.Util;
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

        public Taxameter()
        {

        }

        public bool IsActive { get; private set; } = false;

        public async void Start()
        {
            if(_isLaunched == false)
            {
                _isLaunched = true;

                JsonPrice price = await DataBaseApi.GetActualPrice();

                _lastLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1)), new CancellationTokenSource().Token);

                _interval = 60 / price.PricePerMin * 1000;
                _rubelPerKm = price.PricePerKm;
            }

            IsActive = true;
            CountPrice();
        }

        public void Stop()
        {
            IsActive = false;
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

                _nowLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1)), new CancellationTokenSource().Token);

                _metrs += Xamarin.Essentials.Location.CalculateDistance(_lastLocation, _nowLocation, DistanceUnits.Kilometers) * 1000;

                if ((_usedKm - Math.Floor(_metrs / 1000)) > 0)
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