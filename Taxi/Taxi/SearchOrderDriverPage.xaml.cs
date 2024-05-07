using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchOrderDriverPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public SearchOrderDriverPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;
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

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}