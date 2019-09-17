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
    public partial class AddSkillPage : ContentPage
    {
        private readonly AddSkillViewModel _viewModel;
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddSkillPage()
        {
            _viewModel = new AddSkillViewModel();
            _viewModel.SkillItem = new Skill();
            _viewModel.SkillItem.SkillFirstObservation = _viewModel.Date = DateTime.Now;
            _viewModel.SkillItem.AccessLevel = 0;
            
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
            _viewModel.SkillItem.ProgenyId = ((Progeny)ProgenyCollectionView.SelectedItem).Id;
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
                SaveSkillButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveSkillButton.IsEnabled = true;
            }
        }


        private async void CancelSkillButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveSkillButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                
                Skill saveSkill = new Skill();
                saveSkill.ProgenyId = progeny.Id;
                saveSkill.AccessLevel = _viewModel.AccessLevel;
                saveSkill.Progeny = progeny;
                saveSkill.SkillFirstObservation = FirstObservationDatePicker.Date;
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveSkill.Author = userinfo.UserId;
                saveSkill.SkillAddedDate = DateTime.Now;
                saveSkill.Category = CategoryEntry.Text;
                saveSkill.Description = DescriptionEditor.Text;
                saveSkill.Name = SkillNameEntry.Text;

                if (ProgenyService.Online())
                {
                    // Todo: Translate messages.
                    saveSkill = await ProgenyService.SaveSkill(saveSkill);
                    if (saveSkill.SkillId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorSkillNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("SkillSaved", ci) + saveSkill.SkillId;
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveSkillButton.IsVisible = false;
                        CancelSkillButton.Text = "Ok";
                        CancelSkillButton.BackgroundColor = Color.FromHex("#4caf50");
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