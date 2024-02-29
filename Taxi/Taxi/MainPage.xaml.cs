using System;
using System.Net;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Taxi
{
    public partial class MainPage : ContentPage
    {
        public Label SearchLabel;
        private Button _buttonOrderTaxi;
        private FlyoutMenu _flyoutMenu;
        private Button _fastSearchButton;
        private Button _upDownInfoFrame;
        private Label _taxiInfo;
        private Image _taxiImage;
        private Label _driverInfo;
        private Label _driverExpirience;
        private Label _fastSearchInfo;
        private FlexLayout _buttonOnWaitingDriver = new FlexLayout 
        { 
            WidthRequest = 300,
            HorizontalOptions = LayoutOptions.Center,
            Direction = FlexDirection.Row,
            Margin = new Thickness(10, 0, 0, 0)
        };
        private ImageButton _phoneCallDriver;
        private ImageButton _cancelOrder;

        public JsonRouteInfo RouteInfo;
        public string State = "Order";

        private string _rate = "base";
        private string _priority = "0";
        private string _price;
        private string _paymentType = "cash";
        private int _idOrder;
        private string _phoneDriver;
        private bool _isFastSearch = false;

        public MainPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            SetAndroidOption();
            
            map.Cookies = new CookieContainer();

            map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
            map.Cookies.Add(new Cookie("CoordersFrom", "", "/", "taxiviking.ru"));
            map.Cookies.Add(new Cookie("CoordersTo", "", "/", "taxiviking.ru"));
        }

        public async void RouteAgree_Click(object sender, EventArgs e)
        {
            await Task.Delay(500);

            RouteInfo = JsonConvert.DeserializeObject<JsonRouteInfo>(Convert.ToString(await map.EvaluateJavaScriptAsync("GetRouteInfo()")).Replace(@"\", ""));

            if (RouteInfo.Distance != null && RouteInfo.Distance != "")
            {
                _price = GetPrice(RouteInfo.Distance, RouteInfo.DurationInTraffic);

                routeAgreeButton.Clicked -= RouteAgree_Click;

                await AnimateHeightInfoFrame(200, 500);

                infoLabel.Text = $"Время в пути: {RouteInfo.DurationInTraffic}, Растояние: {RouteInfo.Distance}";
                priceLabel.Text = $"Цена: {_price} рублей.";

                routeAgreeButton.Text = "Изменить маршрут";
                routeAgreeButton.Clicked += RouteReset_Click;

                rateButtons.Margin = new Thickness(0, 0, 0, 0);

                _buttonOrderTaxi = new Button()
                {
                    Text = "Заказать такси",
                    BackgroundColor = Color.FromHex("#2196F3"),
                    TextColor = Color.White
                };

                _buttonOrderTaxi.Clicked += OrderTaxi_Click;

                infoStackLayout.Children.Add(_buttonOrderTaxi);

                map.Cookies.Add(new Cookie("CoordersFrom", Uri.EscapeDataString(RouteInfo.StartCoorders), "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersTo", Uri.EscapeDataString(RouteInfo.FinishCoorders), "/", "taxiviking.ru"));
            }
            else
            {
                await map.EvaluateJavaScriptAsync("EditRoute()");
                await DisplayAlert("Ошибка", "Вы не указали маршрут", "OK");
            }
        }

        public async void RouteReset_Click(object sender, EventArgs e)
        {
            infoLabel.Text = "";
            priceLabel.Text = "";

            await map.EvaluateJavaScriptAsync("EditRoute()");

            rateButtons.Margin = new Thickness(0, -500, 0, 0);

            routeAgreeButton.Text = "Подтвердить маршрут";
            routeAgreeButton.Clicked -= RouteReset_Click;

            infoStackLayout.Children.Remove(_buttonOrderTaxi);

            await AnimateHeightInfoFrame(30, 500);

            routeAgreeButton.Clicked += RouteAgree_Click;
        }

        public async void OrderTaxi_Click(object sender, EventArgs e)
        {
            SetSearchStatus();

            SearchOnMap();

            SetOptionsInfoFrameOnSearch();

            _idOrder = await DataBaseApi.AddOrderTaxi(RouteInfo.Distance, RouteInfo.Duration, RouteInfo.DurationInTraffic, RouteInfo.StartShort, RouteInfo.FinishShort, RouteInfo.StartLong, RouteInfo.FinishLong, RouteInfo.StartCoorders, RouteInfo.FinishCoorders, _priority, _price, _rate, _paymentType);
            _flyoutMenu.SearchTaxi(_idOrder);
        }

        public bool OrderTaxiTimerTick()
        {
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)_flyoutMenu.Timer.ElapsedMilliseconds);
            SearchLabel.Text = $"Поиск...\n{new DateTimeOffset(2024, 1, 1, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss")}";
            return _flyoutMenu.IsTimerStart;
        }

        public void OpenMenu_Click(object sender, EventArgs e)
        {   
            if(_flyoutMenu.IsPresented == false)
            {
                _flyoutMenu.IsPresented = true;
            }
            else
            {
                _flyoutMenu.IsPresented = false;
            }
        }

        public void RateEco_Click(object sender, EventArgs e)
        {
            if(_rate != "base")
            {
                rateEco.BackgroundColor = Color.FromHex("#2196f3");
                rateEco.TextColor = Color.White;

                rateBusiness.BackgroundColor = Color.White;
                rateBusiness.TextColor = Color.Black;

                rateChild.BackgroundColor = Color.White;
                rateChild.TextColor = Color.Black;

                if(_rate == "business")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) - 100);
                }
                if (_rate == "child")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) - 50);
                }

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "base";
                _priority = "0";
            }
        }

        public void RateBusiness_Click(object sender, EventArgs e)
        {
            if (_rate != "business")
            {
                rateEco.BackgroundColor = Color.White;
                rateEco.TextColor = Color.Black;

                rateBusiness.BackgroundColor = Color.FromHex("#2196f3");
                rateBusiness.TextColor = Color.White;

                rateChild.BackgroundColor = Color.White;
                rateChild.TextColor = Color.Black;

                if (_rate == "base")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) + 100);
                }
                if (_rate == "child")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) + 50);
                }

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "business";
                _priority = "1";
            }
        }

        public void RateChild_Click(object sender, EventArgs e)
        {
            if (_rate != "child")
            {
                rateEco.BackgroundColor = Color.White;
                rateEco.TextColor = Color.Black;

                rateBusiness.BackgroundColor = Color.White;
                rateBusiness.TextColor = Color.Black;

                rateChild.BackgroundColor = Color.FromHex("#2196f3");
                rateChild.TextColor = Color.White;

                if (_rate == "base")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) + 50);
                }
                if (_rate == "business")
                {
                    _price = Convert.ToString(Convert.ToInt32(_price) - 50);
                }

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "child";
                _priority = "0";
            }
        }

        public async void FastSearch_Click(object sender, EventArgs e)
        {
            bool isAgree = false;

            isAgree = await DisplayAlert("Быстрый Поиск", "цена: +50 рублей\n\nТакси найдется быстрее", "Принять", "Отказаться");

            if (isAgree)
            {
                _fastSearchButton.Clicked -= FastSearch_Click;
                fast.Children.Clear();

                _isFastSearch = true;

                DataBaseApi.FastSearch(_idOrder);
            }
        }

        public async void WaitingDriverOnMap(string driverCoorders)
        {
            if (_flyoutMenu.PageNumber == 0)
            {
                map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersFrom", Uri.EscapeDataString(driverCoorders), "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersTo", Uri.EscapeDataString(RouteInfo.StartCoorders), "/", "taxiviking.ru"));

                await map.EvaluateJavaScriptAsync($"TaxiDriverGoToUser('{driverCoorders}','{RouteInfo.StartCoorders}')");

                await Task.Delay(1000);
                string timeWaitingDriver = await map.EvaluateJavaScriptAsync("GetTimeWaitingDriver()");

                SearchLabel.Text = "Время ожидания такси: " + timeWaitingDriver;
            }
        }

        public void SetSearchStatus()
        {
            State = "Search";

            if (_flyoutMenu.PageNumber == 0)
            {
                map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersFrom", Uri.EscapeDataString(RouteInfo.StartCoorders), "/", "taxiviking.ru"));
            }
        }

        public async void SearchOnMap()
        {
            if (_flyoutMenu.PageNumber == 0)
            {
                await map.EvaluateJavaScriptAsync($"RemoveRouteBilder()");
                await map.EvaluateJavaScriptAsync($"RemoveTaxiDriverGoToUserOnMap()");
                await map.EvaluateJavaScriptAsync($"SearchViewOnce('{RouteInfo.StartCoorders}')");
            }
        }

        public void SetWaitingUserStatus(string driverCoorders)
        {
            State = "WaitingUser";

            if (_flyoutMenu.PageNumber == 0)
            {
                map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersFrom", Uri.EscapeDataString(driverCoorders), "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersTo", Uri.EscapeDataString(RouteInfo.StartCoorders), "/", "taxiviking.ru"));
            }
        }

        public async void WaitingUserOnMap(string driverCoorders)
        {
            if (_flyoutMenu.PageNumber == 0)
            {
                await map.EvaluateJavaScriptAsync($"RemoveRouteBilder()");
                await map.EvaluateJavaScriptAsync($"WaitingUserView('{RouteInfo.StartCoorders}', '{driverCoorders}')");
            }
        }

        public void SetDriveStatus(string driverCoorders)
        {
            State = "Drive";

            if(_flyoutMenu.PageNumber == 0)
            {
                map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersFrom", Uri.EscapeDataString(driverCoorders), "/", "taxiviking.ru"));
                map.Cookies.Add(new Cookie("CoordersTo", Uri.EscapeDataString(RouteInfo.FinishCoorders), "/", "taxiviking.ru"));
            }
        }

        public async void DriveOnMap(string driverCoorders)
        {
            if (_flyoutMenu.PageNumber == 0)
            {
                await map.EvaluateJavaScriptAsync($"RemoveRouteBilder()");
                await map.EvaluateJavaScriptAsync($"TaxiDriverDrive('{driverCoorders}', '{RouteInfo.FinishCoorders}')");
            }
        }

        public async void SetOptionsInfoFrameOnSearch()
        {
            infoStackLayout.Children.Clear();

            if (_flyoutMenu.PageNumber == 0)
            {
                await AnimateHeightInfoFrame(70, 500);
            }
            else
            {
                infoFrame.HeightRequest = 70;
            }

            SearchLabel = new Label()
            {
                Text = "Поиск...\n00:00:00",
                Margin = new Thickness(0, 0, 0, 0),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 30
            };

            infoStackLayout.Children.Add(SearchLabel);

            _cancelOrder = new ImageButton()
            {
                HeightRequest = 75,
                WidthRequest = 75,
                CornerRadius = 50,
                BackgroundColor = Color.White,
                Source = ImageSource.FromResource("Taxi.Images.cancel.png"),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFill,
                Margin = new Thickness(260, -85, 0, 0),
            };

            infoStackLayout.Children.Add(_cancelOrder);

            if (!_isFastSearch)
            {
                fastFrame.WidthRequest = 185;
                fastFrame.HeightRequest = 45;

                _fastSearchButton = new Button()
                {
                    Text = "Срочный поиск",
                    FontSize = 16,
                    BackgroundColor = Color.FromHex("#293133"),
                    TextColor = Color.White,
                    ImageSource = ImageSource.FromResource("Taxi.Images.fastSearchHorseUWP.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10),
                    CornerRadius = 10
                };

                _fastSearchButton.Clicked += FastSearch_Click;

                if (Device.RuntimePlatform == Device.Android)
                {
                    _fastSearchButton.FontSize = 14;
                    _fastSearchButton.ImageSource = ImageSource.FromResource("Taxi.Images.fastSearchHorseAndroid.png");
                }

                fast.Children.Add(_fastSearchButton);
            }
        }

        public async void SetOptionsInfoFrameOnWaitingDriver(JsonTaxiInfo taxiInfo)
        {
            if (_flyoutMenu.PageNumber == 0)
            {
                await AnimateHeightInfoFrame(180, 500);
            }
            else
            {
                infoFrame.HeightRequest = 180;
            }

            fast.Children.Clear();

            SearchLabel.FontSize = 15;
            SearchLabel.Margin = new Thickness(0, 0, 0, 15);

            _upDownInfoFrame = new Button 
            { 
                Text = "^",
                FontSize = 35,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.White,
                TextColor = Color.Black,
                Margin = new Thickness(0, -20, 0, 0),
                HeightRequest = 50
            };

            _upDownInfoFrame.Clicked += UpInfoFrameOnWaitingDriver;

            _taxiInfo = new Label
            {
                Text = $"{taxiInfo.Color}   {taxiInfo.Brand.ToUpper()} {taxiInfo.Mark}        {taxiInfo.Numer.ToLower()}",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };

            _phoneCallDriver = new ImageButton
            {
                HeightRequest = 75,
                WidthRequest = 75,
                CornerRadius = 50,
                BackgroundColor= Color.White,
                Source = ImageSource.FromResource("Taxi.Images.phone.png"),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFill
            };

            _phoneDriver = taxiInfo.Phone;

            _cancelOrder = new ImageButton
            {
                HeightRequest = 75,
                WidthRequest = 75,
                CornerRadius = 50,
                BackgroundColor = Color.White,
                Source = ImageSource.FromResource("Taxi.Images.cancel.png"),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFill
            };

            _taxiImage = new Image
            {
                Source = new UriImageSource { Uri = new Uri($"http://taxiviking.ru/media/img/cars/{taxiInfo.Image}") },
                WidthRequest = 300,
                HeightRequest = 210,
                Aspect = Aspect.AspectFit
            };

            _driverInfo = new Label
            {
                Text = "Водитель: " + taxiInfo.LastName +  " " + taxiInfo.Name,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };

            _driverExpirience = new Label
            {
                Text = "Рейтинг: " + taxiInfo.Rating + "        Водительский стаж: " + taxiInfo.Experience,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };

            _fastSearchInfo = new Label 
            {
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.DarkGray
            };

            if (_isFastSearch)
            {
                _fastSearchInfo.Text = "Вы воспользовались срочным поиском";
            }

            _buttonOnWaitingDriver.Children.Clear();
            _buttonOnWaitingDriver.Children.Add(_phoneCallDriver);
            _buttonOnWaitingDriver.Children.Add(new Label { WidthRequest = 160 });
            _buttonOnWaitingDriver.Children.Add(_cancelOrder);

            infoStackLayout.Children.Remove(SearchLabel);
            infoStackLayout.Children.Add(_upDownInfoFrame);
            infoStackLayout.Children.Add(_taxiInfo);
            infoStackLayout.Children.Add(SearchLabel);
            infoStackLayout.Children.Add(_buttonOnWaitingDriver);
        }

        public void SetOptionsInfoFrameOnWaitingUser()
        {
            SearchLabel.Text = "Вас ожидает такси";
        }

        public async void SetOptionsInfoFrameOnDrive()
        {
            infoStackLayout.Children.Clear();

            if (_flyoutMenu.PageNumber == 0)
            {
                await AnimateHeightInfoFrame(70, 500);
            }
            else
            {
                infoFrame.HeightRequest = 70;
            }

            SearchLabel = new Label()
            {
                Text = "Таксометр запущен: " + "0" + " Руб.",
                Margin = new Thickness(0, 15, 0, 0),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 30
            };

            infoStackLayout.Children.Add(SearchLabel);
        }

        private async void UpInfoFrameOnWaitingDriver(object sender, EventArgs e)
        {
            _upDownInfoFrame.Clicked -= UpInfoFrameOnWaitingDriver;

            infoStackLayout.Children.Clear();
            infoStackLayout.Children.Add(_upDownInfoFrame);
            infoStackLayout.Children.Add(_taxiImage);
            infoStackLayout.Children.Add(_taxiInfo);
            infoStackLayout.Children.Add(_driverInfo);
            infoStackLayout.Children.Add(_driverExpirience);
            infoStackLayout.Children.Add(SearchLabel);
            infoStackLayout.Children.Add(_fastSearchInfo);
            infoStackLayout.Children.Add(_buttonOnWaitingDriver);

            await AnimateHeightInfoFrame(465, 500);

            _upDownInfoFrame.Text = "v";
            _upDownInfoFrame.FontSize = 25;
            _upDownInfoFrame.Clicked += DownInfoFrameOnWaitingDriver;
        }

        private async void DownInfoFrameOnWaitingDriver(object sender, EventArgs e)
        {
            _upDownInfoFrame.Clicked -= DownInfoFrameOnWaitingDriver;

            infoStackLayout.Children.Clear();
            infoStackLayout.Children.Add(_upDownInfoFrame);
            infoStackLayout.Children.Add(_taxiInfo);
            infoStackLayout.Children.Add(SearchLabel);
            infoStackLayout.Children.Add(_buttonOnWaitingDriver);

            await AnimateHeightInfoFrame(180, 500);

            _upDownInfoFrame.Text = "^";
            _upDownInfoFrame.FontSize = 35;
            _upDownInfoFrame.Clicked += UpInfoFrameOnWaitingDriver;
        }

        public async Task<string> GetCookieMap(string name)
        {
            return await map.EvaluateJavaScriptAsync($"GetCookie('{name}')");
        }

        private string GetPrice(string distance, string duration)
        {
            const int StartPrice = 75;
            const int MinPrice = 150;
            const int PriceToKm = 12;
            const int PriceToMin = 5;

            double km = 0;
            double min = 0;
            double price = 0;

            if (Convert.ToString(distance.ToUpper().Split(' ')[1]) == "М")
            {
                km = Convert.ToDouble(distance.Split(' ')[0]) / 1000;
            }
            else if (Convert.ToString(distance.ToUpper().Split(' ')[1]) == "КМ")
            {
                km = Convert.ToDouble(distance.Split(' ')[0]);
            }

            km = Convert.ToDouble(string.Format("{0:N1}", km));

            if (Convert.ToString(duration.ToUpper().Split(' ')[1]) == "МИН")
            {
                min = Convert.ToDouble(duration.Split(' ')[0]);
            }
            else if (Convert.ToString(duration.ToUpper().Split(' ')[1]) == "Ч")
            {
                min = Convert.ToDouble(duration.Split(' ')[0]) * 60 + Convert.ToDouble(duration.Split(' ')[2]);
            }

            price = Math.Floor(Math.Max((StartPrice + PriceToKm * km + PriceToMin * min), MinPrice));

            return Convert.ToString(price);
        }

        private void SetAndroidOption()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                infoFrame.WidthRequest = 335;
            }
        }

        private async Task<string> AnimateHeightInfoFrame(int newValue, int time)
        {
            Animation animation = new Animation(v => infoFrame.HeightRequest = v, infoFrame.HeightRequest, newValue);

            animation.Commit(this, "AnimateHeightInfoFrame", 16, (uint)time, Easing.Linear, (v, c) => infoFrame.HeightRequest = newValue, () => false);
            await Task.Delay(time);

            return null;
        }
    }
}
