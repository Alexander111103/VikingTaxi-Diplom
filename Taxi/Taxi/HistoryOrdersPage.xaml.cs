using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading;
using Android.Graphics;
using Javax.Security.Auth;
using Android.App;
using Android;
using Plugin.LocalNotification;

namespace Taxi
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryOrdersPage : ContentPage
	{
        private FlyoutMenu _flyoutMenu;

        public HistoryOrdersPage(FlyoutMenu menu)
		{
			InitializeComponent ();

            _flyoutMenu = menu;
        }

        public void OpenMenu_Click(object sender, EventArgs e)
        {
            if (_flyoutMenu.IsPresented == false)
            {
                _flyoutMenu.IsPresented = true;
            }
            else
            {
                _flyoutMenu.IsPresented = false;
            }
        }

        public async void testLocation_Click(object sender, EventArgs e)
        {
            //var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(3)), new CancellationTokenSource().Token);
            //await DisplayAlert("Ok", $"{location.Latitude.ToString().Replace(",", ".")},{location.Longitude.ToString().Replace(",", ".")}", "Ok");
            //await location.OpenMapsAsync();

            double test = Location.CalculateDistance(await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)), new CancellationTokenSource().Token), await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)), new CancellationTokenSource().Token), DistanceUnits.Kilometers);
            DisplayAlert("Ok", $"{test}", "Ok");
        }

        public async void uved(object sender, EventArgs e)
        {

        }
    }
}