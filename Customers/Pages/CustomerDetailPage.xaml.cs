﻿using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using FormsToolkit;
using Xamarin;
using System;

namespace Customers
{
    public partial class CustomerDetailPage : ContentPage
    {
        protected CustomerDetailViewModel ViewModel
        {
            get { return BindingContext as CustomerDetailViewModel; }
        }

        public CustomerDetailPage()
        {
            InitializeComponent();
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();

            MessagingService.Current.Subscribe<CustomerDetailViewModel>(MessageKeys.NavigateToEditPage, async (service, viewmodel) =>
                await Navigation.PushAsync(new CustomerEditPage() { BindingContext = ViewModel }));

            await SetupMap();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingService.Current.Unsubscribe<CustomerDetailViewModel>(MessageKeys.NavigateToEditPage);
        }

        async Task SetupMap()
        {
            ViewModel.IsBusy = true;
            Map.IsVisible = false;

            // set to a default posiion
            Position position;

            try
            {
                position = await ViewModel.GetPosition();
            }
            catch (Exception ex)
            {
                MessagingService.Current.SendMessage(MessageKeys.DisplayGeocodingError);

                ViewModel.IsBusy = false;

                Insights.Report(ex, Insights.Severity.Error);

                return;
            }

            // if lat and lon are both 0, then it's assumed that position acquisition failed
            if (position.Latitude == 0 && position.Longitude == 0)
            {
                MessagingService.Current.SendMessage(MessageKeys.DisplayGeocodingError);

                ViewModel.IsBusy = false;

                return;
            }
            else
            {
                var pin = new Pin()
                { 
                    Type = PinType.Place, 
                    Position = position,
                    Label = ViewModel.Customer.DisplayName, 
                        Address = ViewModel.Customer.AddressString 
                };

                Map.Pins.Clear();

                Map.Pins.Add(pin);

                Map.MoveToRegion(MapSpan.FromCenterAndRadius(pin.Position, Distance.FromMiles(10)));

                Map.IsVisible = true;
                ViewModel.IsBusy = false;
            }
        }
    }
}

