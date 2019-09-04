using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Extensions;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.MyFamily;
using Plugin.Media;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyChildrenPage : ContentPage
    {
        private readonly MyChildrenViewModel _myChildrenViewModel;
        private bool _online = true;
        private string _filePath;
        private bool _reload;
        private int _selectedProgenyId;

        public MyChildrenPage()
        {
            InitializeComponent();
            _myChildrenViewModel = new MyChildrenViewModel();
            _reload = true;
            BindingContext = _myChildrenViewModel;
            ProgenyCollectionView.ItemsSource = _myChildrenViewModel.ProgenyCollection;
            IReadOnlyCollection<TimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo timeZoneInfo in timeZoneList)
            {
                _myChildrenViewModel.TimeZoneList.Add(timeZoneInfo);
            }
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
            _myChildrenViewModel.IsBusy = true;
            _myChildrenViewModel.EditMode = false;
            _myChildrenViewModel.ProgenyCollection.Clear();
            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    try
                    {
                        TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
                    }
                    catch (Exception)
                    {
                        progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
                    }
                    _myChildrenViewModel.ProgenyCollection.Add(progeny);
                }

                if (_selectedProgenyId == 0)
                {
                    string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                    bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                    Progeny viewProgeny = _myChildrenViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    if (viewProgeny != null)
                    {
                        ProgenyCollectionView.SelectedItem =
                            _myChildrenViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                        ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                    }
                    else
                    {
                        ProgenyCollectionView.SelectedItem = _myChildrenViewModel.ProgenyCollection[0];
                    }
                }
                else
                {
                    ProgenyCollectionView.SelectedItem =
                        _myChildrenViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == _selectedProgenyId);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }

                _myChildrenViewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
                if (_myChildrenViewModel.Progeny.BirthDay.HasValue)
                {
                    _myChildrenViewModel.ProgenyBirthDay = _myChildrenViewModel.Progeny.BirthDay.Value;
                    // BirthdayDatePicker.Date = _myChildrenViewModel.Progeny.BirthDay.Value.Date;
                    // BirthdayTimePicker.Time = _myChildrenViewModel.Progeny.BirthDay.Value.TimeOfDay;
                }

                _selectedProgenyId = _myChildrenViewModel.Progeny.Id;

                TimeZoneInfo progenyTimeZoneInfo =
                    _myChildrenViewModel.TimeZoneList.SingleOrDefault(tz =>
                        tz.DisplayName == _myChildrenViewModel.Progeny.TimeZone);
                _myChildrenViewModel.SelectedTimeZone = progenyTimeZoneInfo;
                //int timeZoneIndex = _myChildrenViewModel.TimeZoneList.IndexOf(progenyTimeZoneInfo);
                //TimeZonePicker.SelectedIndex = timeZoneIndex;
            }
            
            _filePath = "";
            _myChildrenViewModel.IsBusy = false;
        }

        private void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _myChildrenViewModel.EditMode = false;
            EditButton.Text = IconFont.AccountEdit;
            _myChildrenViewModel.Progeny = (Progeny) ProgenyCollectionView.SelectedItem;
            ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
            //if (_myChildrenViewModel.Progeny.BirthDay.HasValue)
            //{
            //    BirthdayDatePicker.Date = _myChildrenViewModel.Progeny.BirthDay.Value.Date;
            //    BirthdayTimePicker.Time = _myChildrenViewModel.Progeny.BirthDay.Value.TimeOfDay;
            //}

            TimeZoneInfo progenyTimeZoneInfo =
                _myChildrenViewModel.TimeZoneList.SingleOrDefault(tz =>
                    tz.DisplayName == _myChildrenViewModel.Progeny.TimeZone);
            _myChildrenViewModel.SelectedTimeZone = progenyTimeZoneInfo;
            ChildPicture.Source = _myChildrenViewModel.ProfilePicture;
            _selectedProgenyId = _myChildrenViewModel.Progeny.Id;
            _filePath = "";
            MessageLabel.Text = "";
            MessageLabel.IsVisible = false;
        }

        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_myChildrenViewModel.EditMode)
            {
                _myChildrenViewModel.EditMode = false;
                _myChildrenViewModel.IsBusy = true;

                // Save changes.
                Progeny updatedProgeny = new Progeny();
                updatedProgeny.Id = _myChildrenViewModel.Progeny.Id;
                updatedProgeny.Name = NameEntry.Text;
                updatedProgeny.NickName = DisplayNameEntry.Text;
                TimeZoneInfo timeZoneInfo = (TimeZoneInfo)TimeZonePicker.SelectedItem;
                string timeZoneName;
                if (TZConvert.TryIanaToWindows(timeZoneInfo.Id, out timeZoneName))
                {
                    updatedProgeny.TimeZone = timeZoneName;
                }
                else
                {
                    updatedProgeny.TimeZone = "Romance Standard Time";
                }

                if (!string.IsNullOrEmpty(_filePath))
                {
                    updatedProgeny.PictureLink = await ProgenyService.UploadProgenyPicture(_filePath);
                }

                string[] admins = AdministratorsEntry.Text.Split(',');
                bool validAdminEmails = true;
                foreach (string str in admins)
                {
                    if (!str.Trim().IsValidEmail())
                    {
                        validAdminEmails = false;
                    }
                }

                if (validAdminEmails)
                {
                    updatedProgeny.Admins = AdministratorsEntry.Text;
                }

                DateTime newBirthDay = new DateTime(BirthdayDatePicker.Date.Year, BirthdayDatePicker.Date.Month, BirthdayDatePicker.Date.Day, BirthdayTimePicker.Time.Hours, BirthdayTimePicker.Time.Minutes, 0);
                updatedProgeny.BirthDay = newBirthDay;

                Progeny resultProgeny = await ProgenyService.UpdateProgeny(updatedProgeny);
                _myChildrenViewModel.IsBusy = false;
                EditButton.Text = IconFont.AccountEdit;
                if (resultProgeny != null)
                {
                    MessageLabel.Text = "Profile saved.";
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;
                
                _myChildrenViewModel.EditMode = true;
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.AccountEdit;
            _myChildrenViewModel.EditMode = false;
            await Reload();
        }

        private async void SelectImageButton_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                RotateImage = false,
                SaveMetaData = true
            });

            if (file == null)
            {
                return;
            }

            _filePath = file.Path;
            ChildPicture.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
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