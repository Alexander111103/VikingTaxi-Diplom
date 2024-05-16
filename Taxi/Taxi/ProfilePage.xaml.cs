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
    public partial class ProfilePage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public ProfilePage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            App.Current.Properties.TryGetValue("login", out object login);
            App.Current.Properties.TryGetValue("password", out object password);

            changer.Cookies = new CookieContainer();

            changer.Cookies.Add(new Cookie("login", $"{login}", "/", "taxiviking.ru"));
            changer.Cookies.Add(new Cookie("password", $"{password}", "/", "taxiviking.ru"));
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