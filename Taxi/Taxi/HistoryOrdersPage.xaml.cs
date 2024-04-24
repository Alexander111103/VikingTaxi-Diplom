using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading;

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

            alert.Source = new HtmlWebViewSource { Html="<button onclick=\"confirm('6')\">X</button>" };
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

        public void testf_Click(object sender, EventArgs e)
        {
            Entry entry = new Entry() { Text = ((Button)sender).Text, HeightRequest = ((Button)sender).HeightRequest, WidthRequest = ((Button)sender).WidthRequest, VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center };
            entry.Unfocused += testf_Chendged;

            testf.Children.Remove(((Button)sender));
            testf.Children.Add(entry);

            entry.Focus();
        }

        public void testf_Chendged(object sender, EventArgs e)
        {
            Button button = new Button() { Text = ((Entry)sender).Text, HeightRequest = ((Entry)sender).HeightRequest, WidthRequest = ((Entry)sender).WidthRequest, BackgroundColor = Color.White, TextColor = Color.Black, BorderColor = Color.Black };
            button.Clicked += testf_Click;

            testf.Children.Remove(((Entry)sender));
            testf.Children.Add(button);
        }

        public async void testLocation_Click(object sender, EventArgs e)
        {
            //var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(3)), new CancellationTokenSource().Token);
            //await DisplayAlert("Ok", $"{location.Latitude.ToString().Replace(",", ".")},{location.Longitude.ToString().Replace(",", ".")}", "Ok");
            //await location.OpenMapsAsync();

            double test = Location.CalculateDistance(await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)), new CancellationTokenSource().Token), await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)), new CancellationTokenSource().Token), DistanceUnits.Kilometers);
            DisplayAlert("Ok", $"{test}", "Ok");
        }
    }
}