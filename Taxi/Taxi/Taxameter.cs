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
        private int _minPrice;
        private int _timeToGetLocation = 2;

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


                _interval = 60 / price.PricePerMin - 1 - _timeToGetLocation;
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

            _price = ((int)Math.Floor(Math.Max(_price, _minPrice))) / 10 * 10;
        }

        public int GetNowPrice()
        {
            return Convert.ToInt32(Math.Floor(_price));
        }

        public int GetNowKm()
        {
            return _usedKm;
        }

        private async void CountPrice()
        {
            App.Current.Properties.TryGetValue("login", out object login);

            _lastLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(_timeToGetLocation)), new CancellationTokenSource().Token);

            while (IsActive)
            {
                await Task.Delay(new TimeSpan(0, 0, _interval));

                _price += 1;

                _nowLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(_timeToGetLocation)), new CancellationTokenSource().Token);
                DataBaseApi.SetDriverCoordersByLogin($"{login}", _nowLocation.Latitude, _nowLocation.Longitude);

                _metrs += Xamarin.Essentials.Location.CalculateDistance(_lastLocation, _nowLocation, DistanceUnits.Kilometers) * 1000;

                if (((int)Math.Floor(_metrs / 1000) - _usedKm) > 0)
                {
                    _price += _rubelPerKm * ((int)Math.Floor(_metrs / 1000) - _usedKm);
                    _usedKm += (int)Math.Floor(_metrs / 1000) - _usedKm;
                }

                _lastLocation = _nowLocation;
            }
        }
    }
}