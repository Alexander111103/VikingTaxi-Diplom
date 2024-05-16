using System;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickAutoDriverPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public PickAutoDriverPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            SetListView();

            refreshView.Command = new Command(SetListView);
        }

        private void OpenMenu_Click(object sender, EventArgs e)
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

        private async void SetListView()
        {
            refreshView.IsRefreshing = true;
            App.Current.Properties.TryGetValue("login", out object login);
            listView.ItemsSource = (await DataBaseApi.GetDriverCarsByDriverLogin($"{login}")).Cars;
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label color = new Label() { FontSize = 15 };
                color.SetBinding(Label.TextProperty, "Color");
                Label brand = new Label() { FontSize = 15 };
                brand.SetBinding(Label.TextProperty, "Brand");
                Label mark = new Label() { FontSize = 15 };
                mark.SetBinding(Label.TextProperty, "Mark");
                Label number = new Label() { FontSize = 12 };
                number.SetBinding(Label.TextProperty, "Number");
                Label rate = new Label() { FontSize = 12 };
                rate.SetBinding(Label.TextProperty, "Rate", stringFormat:"Класс автомобиля: {0}");

                StackLayout info = new StackLayout 
                { 
                    Children = {color, brand, mark, number, rate},
                    Padding = new Thickness(0, 5),
                    Orientation = StackOrientation.Vertical
                };
                AbsoluteLayout.SetLayoutBounds(info, new Rectangle(0.45, 0.1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(info, AbsoluteLayoutFlags.PositionProportional);

                Image img = new Image() { WidthRequest = 90, HeightRequest = 90  };
                img.SetBinding(Image.SourceProperty, "Img", stringFormat:"http://taxiviking.ru/media/img/cars/{0}");
                AbsoluteLayout.SetLayoutBounds(img, new Rectangle(0.05, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.PositionProportional);

                ViewCell viewCell = new ViewCell
                {
                    View = new AbsoluteLayout
                    {
                        Children = { img, info }
                    }
                };

                return viewCell;
            });
            listView.ItemTapped += ListViewItemTapped;
            refreshView.IsRefreshing = false;
        }

        private async void ListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            JsonCar car = e.Item as JsonCar;

            if (car != null)
            {
                bool isAgree = await DisplayAlert("Подтверждение", $"Начать работу на:\n{car.Color} {car.Brand} {car.Mark} {car.Number}", "Подтвердить", "Выбрать другой");

                if (isAgree)
                {
                    Location location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)));

                    DataBaseApi.SetDriverStatusToSearchById(car.Owner, car.Id, $"{location.Latitude.ToString().Replace(",", ".")},{location.Longitude.ToString().Replace(",", ".")}");
                    _flyoutMenu.DriverState = "search";
                    _flyoutMenu.SearchOrderDriverPage = new SearchOrderDriverPage(_flyoutMenu);
                    _flyoutMenu.Detail = new NavigationPage(_flyoutMenu.SearchOrderDriverPage);
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}