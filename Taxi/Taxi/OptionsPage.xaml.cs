using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OptionsPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public OptionsPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;
        }

        private void ResetSavePassword_Click(object sender, EventArgs e)
        {
            if (App.Current.Properties.TryGetValue("saveSwitch", out object saveSwitch))
            {
                App.Current.Properties.Remove("saveSwitch");
            }

            DisplayAlert("Успех", "готово.", "ОК");
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