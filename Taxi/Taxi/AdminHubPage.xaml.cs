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
            string password = await DataBaseApi.GetPasswordByLogin($"{login}");

            panel.Cookies = new CookieContainer();

            panel.Cookies.Add(new Cookie("userLogin", $"{login}", "/", "taxiviking.ru"));
            panel.Cookies.Add(new Cookie("userPassword", password, "/", "taxiviking.ru"));
        }
    }
}