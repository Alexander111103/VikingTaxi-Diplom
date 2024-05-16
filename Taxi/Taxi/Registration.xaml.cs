using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Registration : ContentPage
    {
        public Registration()
        {
            InitializeComponent();
        }

        private async void Registration_Click(object sender, EventArgs e)
        {
            buttonLogin.Clicked -= Registration_Click;
            StartAnimationLoading();

            try
            {
                if (loginEntry.Text == null || nameEntry.Text == null || lastNameEntry.Text == null || phoneEntry.Text == null || passwordEntry.Text == null || passwordThoEntry.Text == null || loginEntry.Text == "" || nameEntry.Text == "" || lastNameEntry.Text == "" || phoneEntry.Text == "" || passwordEntry.Text == "" || passwordThoEntry.Text == "")
                {
                    await DisplayAlert("Ошибка", $"Вы не заполнили ключевое поле.", "Ok");

                    buttonLogin.Clicked += Registration_Click;
                    StopAnimationLoading();
                }
                else
                {
                    if (!int.TryParse(phoneEntry.Text, out int res))
                    {
                        await DisplayAlert("Ошибка", $"Телефон указан не верно.", "Ok");

                        buttonLogin.Clicked += Registration_Click;
                        StopAnimationLoading();
                    }
                    else 
                    { 
                        if (swichAgree.IsToggled == false)
                        {
                            await DisplayAlert("Ошибка", $"Вы не согласились с пользовательским соглашением.", "Ok");

                            buttonLogin.Clicked += Registration_Click;
                            StopAnimationLoading();
                        }
                        else
                        {
                            if (passwordEntry.Text.Trim().Length < 8)
                            {
                                await DisplayAlert("Ошибка", $"Пароль слишком короткий.\nМинимальная длина 8 символоф.", "Ok");

                                buttonLogin.Clicked += Registration_Click;
                                StopAnimationLoading();
                            }
                            else
                            {
                                if (passwordEntry.Text != passwordThoEntry.Text)
                                {
                                    await DisplayAlert("Ошибка", $"Пароли не совпадают.", "Ok");

                                    buttonLogin.Clicked += Registration_Click;
                                    StopAnimationLoading();
                                }
                                else
                                {
                                    int countByLogin = await DataBaseApi.GetCountByLogin(loginEntry.Text);

                                    if (countByLogin > 0)
                                    {
                                        await DisplayAlert("Ошибка", $"Даннй логин уже занят.", "Ok");

                                        buttonLogin.Clicked += Registration_Click;
                                        StopAnimationLoading();
                                    }
                                    else
                                    {
                                        int countByPhone = await DataBaseApi.GetCountByPhone(phoneEntry.Text);

                                        if (countByPhone > 0) 
                                        {
                                            await DisplayAlert("Ошибка", $"Даннй телефон уже используется.", "Ok");

                                            buttonLogin.Clicked += Registration_Click;
                                            StopAnimationLoading();
                                        }
                                        else
                                        {
                                            bool resultTryRegistration = await DataBaseApi.TryRegistration(loginEntry.Text, passwordEntry.Text, nameEntry.Text, lastNameEntry.Text, phoneEntry.Text);

                                            if (!resultTryRegistration)
                                            {
                                                await DisplayAlert("Ошибка", $"Что-то пошло не так", "Ok");

                                                buttonLogin.Clicked += Registration_Click;
                                                StopAnimationLoading();
                                            }
                                            else
                                            {
                                                StopAnimationLoading();

                                                await DisplayAlert("Успех", $"Вы успешно зарегистрированны.", "Ok");

                                                buttonLogin.Clicked += Registration_Click;

                                                await Navigation.PushAsync(new Authorization(loginEntry.Text, passwordEntry.Text));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(HttpRequestException ex)
            {
                await DisplayAlert("Ошибка", $"Приложение выдает ошибку.\n{ex.Message}\n\nВероятно ошибка соединения приложения с сервером.\nПопробуйте перезайти через пару минут.", "Ок");

                buttonLogin.Clicked += Registration_Click;
            }
        }

        private async void CustomResolution_Click(object sender, EventArgs e)
        {
            swichAgree.IsToggled = await DisplayAlert("Пользовательсое Соглашение", "Тут могло быть пользовательское соглашение", "Принять", "Отказаться");
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