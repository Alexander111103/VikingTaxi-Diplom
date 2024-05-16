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
    public partial class HistoryOrdersDriverPage : ContentPage
    {
        private FlyoutMenu _flyoutMenu;

        public HistoryOrdersDriverPage(FlyoutMenu menu)
        {
            InitializeComponent();

            _flyoutMenu = menu;

            refreshView.Command = new Command(LoadOrders);
            LoadOrders();
        }

        private async void LoadOrders()
        {
            refreshView.IsRefreshing = true;
            App.Current.Properties.TryGetValue("login", out object login);
            listView.ItemsSource = (await DataBaseApi.GetOrdersHistoryDriverByLogin($"{login}")).Orders;
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label from = new Label() { FontSize = 15 };
                from.SetBinding(Label.TextProperty, "StartShort", stringFormat: "Из: {0}");
                Label to = new Label() { FontSize = 15 };
                to.SetBinding(Label.TextProperty, "FinishShort", stringFormat: "Из: {0}");
                Label time = new Label() { FontSize = 15 };
                time.SetBinding(Label.TextProperty, "TimeFinish", stringFormat: "Время завершения: {0}");
                Label date = new Label() { FontSize = 15 };
                date.SetBinding(Label.TextProperty, "Date", stringFormat: "Дата: {0}");

                ViewCell viewCell = new ViewCell
                {
                    View = new StackLayout
                    {
                        Children = { from, to, time, date},
                        Padding = new Thickness(0, 5),
                        Orientation = StackOrientation.Vertical
                    }
                };

                return viewCell;
            });
            refreshView.IsRefreshing = false;
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