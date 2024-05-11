using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlyoutMenu : FlyoutPage
    {
        private MainPage _mainPage;
        private HistoryOrdersPage _historyOrdersPage;
        private AdminHubPage _adminHubPage;

        public PickAutoDriverPage PickAutoDriverPage;
        public SearchOrderDriverPage SearchOrderDriverPage;
        public TaxameterPage TaxameterPage;

        private List<Button> _buttons = new List<Button>();
        private Button _searchOrderDriverButton;
        private Button _historyOrdersDriverButton;
        private Button _myCarsDriverButton;
        private Button _adminHub;

        public int PageNumber { get; private set; }

        public Stopwatch Timer = new Stopwatch();
        public bool IsTimerStart = false;

        public string DriverCoorders;
        public string DriverState = "pickAuto";

        public FlyoutMenu()
        {
            InitializeComponent();
            _mainPage = new MainPage(this);
            _historyOrdersPage = new HistoryOrdersPage(this);

            Detail = new NavigationPage(_mainPage);

            SetAndroidOption();

            extraMap.Source = new HtmlWebViewSource { Html = "<!DOCTYPE html><html><head><meta http-equiv=\"Content-Type\" content=\"map/html; charset=utf-8\"/><script src=\"https://api-maps.yandex.ru/2.1/?lang=ru_RU&apikey=a36cde6b-2202-41e4-ac57-34889ffb135f&suggest_apikey=8d505c91-3825-4543-9d6a-3fd3c38c4d40\" type=\"text/javascript\"></script><script src=\"https://yandex.st/jquery/2.2.3/jquery.min.js\" type=\"text/javascript\"></script><style> html, body, #map { width: 100%; height: 100%; padding: 0; margin: 0; box-sizing: border-box; } </style></head><body><div id=\"map\"></div></body></html>\r\n<script>\r\n    var routeInfo = {};\r\n    var myMap;\r\n\r\n    ymaps.ready(function () {\r\n    myMap = new ymaps.Map('map', { center: [56.843994, 53.252093], zoom: 13, controls: [] }, { restrictMapArea: [[55.843994,52.252093],[57.843994,54.252093]]});\r\n    var routePanelControl = new ymaps.control.RoutePanel({ options: {showHeader: true,title: 'Маршрут',routePanelTypes: {taxi: false},maxWidth: '300px'}});\r\n    routePanelControl.routePanel.state.set({type: \"auto\", toEnabled: true});\r\n\r\n    myMap.controls.add(routePanelControl);\r\n        var multiRoutePromise = routePanelControl.routePanel.getRouteAsync();\r\n        multiRoutePromise.then(function (multiRoute) {\r\n            multiRoute.model.setParams({results: 1, reverseGeocoding: true, boundedBy: [[55.843994,52.252093],[57.843994,54.252093]], strictBounds: true});\r\n            multiRoute.model.events.add('requestsuccess', function () {\r\n                var activeRoute = multiRoute.getActiveRoute();\r\n                var wayPoints = multiRoute.getWayPoints();\r\n                if (activeRoute) {\r\n                    routeInfo.distance = activeRoute.properties.get(\"distance\").text;\r\n                    routeInfo.duration = activeRoute.properties.get(\"duration\").text;\r\n                    routeInfo.durationInTraffic = activeRoute.properties.get(\"durationInTraffic\").text;\r\n                    routeInfo.startShort = \"\" + wayPoints.get(1).properties.get(\"name\");\r\n                    routeInfo.finishShort = \"\" + wayPoints.get(0).properties.get(\"name\");\r\n                    routeInfo.startLong = \"\" + wayPoints.get(1).properties.get(\"address\");\r\n                    routeInfo.finishLong = \"\" + wayPoints.get(0).properties.get(\"address\");\r\n                    routeInfo.startCoorders = \"\" + wayPoints.get(1).properties.get(\"request\");\r\n                    routeInfo.finishCoorders = \"\" + wayPoints.get(0).properties.get(\"request\");\r\n                }\r\n                else\r\n                {\r\n                    routeInfo.distance = \"\";\r\n                    routeInfo.duration = \"\";\r\n                    routeInfo.durationInTraffic = \"\";\r\n                    routeInfo.startShort = \"\";\r\n                    routeInfo.finishShort = \"\";\r\n                    routeInfo.startLong = \"\";\r\n                    routeInfo.finishLong = \"\";\r\n                    routeInfo.startCoorders = \"\";\r\n                    routeInfo.finishCoorders = \"\";\r\n                }\r\n            }); \r\n        });\r\n    });\r\n\r\n    function SetRoute(_from, _to)\r\n    {\r\n        myMap.controls.get(0).routePanel.state.set({from: _from, to: _to });}\r\n    function GetTimeWaiting(){    return routeInfo.durationInTraffic;}\r\n    \r\n\r\n    const delay = async (ms) => await new Promise(resolve => setTimeout(resolve, ms));\r\n</script>\r\n" };

            PageNumber = 0;

            _buttons.Add(orderTaxi);
            _buttons.Add(historyOrders);
            _buttons.Add(myAdresses);
            _buttons.Add(game);
            _buttons.Add(options);
            _buttons.Add(infoTaxi);
            _buttons.Add(profile);

            SetFio();
            SetRole();
        }

        public async void OrderTaxi_Click(object sender, EventArgs e)
        {
            if (PageNumber != 0)
            {
                Detail = new NavigationPage(_mainPage);
                orderTaxi.TextColor = Color.Gray;
                orderTaxi.BorderColor = Color.Black;
                PageNumber = 0;

                DisableAllButtons();
                RecolorButtons();

                await Task.Delay(1000);
                string cookieState = await _mainPage.GetCookieMap("State");

                if (cookieState != _mainPage.State)
                {
                    switch (_mainPage.State)
                    {
                        case "Waiting":
                            _mainPage.WaitingDriverOnMap(DriverCoorders);
                            break;

                        case "Search":
                            _mainPage.SetSearchStatus();
                            _mainPage.SearchOnMap();
                            break;

                        case "WaitingUser":
                            _mainPage.SetWaitingUserStatus(DriverCoorders);
                            _mainPage.WaitingUserOnMap(DriverCoorders);
                            break;

                        case "Drive":
                            _mainPage.SetDriveStatus(DriverCoorders);
                            _mainPage.DriveOnMap(DriverCoorders);
                            break;
                    }
                }

                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void HistoryOrders_Click(object sender, EventArgs e)
        {
            if (PageNumber != 1)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                historyOrders.TextColor = Color.Gray;
                historyOrders.BorderColor = Color.Black;
                PageNumber = 1;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void MyAdresses_Click(object sender, EventArgs e)
        {
            if (PageNumber != 2)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                myAdresses.TextColor = Color.Gray;
                myAdresses.BorderColor = Color.Black;
                PageNumber = 2;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void Game_Click(object sender, EventArgs e)
        {
            if (PageNumber != 3)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                game.TextColor = Color.Gray;
                game.BorderColor = Color.Black;
                PageNumber = 3;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void Options_Click(object sender, EventArgs e)
        {
            if (PageNumber != 4)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                options.TextColor = Color.Gray;
                options.BorderColor = Color.Black;
                PageNumber = 4;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void InfoTaxi_Click(object sender, EventArgs e)
        {
            if (PageNumber != 5)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                infoTaxi.TextColor = Color.Gray;
                infoTaxi.BorderColor = Color.Black;
                PageNumber = 5;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public void Exit_Click(object sender, EventArgs e)
        {

        }

        public async void Profile_Click(object sender, EventArgs e)
        {
            if (PageNumber != 6)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                profile.TextColor = Color.Gray;
                profile.BorderColor = Color.Black;
                PageNumber = 6;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void SearchOrderDriver_Click(object sender, EventArgs e)
        {
            if (PageNumber != 7)
            {
                DisableAllButtons();

                switch(DriverState)
                {
                    case "pickAuto":
                        Detail = new NavigationPage(PickAutoDriverPage);
                        break;

                    case "search":
                        Detail= new NavigationPage(SearchOrderDriverPage);
                        break;

                    case "drive":
                        Detail = new NavigationPage(TaxameterPage);
                        break;
                }

                _searchOrderDriverButton.TextColor = Color.FromHex("#FFA940");
                _searchOrderDriverButton.BorderColor = Color.FromHex("#D57500");
                PageNumber = 7;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void HistoryOrdersDriver_Click(object sender, EventArgs e)
        {
            if (PageNumber != 8)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                _historyOrdersDriverButton.TextColor = Color.FromHex("#FFA940");
                _historyOrdersDriverButton.BorderColor = Color.FromHex("#D57500");
                PageNumber = 8;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void MyCarsDriver_Click(object sender, EventArgs e)
        {
            if (PageNumber != 9)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_historyOrdersPage);

                _myCarsDriverButton.TextColor = Color.FromHex("#FFA940");
                _myCarsDriverButton.BorderColor = Color.FromHex("#D57500");
                PageNumber = 9;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        public async void AdminHub_Click(object sender, EventArgs e)
        {
            if (PageNumber != 7)
            {
                DisableAllButtons();
                Detail = new NavigationPage(_adminHubPage);

                _adminHub.TextColor = Color.FromHex("#A52A2A");
                _adminHub.BorderColor = Color.FromHex("#902537");
                PageNumber = 7;
                RecolorButtons();

                await Task.Delay(1000);
                EnableAllButtons();
            }

            IsPresented = false;
        }

        private void RecolorButtons()
        {
            App.Current.Properties.TryGetValue("role", out object role);

            for (int i = 0; i < _buttons.Count; i++)
            {
                if (i != PageNumber)
                {
                    _buttons[i].TextColor = Color.Black;
                    _buttons[i].BorderColor = Color.FromHex("#F2F2F2");

                    if ((string)role == "driver" && (i == 7 || i == 8 || i == 9))
                    {
                        _buttons[i].TextColor = Color.FromHex("#D57500");
                    }

                    if ((string)role == "admin" && i == 7)
                    {
                        _buttons[i].TextColor = Color.FromHex("#902537");
                    }
                }
            }
        }

        public async void SearchTaxi(int idOrder)
        {
            string status;
            bool isSearch = true;
            bool isAccept = false;

            Timer.Restart();
            IsTimerStart = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), _mainPage.OrderTaxiTimerTick);

            ShowNotification("Поиск такси", "Запушен поиск такси.");

            while (isSearch)
            {
                _mainPage.SearchLabel.FadeTo(0.4, 5000);
                await Task.Delay(new TimeSpan(0, 0, 5));
                _mainPage.SearchLabel.FadeTo(1, 1000);
                status = await DataBaseApi.GetStatusOrderById(idOrder);

                if (status == "searched")
                {
                    isSearch = false;

                    DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(idOrder);
                    string userCoorders = _mainPage.RouteInfo.StartCoorders;

                    string waitingTime = await GetTimeWaitingDriver(DriverCoorders, userCoorders);

                    JsonTaxiInfo taxiInfo = await DataBaseApi.GetTaxiInfoByIdOrder(idOrder);

                    ShowNotification("Найдено такси", $"{taxiInfo.Color} {taxiInfo.Brand} {taxiInfo.Mark} {taxiInfo.Numer.ToLower()}.\nТакси прибудет через {waitingTime}.");

                    isAccept = await DisplayAlert("Найдено такси", $"{taxiInfo.Color} {taxiInfo.Brand} {taxiInfo.Mark}\nРейтинг водителя: {taxiInfo.Rating}\n\nТакси прибудет через {waitingTime}\n\n{taxiInfo.Numer.ToLower()}", "Принять", "Отказаться");

                    if (await DataBaseApi.GetStatusOrderById(idOrder) == "search")
                    {
                        isSearch = true;
                        DisplayAlert("Ошибка", "Вы слишком долго думали, найденное такси отменено.", "Ок");
                        ShowNotification("Поиск такси", "Запушен поиск такси.");
                    }
                    else
                    {
                        if (!isAccept)
                        {
                            isSearch = true;
                            DataBaseApi.ReupdateStatusToSearchByIdOrder(idOrder);

                            ShowNotification("Поиск такси", "Запушен поиск такси.");
                        }
                        else
                        {
                            IsTimerStart = false;
                            DataBaseApi.SetStatusToWaitingDriverByIdOrder(idOrder, Timer.ElapsedMilliseconds);
                            Timer.Stop();

                            _mainPage.State = "Waiting";
                            _mainPage.SetOptionsInfoFrameOnWaitingDriver(taxiInfo);
                            _mainPage.WaitingDriverOnMap(DriverCoorders);

                            WaitingDriver(idOrder);
                        }
                    }
                }
            }
        }

        public async void WaitingDriver(int idOrder)
        {
            string status;
            bool isWaiting = true;

            ShowNotification("Такси в пути", "Такси направляет к вам.");

            while (isWaiting)
            {
                await Task.Delay(new TimeSpan(0, 0, 10));
                status = await DataBaseApi.GetStatusOrderById(idOrder);

                switch (status)
                {
                    case "waitingDriver":
                        DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(idOrder);
                        _mainPage.WaitingDriverOnMap(DriverCoorders);
                        break;

                    case "waitingUser":
                        DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(idOrder);
                        _mainPage.SetWaitingUserStatus(DriverCoorders);
                        _mainPage.SetOptionsInfoFrameOnWaitingUser();
                        _mainPage.WaitingUserOnMap(DriverCoorders);
                        ShowNotification("Такси на месте", "Вас ожидает такси.");
                        break;

                    case "search":
                        isWaiting = false;
                        _mainPage.SetSearchStatus();
                        _mainPage.SetOptionsInfoFrameOnSearch();
                        _mainPage.SearchOnMap();
                        SearchTaxi(idOrder);
                        DisplayAlert("Водитель отменил поездку", "К сожалению водитель отменил поезду\nПостараемся в кратчайший срок найти вам нового водителя", "Ок");
                        break;

                    case "drive":
                        isWaiting = false;
                        DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(idOrder);
                        _mainPage.SetDriveStatus(DriverCoorders);
                        _mainPage.SetOptionsInfoFrameOnDrive();
                        _mainPage.DriveOnMap(DriverCoorders);
                        Drive(idOrder);
                        break;

                    default: 
                        break;
                }
            }
        }

        public async void Drive(int idOrder)
        {
            string status;
            bool isDriving = true;

            ShowNotification("Такси в поездке", "Таксометр запущен.");

            while (isDriving)
            {
                await Task.Delay(new TimeSpan(0, 0, 10));

                status = await DataBaseApi.GetStatusOrderById(idOrder);

                if (status == "drive")
                {
                    DriverCoorders = await DataBaseApi.GetDriverCoordersByIdOrder(idOrder);
                    _mainPage.SetDriveStatus(DriverCoorders);
                    _mainPage.DriveOnMap(DriverCoorders);
                }
                else
                {
                    isDriving = false;

                    ShowNotification("Поезка завершена", "Оцените поездку.");

                    string rating = await DisplayActionSheet("Как вы оцените поездку?", null, null, "5 Звезд", "4 Звезды", "3 Звезды", "2 Звезды", "1 Звезда");

                    if (rating == null)
                    {
                        rating = "0 Звезд";
                    }
                    else
                    {
                        DataBaseApi.AddDriverRatingByOrderId(idOrder, Convert.ToInt32(Convert.ToString(rating[0])));
                    }

                    DataBaseApi.SetRatingOrderById(idOrder, Convert.ToInt32(Convert.ToString(rating[0])));

                    await DisplayAlert("Благодарность", "Спасибо за поезду <3", "Ок");

                    _mainPage = new MainPage(this);

                    if (PageNumber == 0)
                    {
                        Detail = new NavigationPage(_mainPage);
                    }
                }
            }
        }

        public void CanselOrder(int idOrder)
        {
            DataBaseApi.SetStatusToCanseledByIdOrder(idOrder);

            Navigation.RemovePage(this);
            Navigation.PushAsync(new FlyoutMenu());
        }

        private async void ShowNotification(string title, string description)
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            LocalNotificationCenter.Current.Show(new NotificationRequest { Description = description, Title = title });
        }

        private async Task<string> GetTimeWaitingDriver(string from, string to)
        {
            await extraMap.EvaluateJavaScriptAsync($"SetRoute('{from}', '{to}');");
            await Task.Delay(1000);
            return await extraMap.EvaluateJavaScriptAsync($"GetTimeWaiting();");
        }

        private async void SetFio()
        {
            App.Current.Properties.TryGetValue("login", out object login);
            string fio = await DataBaseApi.GetFioByLogin((string)login);

            profile.Text = fio;
        }

        private async void SetRole()
        {
            App.Current.Properties.TryGetValue("login", out object login);
            string role = await DataBaseApi.GetRoleByLogin((string)login);

            if (App.Current.Properties.TryGetValue("role", out object _role))
            {
                App.Current.Properties.Remove("role");
            }

            App.Current.Properties.Add("role", role);

            if (role == "user")
            {
                roleLabel.Text = "Пассажир";
            }

            if (role == "driver")
            {
                roleLabel.Text = "Водитель";

                _searchOrderDriverButton = new Button
                {
                    Text = "Поиск заказа",
                    BackgroundColor = Color.FromHex("#F2F2F2"),
                    TextColor = Color.FromHex("#D57500"),
                    FontSize = 26
                };
                _searchOrderDriverButton.Clicked += SearchOrderDriver_Click;
                AbsoluteLayout.SetLayoutBounds(_searchOrderDriverButton, new Rectangle(0.5, 0.565, 250, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(_searchOrderDriverButton, AbsoluteLayoutFlags.PositionProportional);

                _historyOrdersDriverButton = new Button
                {
                    Text = "История заказов",
                    BackgroundColor = Color.FromHex("#F2F2F2"),
                    TextColor = Color.FromHex("#D57500"),
                    FontSize = 26
                };
                _historyOrdersDriverButton.Clicked += HistoryOrdersDriver_Click;
                AbsoluteLayout.SetLayoutBounds(_historyOrdersDriverButton, new Rectangle(0.5, 0.645, 250, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(_historyOrdersDriverButton, AbsoluteLayoutFlags.PositionProportional);

                _myCarsDriverButton = new Button
                {
                    Text = "Мои автомобили",
                    BackgroundColor = Color.FromHex("#F2F2F2"),
                    TextColor = Color.FromHex("#D57500"),
                    FontSize = 26
                };
                _myCarsDriverButton.Clicked += MyCarsDriver_Click;
                AbsoluteLayout.SetLayoutBounds(_myCarsDriverButton, new Rectangle(0.5, 0.725, 250, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(_myCarsDriverButton, AbsoluteLayoutFlags.PositionProportional);

                if (Device.RuntimePlatform == Device.Android)
                {
                    _searchOrderDriverButton.FontSize = 22;
                    _historyOrdersDriverButton.FontSize = 22;
                    _myCarsDriverButton.FontSize = 22;
                }

                Layout.Children.Add(_searchOrderDriverButton);
                Layout.Children.Add(_historyOrdersDriverButton);
                Layout.Children.Add(_myCarsDriverButton);

                _buttons.Add(_searchOrderDriverButton);
                _buttons.Add(_historyOrdersDriverButton);
                _buttons.Add(_myCarsDriverButton);

                PickAutoDriverPage = new PickAutoDriverPage(this);
            }

            if (role == "admin")
            {
                roleLabel.Text = "Администратор";

                _adminHub = new Button
                {
                    Text = "Admin Hub",
                    BackgroundColor = Color.FromHex("#F2F2F2"),
                    TextColor = Color.FromHex("#902537"),
                    FontSize = 26
                };
                _adminHub.Clicked += AdminHub_Click;
                AbsoluteLayout.SetLayoutBounds(_adminHub, new Rectangle(0.5, 0.565, 250, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(_adminHub, AbsoluteLayoutFlags.PositionProportional);

                if (Device.RuntimePlatform == Device.Android)
                {
                    _adminHub.FontSize = 22;
                }

                Layout.Children.Add(_adminHub);

                _buttons.Add(_adminHub);

                _adminHubPage = new AdminHubPage(this);
            }
        }

        private void SetAndroidOption()
        {
            if(Device.RuntimePlatform == Device.Android)
            {
                titleLabel.FontSize = 22;
                roleLabel.FontSize = 14;
                orderTaxi.FontSize = 22;
                historyOrders.FontSize = 22;
                myAdresses.FontSize = 22;
                game.FontSize = 22;
            }
        }

        private void DisableAllButtons()
        {
            foreach (Button button in _buttons)
            {
                button.IsEnabled = false;
            }
        }

        private void EnableAllButtons()
        {
            foreach (Button button in _buttons)
            {
                button.IsEnabled = true;
            }
        }
    }
}