using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading;

namespace Taxi
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryOrdersPage : ContentPage
	{
        private FlyoutMenu _flyoutMenu;

        public HistoryOrdersPage(FlyoutMenu menu)
		{
			InitializeComponent ();

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