using System;
using Xamarin.Forms;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;

namespace Taxi
{
    public partial class Authorization : ContentPage
    {
        public Authorization()
        {
            InitializeComponent();

            SetSaveValue();
        }

        public Authorization(string login, string password)
        {
            InitializeComponent();

            loginEntry.Text = login;
            passwordEntry.Text = password;
            swichPasswordSave.IsToggled = true;
        }

        private async void Authorization_Click(object sender, EventArgs e)
        {
            buttonLogin.Clicked -= Authorization_Click;
            StartAnimationLoading();

            try
            {
                int countByLogin = await DataBaseApi.GetCountByLogin(loginEntry.Text);

                if (App.Current.Properties.TryGetValue("login", out object login))
                {
                    App.Current.Properties.Remove("login");
                }

                if (App.Current.Properties.TryGetValue("password", out object password))
                {
                    App.Current.Properties.Remove("password");
                }

                if (App.Current.Properties.TryGetValue("saveSwitch", out object saveSwitch))
                {
                    App.Current.Properties.Remove("saveSwitch");
                }

                if (countByLogin == 0)
                {
                    await DisplayAlert("Ошибка", $"Пользователь не найден.", "Ok");

                    buttonLogin.Clicked += Authorization_Click;
                    StopAnimationLoading();
                }
                else
                {
                    bool resultCheckPassword = await DataBaseApi.CheckPassword(loginEntry.Text, passwordEntry.Text);

                    if (!resultCheckPassword)
                    {
                        await DisplayAlert("Ошибка", $"Неверный логин или пароль.", "Ok");

                        buttonLogin.Clicked += Authorization_Click;
                        StopAnimationLoading();
                    }
                    else
                    {
                        StopAnimationLoading();

                        await DisplayAlert("Успех", $"Вы успешно вошли.", "Ok");

                        buttonLogin.Clicked += Authorization_Click;

                        if (App.Current.Properties.TryGetValue("login", out login))
                        {
                            App.Current.Properties.Remove("login");
                        }

                        if (App.Current.Properties.TryGetValue("password", out password))
                        {
                            App.Current.Properties.Remove("password");
                        }

                        App.Current.Properties.Add("login", loginEntry.Text);
                        App.Current.Properties.Add("password", passwordEntry.Text);

                        if (swichPasswordSave.IsToggled)
                        {
                            if (App.Current.Properties.TryGetValue("saveSwitch", out saveSwitch))
                            {
                                App.Current.Properties.Remove("saveSwitch");
                            }

                            App.Current.Properties.Add("saveSwitch", swichPasswordSave.IsToggled);
                        }

                        if (App.Current.Properties.TryGetValue("role", out object _role))
                        {
                            App.Current.Properties.Remove("role");
                        }

                        await Navigation.PushAsync(new FlyoutMenu());
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Ошибка", $"Приложение выдает ошибку.\n{ex.Message}\n\nВероятно ошибка соединения приложения с сервером.\nПопробуйте перезайти через пару минут.", "Ок");

                buttonLogin.Clicked += Authorization_Click;
                StopAnimationLoading();
            }
            catch (WebException ex)
            {
                await DisplayAlert("Ошибка", $"Приложение выдает ошибку.\n{ex.Message}\n\nВероятно ошибка соединения приложения с сервером.\nПопробуйте перезайти через пару минут.", "Ок");

                buttonLogin.Clicked += Authorization_Click;
                StopAnimationLoading();
            }
        }

        private async void Registration_Click(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registration()); 
        }

        private void SetSaveValue()
        {
            if (App.Current.Properties.TryGetValue("saveSwitch", out object saveSwitch))
            {
                swichPasswordSave.IsToggled = (bool)saveSwitch;

                if((bool)saveSwitch == true)
                {

                    if (App.Current.Properties.TryGetValue("login", out object login))
                    {
                        loginEntry.Text = (string)login;
                    }

                    if (App.Current.Properties.TryGetValue("password", out object password))
                    {
                        passwordEntry.Text = (string)password;
                    }
                }
            }
        }

        private void StartAnimationLoading()
        {
            Image loadingImage = new Image { Source = ImageSource.FromResource("Taxi.Images.loading.png"), HeightRequest = 60, WidthRequest = 60 };

            loadingFrame.Content = loadingImage;

            Animation rotation = new Animation(v => loadingImage.Rotation = v, 0, 360);
            rotation.Commit(this, "loading", 16, 2000, Easing.Linear, (v, c) => loadingImage.Rotation = 360, () => true);
        }

        private void StopAnimationLoading()
        {
            this.AbortAnimation("loading");
            loadingFrame.Content = null;
        }
    }
}