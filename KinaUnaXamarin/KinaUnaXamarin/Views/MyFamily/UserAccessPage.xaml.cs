using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.MyFamily;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserAccessPage : ContentPage
    {
        private readonly UserAccessViewModel _viewModel;
        private bool _online = true;
        private bool _reload;
        private int _selectedProgenyId;
        public UserAccessPage()
        {
            InitializeComponent();
            _viewModel = new UserAccessViewModel();
            _reload = true;
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
            UserAccessCollectionView.ItemsSource = _viewModel.UserAccessCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            if (_reload)
            {
                _reload = false;
                await Reload();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                await Reload();
            }
        }

        private async Task Reload()
        {
            _viewModel.EditMode = false;
            _viewModel.ProgenyCollection.Clear();
            UserAccessCollectionView.SelectedItem = null;
            _viewModel.SelectedAccess = null;
            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(progeny);
                }

                if (_selectedProgenyId == 0)
                {
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
                else
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == _selectedProgenyId);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }

                _viewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
                
                _selectedProgenyId = _viewModel.Progeny.Id;
                
                List<UserAccess> userAccessList = await ProgenyService.GetProgenyAccessList(_selectedProgenyId);
                _viewModel.UserAccessCollection.Clear();
                foreach (UserAccess ua in userAccessList)
                {
                    ua.AccessLevelString = _viewModel.AccessLevelList[ua.AccessLevel];
                    _viewModel.UserAccessCollection.Add(ua);
                }
            }
        }

        private async void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserAccessCollectionView.SelectedItem = null;
            _viewModel.SelectedAccess = null;
            _viewModel.EditMode = false;
            _viewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
            ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
            _selectedProgenyId = _viewModel.Progeny.Id;

            if (_selectedProgenyId == 0)
            {
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
                    _viewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }
            }
            
            _selectedProgenyId = _viewModel.Progeny.Id;
            
            List<UserAccess> userAccessList = await ProgenyService.GetProgenyAccessList(_selectedProgenyId);
            _viewModel.UserAccessCollection.Clear();
            foreach (UserAccess ua in userAccessList)
            {
                ua.AccessLevelString = _viewModel.AccessLevelList[ua.AccessLevel];
                _viewModel.UserAccessCollection.Add(ua);
            }
        }

        private void UserAccessCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedAccess = (UserAccess) UserAccessCollectionView.SelectedItem;
            if (_viewModel.SelectedAccess != null)
            {
                _viewModel.EditMode = true;
            }
            else
            {
                _viewModel.EditMode = false;
            }
        }

        private async void SaveAccessButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            _viewModel.EditMode = false;
            _viewModel.SelectedAccess.AccessLevel = AccessLevelPicker.SelectedIndex;
            UserAccess updatedUserAccess = await ProgenyService.UpdateUserAccess(_viewModel.SelectedAccess);
            if (updatedUserAccess.AccessId != 0)
            {
                _viewModel.EditMode = false;
                _viewModel.SelectedAccess = null;
                UserAccessCollectionView.SelectedItem = null;
                // Todo: Show success message
                await Reload();
            }
            else
            {
                _viewModel.EditMode = true;
                // Todo: Show failed message
            }

            _viewModel.IsBusy = false;
        }

        private void CancelAccessButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.EditMode = false;
            _viewModel.SelectedAccess = null;
            UserAccessCollectionView.SelectedItem = null;
        }

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            ObservableCollection<Progeny> progenyCollection = new ObservableCollection<Progeny>();
            List<Progeny> progList = await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            foreach (Progeny progeny in progList)
            {
                progenyCollection.Add(progeny);
            }
            SelectProgenyPage selProPage = new SelectProgenyPage(progenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }
    }
}