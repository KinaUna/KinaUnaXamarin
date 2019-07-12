using System;
using System.Threading.Tasks;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private AccountViewModel viewModel;
        private bool _online = true;
        public AccountPage()
        {
            InitializeComponent();
            viewModel = new AccountViewModel();
            LoginStackLayout.BindingContext = viewModel;
            viewModel.LoggedIn = false;

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await Reload();
            });
        }

        private async void RegisterButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//register");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            await Reload();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async Task Reload()
        {
            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _online = true;
                OfflineStackLayout.IsVisible = false;
                LogInButton.IsEnabled = true;
                LogOutButton.IsEnabled = true;
                viewModel.Message = "Login expires: " + await UserService.GetAuthAccessTokenExpires();
                viewModel.Username = await UserService.GetUsername();
                viewModel.FullName = await UserService.GetFullname();
                viewModel.Email = await UserService.GetUserEmail();
                viewModel.Timezone = await UserService.GetUserTimezone();
                viewModel.UserId = await UserService.GetUserId();
                bool accessTokenCurrent = UserService.IsAccessTokenCurrent(await UserService.GetAuthAccessTokenExpires());
                string accessToken = await UserService.GetAuthAccessToken();
                if (String.IsNullOrEmpty(accessToken) || !accessTokenCurrent)
                {
                    viewModel.LoggedIn = false;
                }
                else
                {
                    viewModel.LoggedIn = true;
                }

            }
            else
            {
                _online = false;
                OfflineStackLayout.IsVisible = true;
                LogInButton.IsEnabled = false;
                LogOutButton.IsEnabled = false;
            }
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
    }
}