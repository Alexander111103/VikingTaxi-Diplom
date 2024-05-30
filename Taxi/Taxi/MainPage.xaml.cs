using System;
using System.Net;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Collections.Generic;
using System.Linq;

namespace Taxi
{
    public partial class MainPage : ContentPage
    {
        public Label SearchLabel;

        private Label _timerLabel;
        private Button _buttonOrderTaxi;
        private FlyoutMenu _flyoutMenu;
        private Button _fastSearchButton;
        private Button _upDownInfoFrame;
        private Label _taxiInfo;
        private Image _taxiImage;
        private Label _driverInfo;
        private Label _driverExpirience;
        private Label _fastSearchInfo;
        private FlexLayout _buttonOnWaitingDriver;
        private ImageButton _phoneCallDriver;
        private ImageButton _cancelOrder;

        public JsonRouteInfo RouteInfo;
        public string State = "Order";

        private string _rate = "base";
        private string _priority = "0";
        private string _price;
        private string _ecoPrice;
        private string _paymentType = "cash";
        private int _idOrder;
        private string _phoneDriver;
        private bool _isFastSearch = false;

        public MainPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;
            
            map.Cookies = new CookieContainer();

            map.Cookies.Add(new Cookie("State", State, "/", "taxiviking.ru"));
            map.Cookies.Add(new Cookie("CoordersFrom", "", "/", "taxiviking.ru"));
            map.Cookies.Add(new Cookie("CoordersTo", "", "/", "taxiviking.ru"));

            LoadingActiveOrder();
        }

