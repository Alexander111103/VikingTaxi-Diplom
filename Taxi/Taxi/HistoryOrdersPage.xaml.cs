﻿using System;
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

            refreshView.Command = new Command(LoadOrders);
            LoadOrders();
        }

        private async void LoadOrders()
        {
            refreshView.IsRefreshing = true;
            App.Current.Properties.TryGetValue("login", out object login);
            listView.ItemsSource = (await DataBaseApi.GetOrdersHistoryUserByLogin($"{login}")).Orders;
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label from = new Label() { FontSize = 15 };
                from.SetBinding(Label.TextProperty, "StartShort", stringFormat: "Из: {0}");
                Label to = new Label() { FontSize = 15 };
                to.SetBinding(Label.TextProperty, "FinishShort", stringFormat: "Из: {0}");
                Label price = new Label() { FontSize = 15 };
                price.SetBinding(Label.TextProperty, "Price", stringFormat: "{0} Р");
                Label rating = new Label() { FontSize = 15 };
                rating.SetBinding(Label.TextProperty, "Rating", stringFormat: "Оценка: {0}");
                Label date = new Label() { FontSize = 15 };
                date.SetBinding(Label.TextProperty, "Date", stringFormat: "Дата: {0}");

                ViewCell viewCell = new ViewCell
                {
                    View = new StackLayout
                    {
                        Children = { from, to, price, rating, date },
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