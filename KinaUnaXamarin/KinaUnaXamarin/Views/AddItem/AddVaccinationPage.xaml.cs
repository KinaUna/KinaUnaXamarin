using System;
using System.Collections.Generic;
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
    public partial class AddVaccinationPage
    {
        private readonly AddVaccinationViewModel _viewModel;
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddVaccinationPage()
        {
            _viewModel = new AddVaccinationViewModel();
            _viewModel.VaccinationItem = new Vaccination();
            _viewModel.VaccinationItem.VaccinationDate = _viewModel.Date = DateTime.Now;
            _viewModel.VaccinationItem.AccessLevel = 0;
            
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
                Progeny viewProgeny = new Progeny();
                if (viewchildParsed)
                {
                    viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                }

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
            _viewModel.VaccinationItem.ProgenyId = ((Progeny)ProgenyCollectionView.SelectedItem).Id;
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
                SaveVaccinationButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveVaccinationButton.IsEnabled = true;
            }
        }


        private async void CancelVaccinationButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveVaccinationButton_OnClicked(object sender, EventArgs e)
        {
            
            if (ProgenyCollectionView.SelectedItem is Progeny progeny)
            {
                _viewModel.IsBusy = true;
                _viewModel.IsSaving = true;

                Vaccination saveVaccination = new Vaccination();
                saveVaccination.ProgenyId = progeny.Id;
                saveVaccination.AccessLevel = _viewModel.AccessLevel;
                saveVaccination.Progeny = progeny;
                saveVaccination.VaccinationDate = VaccinationDatePicker.Date;
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveVaccination.Author = userinfo.UserId;
                saveVaccination.Notes = NotesEditor.Text;
                saveVaccination.VaccinationDescription = DescriptionEditor.Text;
                saveVaccination.VaccinationName = VaccinationNameEntry.Text;

                if (ProgenyService.Online())
                {
                    // Todo: Translate messages.
                    saveVaccination = await ProgenyService.SaveVaccination(saveVaccination);
                    _viewModel.IsBusy = false;
                    _viewModel.IsSaving = false;
                    if (saveVaccination.VaccinationId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorVaccinationNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("VaccinationSaved", ci) + saveVaccination.VaccinationId;
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveVaccinationButton.IsVisible = false;
                        CancelVaccinationButton.Text = "Ok";
                        CancelVaccinationButton.BackgroundColor = Color.FromHex("#4caf50");
                        await Shell.Current.Navigation.PopModalAsync();
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
            _viewModel.IsSaving = false;
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}