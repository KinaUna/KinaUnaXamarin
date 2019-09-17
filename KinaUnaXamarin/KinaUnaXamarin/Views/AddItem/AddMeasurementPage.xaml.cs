using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Multilingual;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMeasurementPage : ContentPage
    {
        private readonly AddMeasurementViewModel _viewModel;
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddMeasurementPage()
        {
            _viewModel = new AddMeasurementViewModel();
            _viewModel.MeasurementItem = new Measurement();
            _viewModel.MeasurementItem.Date = _viewModel.Date = DateTime.Now;
            _viewModel.MeasurementItem.AccessLevel = 0;
            
            InitializeComponent();
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
            }

            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(progeny);
                }

                string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                Progeny viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                if (viewProgeny != null)
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                }
            }
            _viewModel.MeasurementItem.ProgenyId = ((Progeny)ProgenyCollectionView.SelectedItem).Id;
            _viewModel.MeasurementItem.Date = DateTime.Now;
            _viewModel.MeasurementItem.AccessLevel = 0;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
                SaveMeasurementButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveMeasurementButton.IsEnabled = true;
            }
        }


        private async void CancelMeasurementButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveMeasurementButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                
                Measurement saveMeasurement = new Measurement();
                saveMeasurement.ProgenyId = progeny.Id;
                saveMeasurement.AccessLevel = _viewModel.AccessLevel;
                saveMeasurement.Progeny = progeny;
                saveMeasurement.Date = MeasurementDatePicker.Date;
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveMeasurement.Author = userinfo.UserId;
                saveMeasurement.EyeColor = EyeColorEntry.Text;
                saveMeasurement.HairColor = HairColorEntry.Text;
                saveMeasurement.CreatedDate = DateTime.Now;
                bool circumFerenceValid = double.TryParse(CircumferenceEntry.Text, out double circumference);
                if (circumFerenceValid)
                {
                    saveMeasurement.Circumference = circumference;
                }

                bool heightValid = double.TryParse(HeightEntry.Text, out double height);
                if (heightValid)
                {
                    saveMeasurement.Height = height;
                }

                bool weightValid = double.TryParse(WeightEntry.Text, out double weight);
                if (weightValid)
                {
                    saveMeasurement.Weight = weight;
                }

                if (ProgenyService.Online())
                {
                    // Todo: Translate messages.
                    saveMeasurement = await ProgenyService.SaveMeasurement(saveMeasurement);
                    if (saveMeasurement.MeasurementId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorMeasurementNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("MeasurementSaved", ci) + saveMeasurement.MeasurementId;
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveMeasurementButton.IsVisible = false;
                        CancelMeasurementButton.Text = "Ok";
                        CancelMeasurementButton.BackgroundColor = Color.FromHex("#4caf50");
                    }
                }
                else
                {
                    // Todo: Translate message.
                    ErrorLabel.Text = $"Error: No internet connection. Measurement for {progeny.NickName} was not saved. Try again later.";
                    ErrorLabel.BackgroundColor = Color.Red;
                }
                
                ErrorLabel.IsVisible = true;
            }

            _viewModel.IsBusy = false;
        }
    }
}