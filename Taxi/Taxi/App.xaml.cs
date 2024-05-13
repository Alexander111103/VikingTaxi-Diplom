using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new Authorization());
            MainPage = new NavigationPage(new TaxameterPage(new FlyoutMenu(), 299));
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {
                
        }
    }
}
