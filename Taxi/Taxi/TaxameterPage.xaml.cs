using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaxameterPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;
        private int _idOrder;
        private JsonOrder _orderInfo;

        private Stopwatch _timer = new Stopwatch();
        private Taxameter _taxameter;
        private bool _isTimerStart = false;
        private bool _isCheckUserCancel = true;

        public TaxameterPage(FlyoutMenu menu, int idOrder)
        {
            InitializeComponent();

            _flyoutMenu = menu;
            _idOrder = idOrder;

            _taxameter = new Taxameter(idOrder);

            Logo.Source = ImageSource.FromResource("Taxi.Images.logo.png");
            SetActualPrice();
            LoadOrderInfo();

            CheckUserCansel();
            _isTimerStart = true;
            _timer.Restart();
            Device.StartTimer(TimeSpan.FromSeconds(1), TimerTick);
        }

        public void SetToWaitingUserStatus()
        {
            StartButton.Clicked -= StartWaitingUser_Click;
            StartButton.BackgroundColor = Color.FromHex("#f5f578");
            StartButton.Text = "Начать поездку";
            StartButton.Clicked += StartDrive_Click;

            ColorFrame.BackgroundColor = Color.CadetBlue;
            NowStatus.Text = "Статус: Ожидание Пассажира";

            _isTimerStart = false;
        }

        public async void SetToDriveStatus()
        {
            _isTimerStart = false;

            CancelButton.Clicked -= CancelOrder_Click;
            CancelButton.BackgroundColor = Color.FromHex("#eb8034");
            CancelButton.Text = "Пауза";
            CancelButton.Clicked += Pause_Click;

            StartButton.Clicked -= StartWaitingUser_Click;
            StartButton.Clicked -= StartDrive_Click;
            StartButton.BackgroundColor = Color.LightGreen;
            StartButton.Text = "Завершить поездку";
            StartButton.IsEnabled = false;
            StartButton.Clicked += End_Click;

            ColorFrame.BackgroundColor = Color.LightGray;
            NowStatus.Text = "Статус: В поездке";

            _isTimerStart = true;
            _isCheckUserCancel = false;
            _timer.Restart();
            await _taxameter.Start();

            Device.StartTimer(TimeSpan.FromSeconds(1), TimerTick);
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

        private async void Addresses_Click(object sender, EventArgs e)
        {
            if(_orderInfo != null)
            {
                string address = await DisplayActionSheet("Адреса", "Закрыть", null, $"Из: {_orderInfo.StartLong}", $"В: {_orderInfo.FinishLong}");
            
                if(address != null && address != "Закрыть")
                {
                    if(address == $"Из: {_orderInfo.StartLong}")
                    {
                        await new Xamarin.Essentials.Location(Convert.ToDouble(_orderInfo.StartCoorders.Split(',')[0].Replace('.', ',')), Convert.ToDouble(_orderInfo.StartCoorders.Split(',')[1].Replace('.', ','))).OpenMapsAsync();
                    }
                    else if(address == $"В: {_orderInfo.FinishLong}")
                    {
                        await new Xamarin.Essentials.Location(Convert.ToDouble(_orderInfo.FinishCoorders.Split(',')[0].Replace('.', ',')), Convert.ToDouble(_orderInfo.FinishCoorders.Split(',')[1].Replace('.', ','))).OpenMapsAsync();
                    }
                }
            }
        }

        private async void PhoneUser_Click(object sender, EventArgs e)
        {
            string userPhone = await DataBaseApi.GetUserPhoneByOrderId(_idOrder);

            PhoneDialer.Open(userPhone);
        }

        private async void CancelOrder_Click(object sender, EventArgs e)
        {
            bool isCancel = await DisplayAlert("Подтверждение", "Вы точно хотите отменить заказ?", "Да", "Нет");

            if (isCancel)
            { 
                DataBaseApi.ReupdateStatusToSearchByIdOrder(_idOrder);

                PushToSearch();
            }
        }

        private async void StartWaitingUser_Click(object sender, EventArgs e)
        {
            SetToWaitingUserStatus();

            long time = _timer.ElapsedMilliseconds;
            DataBaseApi.SetStatusToWaitingUserByIdOrder(_idOrder, time);
        }

        private async void StartDrive_Click(object sender, EventArgs e)
        {
            SetToDriveStatus();

            DataBaseApi.SetStatusToDriveByIdOrder(_idOrder);            
        }

        private void Pause_Click(object sender, EventArgs e)
        {
            CancelButton.Clicked -= Pause_Click;
            CancelButton.BackgroundColor = Color.FromHex("#f5f578");
            CancelButton.Text = "Продолжить";
            CancelButton.Clicked += CancelPause_Click;

            ColorFrame.BackgroundColor = Color.FromHex("#f5f578");

            StartButton.IsEnabled = true;

            _timer.Stop();
            _isTimerStart = false;
            _taxameter.Stop();
        }

        private async void CancelPause_Click(object sender, EventArgs e)
        {
            CancelButton.Clicked -= CancelPause_Click;
            CancelButton.BackgroundColor = Color.FromHex("#eb8034");
            CancelButton.Text = "Пауза";
            CancelButton.Clicked += Pause_Click;

            ColorFrame.BackgroundColor = Color.LightGray;

            StartButton.IsEnabled = false;

            _timer.Start();
            _isTimerStart = true;
            await _taxameter.Start();

            Device.StartTimer(TimeSpan.FromSeconds(1), TimerTick);
        }

        private void End_Click(object sender, EventArgs e)
        {
            StartButton.IsEnabled = false;
            _taxameter.End();

            NowPrice.Text = $"{_taxameter.GetNowPrice()} Р";
            ColorFrame.BackgroundColor = Color.LightGreen;
            NowStatus.Text = "Статус: Завершен";

            long time = _timer.ElapsedMilliseconds;
            DataBaseApi.SetStatusToEndByIdOrder(_idOrder, time, _taxameter.GetNowPrice());

            CancelButton.Clicked -= CancelPause_Click;
            CancelButton.BackgroundColor = Color.FromHex("#eb8034");
            CancelButton.Text = "Закрыть";
            CancelButton.Clicked += Close_Click;
        }

        private void Close_Click(object sender, EventArgs e)
        {
            ((Button)sender).Clicked -= Close_Click;

            PushToSearch();
        }

        private async void SetActualPrice()
        {
            JsonPrice jsonPrice = await DataBaseApi.GetActualPrice();

            StartPrice.Text = $"Цена посадки: {jsonPrice.StartPrice} Р";
            MinPrice.Text = $"Мин цена: {jsonPrice.MinPrice} Р";
            ToKmPrice.Text = $"Цена за Км: {jsonPrice.PricePerKm} Р";
            ToMinPrice.Text = $"Цена за Мин: {jsonPrice.PricePerMin} Р";
        }

        private async void LoadOrderInfo()
        {
            _orderInfo = await DataBaseApi.GetOrderById(_idOrder);

            if(_orderInfo.PaymentType == "cash")
            {
                NowPaymentTipe.Text = "Способ оплаты: Наличные"; 
            }
        }

        private async void CheckUserCansel()
        {
            while (_isCheckUserCancel)
            {
                await Task.Delay(new TimeSpan(0, 0, 5));

                string status = await DataBaseApi.GetStatusOrderById(_idOrder);

                if(status == "canseled")
                {
                    ShowNotification("Отмена", "Заказ отменен");
                    await DisplayAlert("Заказ отменен", "Пассажир отменил заказ", "Ок");

                    PushToSearch();
                }
            }
        }

        private bool TimerTick()
        {
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)_timer.ElapsedMilliseconds);
            NowDuration.Text = "Время: " + new DateTimeOffset(2024, 1, 1, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss");

            if (_taxameter.IsActive)
            {
                NowDistance.Text = $"Расстояние: {_taxameter.GetNowKm()} КМ.";
                NowPrice.Text = $"{_taxameter.GetNowPrice()} Р";
            }

            return _isTimerStart;
        }

        private async void PushToSearch()
        {
            _isCheckUserCancel = false;
            _isTimerStart = false;

            App.Current.Properties.TryGetValue("login", out object login);
            Xamarin.Essentials.Location location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(2)));
            JsonCar car = await DataBaseApi.GetDriverCurrentCarByDriverLogin($"{login}");

            DataBaseApi.SetDriverStatusToSearchById(car.Owner, car.Id, $"{location.Latitude.ToString().Replace(",", ".")},{location.Longitude.ToString().Replace(",", ".")}");

            _flyoutMenu.DriverState = "search";
            _flyoutMenu.SearchOrderDriverPage = new SearchOrderDriverPage(_flyoutMenu);
            _flyoutMenu.Detail = new NavigationPage(_flyoutMenu.SearchOrderDriverPage);
        }

        private async void ShowNotification(string title, string description)
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            LocalNotificationCenter.Current.Show(new NotificationRequest { Description = description, Title = title });
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}