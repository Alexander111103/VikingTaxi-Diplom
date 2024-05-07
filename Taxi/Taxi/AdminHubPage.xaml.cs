using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminHubPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public AdminHubPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            SetCookie();
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

        private async void SetCookie()
        {
            App.Current.Properties.TryGetValue("login", out object login);
            App.Current.Properties.TryGetValue("password", out object password);

            bool isLogin = await DataBaseApi.CheckPassword($"{login}", $"{password}");
            string role = await DataBaseApi.GetRoleByLogin($"{login}");

            if (isLogin && role == "admin")
            {
                panel.Source = "http://taxiviking.ru/admin/";
                panel.Cookies = new CookieContainer();

                panel.Cookies.Add(new Cookie("userLogin", $"{login}", "/", "taxiviking.ru"));
                panel.Cookies.Add(new Cookie("userPassword", $"{password}", "/", "taxiviking.ru"));
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}