using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading;

namespace Taxi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaxameterPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;
        private int _idOrder;

        public TaxameterPage(FlyoutMenu menu, int idOrder)
        {
            InitializeComponent();

            _flyoutMenu = menu;
            _idOrder = idOrder;
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