        private async void RouteAgree_Click(object sender, EventArgs e)
        {
            routeAgreeButton.Clicked -= RouteAgree_Click;

            await Task.Delay(500);

            RouteInfo = JsonConvert.DeserializeObject<JsonRouteInfo>(Convert.ToString(await map.EvaluateJavaScriptAsync("GetRouteInfo()")).Replace(@"\", ""));

            if (RouteInfo.Distance != null && RouteInfo.Distance != "")
            {
                _price = await GetPrice(RouteInfo.Distance, RouteInfo.DurationInTraffic);

                _ecoPrice = _price;

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
                routeAgreeButton.Clicked += RouteAgree_Click;

                await map.EvaluateJavaScriptAsync("EditRoute()");
                await DisplayAlert("Ошибка", "Вы не указали маршрут", "OK");
            }
        }

        private async void RouteReset_Click(object sender, EventArgs e)
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

        private async void FromAdress_Click(object sender, EventArgs e)
        {
            ((Button)sender).Clicked -= FromAdress_Click;

            App.Current.Properties.TryGetValue("login", out object login);

            JsonFavoriteAddresses addresses = await DataBaseApi.GetFavoriteAddressesByLogin($"{login}");

            if (addresses.Addresses.Count > 0)
            {
                List<string> names = addresses.Addresses.Select(x => x.Name).ToList();

                string FromAddress = await DisplayActionSheet("Выберите избранный адрес для начальной точки маршрута.", null, null, names.ToArray());

                if (FromAddress != null)
                {
                    string coorders = (addresses.Addresses.Find(x => x.Name == FromAddress)).Coorders;

                    map.EvaluateJavaScriptAsync($"SetFromAddress('{coorders}')");
                }
            }
            else
            {
                DisplayAlert("Ошибка", "У вас еще нет избранных адресов.\nВы можете создать новый избранный адрес на странице \"Мои адреса\".", "Ok");
            }

            ((Button)sender).Clicked += FromAdress_Click;
        }

        private async void ToAdress_Click(object sender, EventArgs e)
        {
            ((Button)sender).Clicked -= ToAdress_Click;

            App.Current.Properties.TryGetValue("login", out object login);

            JsonFavoriteAddresses addresses = await DataBaseApi.GetFavoriteAddressesByLogin($"{login}");

            if (addresses.Addresses.Count > 0)
            { 
                List<string> names = addresses.Addresses.Select(x => x.Name).ToList();

                string ToAddress = await DisplayActionSheet("Выберите избранный адрес для конечной точки маршрута.", null, null, names.ToArray());

                if (ToAddress != null)
                {
                    string coorders = (addresses.Addresses.Find(x => x.Name == ToAddress)).Coorders;

                    map.EvaluateJavaScriptAsync($"SetToAddress('{coorders}')");
                }
            }
            else
            {
                DisplayAlert("Ошибка", "У вас еще нет избранных адресов.\nВы можете создать новый избранный адрес на странице \"Мои адреса\".", "Ok");
            }

            ((Button)sender).Clicked += ToAdress_Click;
        }

        private async void OrderTaxi_Click(object sender, EventArgs e)
        {
            await SetOptionsInfoFrameOnSearch();

            SetSearchStatus();

            SearchOnMap();

            _idOrder = await DataBaseApi.AddOrderTaxi(RouteInfo.Distance, RouteInfo.Duration, RouteInfo.DurationInTraffic, RouteInfo.StartShort, RouteInfo.FinishShort, RouteInfo.StartLong, RouteInfo.FinishLong, RouteInfo.StartCoorders, RouteInfo.FinishCoorders, _priority, _price, _rate, _paymentType);
            _flyoutMenu.SearchTaxi(_idOrder);
        }

        public bool OrderTaxiTimerTick()
        {
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)_flyoutMenu.Timer.ElapsedMilliseconds);
            _timerLabel.Text = new DateTimeOffset(2024, 1, 1, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss");
            return _flyoutMenu.IsTimerStart;
        }

        private void OpenMenu_Click(object sender, EventArgs e)
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

        private void RateEco_Click(object sender, EventArgs e)
        {
            if(_rate != "base")
            {
                rateEco.BackgroundColor = Color.FromHex("#2196f3");
                rateEco.TextColor = Color.White;

                rateBusiness.BackgroundColor = Color.White;
                rateBusiness.TextColor = Color.Black;

                rateChild.BackgroundColor = Color.White;
                rateChild.TextColor = Color.Black;

                _price = _ecoPrice;

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "base";
                _priority = "0";
            }
        }

        private void RateBusiness_Click(object sender, EventArgs e)
        {
            if (_rate != "business")
            {
                rateEco.BackgroundColor = Color.White;
                rateEco.TextColor = Color.Black;

                rateBusiness.BackgroundColor = Color.FromHex("#2196f3");
                rateBusiness.TextColor = Color.White;

                rateChild.BackgroundColor = Color.White;
                rateChild.TextColor = Color.Black;

                _price = Convert.ToString(Convert.ToInt32(_ecoPrice) + 100);

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "business";
                _priority = "1";
            }
        }

        private void RateChild_Click(object sender, EventArgs e)
        {
            if (_rate != "child")
            {
                rateEco.BackgroundColor = Color.White;
                rateEco.TextColor = Color.Black;

                rateBusiness.BackgroundColor = Color.White;
                rateBusiness.TextColor = Color.Black;

                rateChild.BackgroundColor = Color.FromHex("#2196f3");
                rateChild.TextColor = Color.White;

                _price = Convert.ToString(Convert.ToInt32(_ecoPrice) + 50);

                priceLabel.Text = $"Цена: {_price} рублей.";

                _rate = "child";
                _priority = "0";
            }
        }

        private async void FastSearch_Click(object sender, EventArgs e)
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

        private async void Cancel_Click(object sender, EventArgs e)
        {
            _cancelOrder.Clicked -= Cancel_Click;

            bool isCansel = false;

            isCansel = await DisplayAlert("Отмена заказа", "Вы точно хотите отменить заказ?", "Да", "Нет");

            if (isCansel)
            { 
                _flyoutMenu.CanselOrder(_idOrder);
            }
            else
            {
                _cancelOrder.Clicked += Cancel_Click;
            }
        }

        private void PhoneCallDriver_Click(object sender, EventArgs e)
        {
            PhoneDialer.Open(_phoneDriver);
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

        public async Task<string> SetOptionsInfoFrameOnSearch()
        {
            stackLayout.Children.Remove(myAdresses);

            infoStackLayout.Children.Clear();

            if (_flyoutMenu.PageNumber == 0)
            {
                await AnimateHeightInfoFrame(70, 500);
            }
            else
            {
                infoFrame.HeightRequest = 70;
            }

            AbsoluteLayout layout = new AbsoluteLayout();

            SearchLabel = new Label()
            {
                Text = "Поиск...",
                Margin = new Thickness(0, 0, 0, 0),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 30
            };

            AbsoluteLayout.SetLayoutBounds(SearchLabel, new Rectangle(0.55, 0.2, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(SearchLabel, AbsoluteLayoutFlags.PositionProportional);

            _timerLabel = new Label()
            {
                Text = "00:00:00",
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 20
            };

            AbsoluteLayout.SetLayoutBounds(_timerLabel, new Rectangle(0.1, 1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(_timerLabel, AbsoluteLayoutFlags.PositionProportional);

            _cancelOrder = new ImageButton()
            {
                HeightRequest = 70,
                WidthRequest = 70,
                CornerRadius = 50,
                BackgroundColor = Color.White,
                Source = ImageSource.FromResource("Taxi.Images.cancel.png"),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Aspect = Aspect.AspectFill,
                Margin = new Thickness(0, 0, 0, 0),
            };

            _cancelOrder.Clicked += Cancel_Click;

            AbsoluteLayout.SetLayoutBounds(_cancelOrder, new Rectangle(0.97, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(_cancelOrder, AbsoluteLayoutFlags.PositionProportional);

            infoStackLayout.Children.Add(layout);
            layout.Children.Add(SearchLabel);
            layout.Children.Add(_timerLabel);
            layout.Children.Add(_cancelOrder);
            

            if (!_isFastSearch)
            {
                fastFrame.WidthRequest = 185;
                fastFrame.HeightRequest = 45;

                _fastSearchButton = new Button()
                {
                    Text = "Срочный поиск",
                    FontSize = 14,
                    BackgroundColor = Color.FromHex("#293133"),
                    TextColor = Color.White,
                    ImageSource = ImageSource.FromResource("Taxi.Images.fastSearchHorseAndroid.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10),
                    CornerRadius = 10
                };

                _fastSearchButton.Clicked += FastSearch_Click;

                fast.Children.Add(_fastSearchButton);
            }

            return null;
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

            infoStackLayout.Children.Clear();

            fast.Children.Clear();

            SearchLabel = new Label()
            {
                Text = "Поиск...",
                Margin = new Thickness(0, 0, 0, 15),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 15
            };

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

            _phoneCallDriver.Clicked += PhoneCallDriver_Click;

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

            _cancelOrder.Clicked += Cancel_Click;

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

            _buttonOnWaitingDriver = new FlexLayout
            {
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center,
                Direction = FlexDirection.Row,
                Margin = new Thickness(10, 0, 0, 0)
            };

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
                Text = "Таксометр запущен.",
                Margin = new Thickness(0, 15, 0, 0),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                FontSize = 20
            };

            infoStackLayout.Children.Add(SearchLabel);
        }

        private async void LoadingActiveOrder()
        {
            App.Current.Properties.TryGetValue("login", out object login);

            await Task.Delay(new TimeSpan(0, 0, 3));

            int activeOrderId = await DataBaseApi.GetActiveOrderIdByLoginUser($"{login}");

            if (activeOrderId != 0)
            {
                DisplayAlert("Загрузка", "Загружается уже запущенный заказ.", "Ок");

                string status = await DataBaseApi.GetStatusOrderById(activeOrderId);

                switch (status)
                {
                    case "search":
                    case "searched":
                        RouteInfo = await DataBaseApi.GetRouteInfoByIdOrder(activeOrderId);
                        _idOrder = activeOrderId;
                        await SetOptionsInfoFrameOnSearch();
                        SetSearchStatus();
                        SearchOnMap();
                        _flyoutMenu.SearchTaxi(activeOrderId);
                        break;

                    case "waitingUser":
                    case "waitingDriver":
                        State = "Waiting";
                        RouteInfo = await DataBaseApi.GetRouteInfoByIdOrder(activeOrderId);
                        _idOrder = activeOrderId;
                        _flyoutMenu.DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(activeOrderId);
                        SetOptionsInfoFrameOnWaitingDriver(await DataBaseApi.GetTaxiInfoByIdOrder(activeOrderId));
                        WaitingDriverOnMap(_flyoutMenu.DriverCoorders);
                        _flyoutMenu.WaitingDriver(activeOrderId);
                        break;

                    case "drive":
                        _flyoutMenu.DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(activeOrderId);
                        RouteInfo = await DataBaseApi.GetRouteInfoByIdOrder(activeOrderId);
                        _idOrder = activeOrderId;
                        _flyoutMenu.DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(activeOrderId);
                        SetDriveStatus(_flyoutMenu.DriverCoorders);
                        SetOptionsInfoFrameOnDrive();
                        DriveOnMap(_flyoutMenu.DriverCoorders);
                        _flyoutMenu.Drive(activeOrderId);
                        break;

                    default:
                        break;
                }
            }
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

        private async Task<string> GetPrice(string distance, string duration)
        {
            JsonPrice priceInfo = await DataBaseApi.GetActualPrice();

            int StartPrice = priceInfo.StartPrice;
            int MinPrice = priceInfo.MinPrice;
            int PriceToKm = priceInfo.PricePerKm;
            int PriceToMin = priceInfo.PricePerMin;

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

        private async Task<string> AnimateHeightInfoFrame(int newValue, int time)
        {
            Animation animation = new Animation(v => infoFrame.HeightRequest = v, infoFrame.HeightRequest, newValue);

            animation.Commit(this, "AnimateHeightInfoFrame", 16, (uint)time, Easing.Linear, (v, c) => infoFrame.HeightRequest = newValue, () => false);
            await Task.Delay(time);

            return null;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
