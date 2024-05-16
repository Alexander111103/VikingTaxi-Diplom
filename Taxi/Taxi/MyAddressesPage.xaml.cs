using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyAddressesPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public MyAddressesPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            map.Source = new HtmlWebViewSource { Html = "<!DOCTYPE html>\r\n<html>\r\n    <head>\r\n        <meta http-equiv=\"Content-Type\" content=\"map/html; charset=utf-8\"/>\r\n        <script src=\"https://api-maps.yandex.ru/2.1/?lang=ru_RU&apikey=a36cde6b-2202-41e4-ac57-34889ffb135f&suggest_apikey=8d505c91-3825-4543-9d6a-3fd3c38c4d40\" type=\"text/javascript\"></script>\r\n        <script src=\"https://yandex.st/jquery/2.2.3/jquery.min.js\" type=\"text/javascript\"></script>\r\n        <style> \r\n            html, body, #map, #upperlayer { \r\n                width: 100%; \r\n                height: 100%; \r\n                padding: 0; \r\n                margin: 0; \r\n                box-sizing: border-box; \r\n                z-index: 0;\r\n                left: 0;\r\n                top: 0;\r\n                position: absolute;\r\n            }\r\n            body{\r\n                position: relative;\r\n            }\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <div id=\"map\"></div>\r\n    </body>\r\n</html>\r\n<script>\r\n    var myMap;\r\n    var coords;\r\n    var placemark\r\n\r\n    ymaps.ready(function () {\r\n        myMap = new ymaps.Map('map', { center: [56.843994, 53.252093], zoom: 13, controls: [] }, { restrictMapArea: [[55.843994,52.252093],[57.843994,54.252093]]});\r\n\r\n\r\n        myMap.events.add('balloonopen', function(){\r\n            myMap.balloon.close();\r\n        });\r\n\r\n        myMap.behaviors.disable('rightMouseButtonMagnifier');\r\n\r\n        myMap.events.add('click', function (e) {\r\n                myMap.geoObjects.remove(placemark);\r\n                coords = e.get('coords');\r\n\r\n                placemark = new ymaps.Placemark(coords, { }, { preset: 'islands#blackDotIcon', hasBalloon: false, hasHint: false });\r\n                myMap.geoObjects.add(placemark);\r\n        });\r\n\r\n    });\r\n\r\n    function GetCoorders()\r\n    {\r\n        return coords;\r\n    }\r\n\r\n    const delay = async (ms) => await new Promise(resolve => setTimeout(resolve, ms));\r\n\r\n</script>\r\n" };
            refreshView.Command = new Command(LoadAddresses);

            LoadAddresses();
        }

        private async void LoadAddresses()
        {
            App.Current.Properties.TryGetValue("login", out object login);
            refreshView.IsRefreshing = true;
            listView.ItemsSource = (await DataBaseApi.GetFavoriteAddressesByLogin($"{login}")).Addresses;
            listView.ItemTapped -= ListViewItemTapped;
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label name = new Label() { FontSize = 25 };
                name.SetBinding(Label.TextProperty, "Name");

                ViewCell viewCell = new ViewCell
                {
                    View = new StackLayout
                    {
                        Children = { name },
                        Padding = new Thickness(0, 10),
                        Margin = new Thickness(5, 0),
                        Orientation = StackOrientation.Vertical
                    }
                };

                return viewCell;
            });
            listView.ItemTapped += ListViewItemTapped;
            refreshView.IsRefreshing = false;
        }

        private async void ListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            JsonFavoriteAddress address = e.Item as JsonFavoriteAddress;
            bool isDelete = await DisplayAlert("Подтверждение", "Вы точно хотите удалить адрес из избранных?", "Да", "Нет");

            if (isDelete)
            {
                DataBaseApi.RemoveFavoriteAddressById(address.Id);

                LoadAddresses();
            }
        }

        private async void AddNewAddress_Click(object sender, EventArgs e)
        {
            string coorders = await map.EvaluateJavaScriptAsync("GetCoorders()");

            if(coorders == null)
            {
                DisplayAlert("Ошибка", "Вы не поставили метку на адрес.", "Ok");
            }
            else
            {
                string name = await DisplayPromptAsync("Название", "Дайте название точке на карте", accept:"Ok", cancel:null, maxLength:50);

                if(name == "" || name == null)
                {
                    DisplayAlert("Ошибка", "Вы не ввели название", "Ok");
                }
                else
                {
                    App.Current.Properties.TryGetValue("login", out object login);

                    DataBaseApi.AddFavoriteAddress($"{login}", name, coorders.Replace('[', ' ').Replace(']', ' ').Trim());

                    LoadAddresses();
                }
            }
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

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}