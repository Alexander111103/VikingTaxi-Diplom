using Java.Lang;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchOrderDriverPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        private bool _autoReload = false;
        private string _rate;
        private string _isChildSeet;

        private bool _isPick = false;
        private int _idOrder;
        private bool _isDoCansel = false;

        public SearchOrderDriverPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            SetDriverPropertis();
            LoadOrders();

            refreshView.Command = new Command(LoadOrders);
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

        private async void SetDriverPropertis()
        {
            App.Current.Properties.TryGetValue("login", out object login);
            JsonCar car = await DataBaseApi.GetDriverCurrentCarByDriverLogin($"{login}");

            _rate = car.Rate;
            _isChildSeet = car.IsChildSeet;
        }

        private async void Sleep_Click(object sender, EventArgs e)
        {
            App.Current.Properties.TryGetValue("login", out object login);
            DataBaseApi.SetDriverStatusToSleepByLogin($"{login}");

            _flyoutMenu.DriverState = "pickAuto";

            _flyoutMenu.PickAutoDriverPage = new PickAutoDriverPage(_flyoutMenu);

            await Navigation.PushAsync(_flyoutMenu.PickAutoDriverPage);
        }

        private async void LoadOrders_Click(object sender, EventArgs e)
        {
            ((Button)sender).Clicked -= LoadOrders_Click;
            LoadOrders();

            await Task.Delay(new TimeSpan(0, 0, 2));
            ((Button)sender).Clicked += LoadOrders_Click;
        }

        private async void CanselOrderDriver_Click(object sender, EventArgs e)
        {
            bool isAgree = await DisplayAlert("Подтверждение", "Вы точно хотите отменить заказ?", "Да", "Нет");

            if (isAgree)
            {
                _isDoCansel = true;
                DataBaseApi.ReupdateStatusToSearchByIdOrder(_idOrder);
            }
        }

        private async void LoadOrders()
        {
            refreshView.IsRefreshing = true;
            listView.ItemsSource = (await DataBaseApi.GetOrders(_rate, _isChildSeet)).Orders;
            listView.ItemTapped -= ListViewItemTapped;
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label from = new Label() { FontSize = 15 };
                from.SetBinding(Label.TextProperty, "StartShort", stringFormat:"Из: {0}");
                Label to = new Label() { FontSize = 15 };
                to.SetBinding(Label.TextProperty, "FinishShort", stringFormat: "В: {0}");
                Label distance = new Label() { FontSize = 15 };
                distance.SetBinding(Label.TextProperty, "Distance", stringFormat:"Дистанция в заказе: {0}");
                Label price = new Label() { FontSize = 15 };
                price.SetBinding(Label.TextProperty, "Price", stringFormat:"Цена: {0} Р.");
                Label rate = new Label() { FontSize = 15 };
                rate.SetBinding(Label.TextProperty, "Rate", stringFormat:"Тариф: {0}");
                Label time = new Label() { FontSize = 15 };
                time.SetBinding(Label.TextProperty, "TimeStart", stringFormat:"Время начала: {0}");

                ViewCell viewCell = new ViewCell
                {
                    View = new StackLayout
                    {
                        Children = { from, to, distance, price, rate, time },
                        Padding = new Thickness(0, 5),
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
            if (_isPick == false)
            {
                JsonOrder order = e.Item as JsonOrder;

                if (await DataBaseApi.GetStatusOrderById(order.Id) == "search")
                {
                    bool isPick = await DisplayAlert("Подтверждение", "Вы точно хотите взять этот заказ?", "Да", "Нет");

                    if (isPick)
                    {
                        _isPick = true;
                        bool isAnswer = false;
                        string statusOrder;
                        App.Current.Properties.TryGetValue("login", out object login);
                        DateTime timeToCansel = DateTime.Now.Add(new TimeSpan(0, 1, 0));

                        CanselOrderDriver.IsEnabled = true;
                        Sleep.IsEnabled = false;
                        _idOrder = order.Id;

                        DataBaseApi.PickOrderDriver(order.Id, $"{login}");

                        while (isAnswer == false)
                        {
                            await Task.Delay(new TimeSpan(0, 0, 5));

                            if (timeToCansel.Subtract(DateTime.Now).Seconds > 0)
                            {
                                statusOrder = await DataBaseApi.GetStatusOrderById(order.Id);

                                switch (statusOrder)
                                {
                                    case "canseled":
                                    case "search":
                                        if (_isDoCansel == false)
                                        {
                                            DisplayAlert("Отказ", "К сожалению пассажир отказался от поездки.", "Ок");
                                            _isPick = false;
                                            isAnswer = true;
                                            CanselOrderDriver.IsEnabled = false;
                                            Sleep.IsEnabled = true;
                                        }
                                        else
                                        {
                                            isAnswer = true;
                                            _isPick = false;
                                            _isDoCansel = false;
                                            CanselOrderDriver.IsEnabled = false;
                                            Sleep.IsEnabled = true;
                                        }
                                        break;

                                    case "waitingDriver":
                                        DataBaseApi.SetDriverStatusToDriveByLogin($"{login}");
                                        isAnswer = true;
                                        _flyoutMenu.DriverState = "drive";
                                        _flyoutMenu.TaxameterPage = new TaxameterPage(_flyoutMenu, order.Id);
                                        await Navigation.PushAsync(_flyoutMenu.TaxameterPage);
                                        break;

                                    case "searched":
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                DataBaseApi.ReupdateStatusToSearchByIdOrder(order.Id);
                                DisplayAlert("Отказ", "К сожалению пассажир слишком долго думал.", "Ок");
                                _isPick = false;
                                isAnswer = true;
                                CanselOrderDriver.IsEnabled = false;
                                Sleep.IsEnabled = true;
                            }
                        }
                    }
                }
                else
                {
                    DisplayAlert("Ошибка", "Другой таксист уже взял этот заказ.", "Ок");
                }
            }
            else
            {
                DisplayAlert("Ошибка", "Вы уже ждете ответ от другого заказа.", "Ок");
            }
        }

        private async void AutoReloadOrders(object sender, EventArgs e)
        {
            _autoReload = ((Switch)sender).IsToggled;

            while(_autoReload)
            {
                await Task.Delay(new TimeSpan(0, 0, 10));

                LoadOrders();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}