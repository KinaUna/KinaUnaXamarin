using System;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.ViewModels;
using Plugin.Multilingual;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        private HelpViewModel _viewModel;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        readonly Lazy<ResourceManager> _resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        public HelpPage()
        {
            InitializeComponent();
            _viewModel = new HelpViewModel();
            BindingContext = _viewModel;
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
            }
            else
            {
                _viewModel.Online = false;
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
            if (internetAccess != _viewModel.Online)
            {
                _viewModel.Online = internetAccess;
               
            }
        }
        
        private void OptionsToolBarItem_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = !_viewModel.ShowOptions;
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private void GettingStartedButton_OnClicked(object sender, EventArgs e)
        {
            GettingStartedButton.BackgroundColor = Color.Green;
            GettingStartedButton.TextColor = Color.White;
            GettingStartedButton.FontSize = 12.0;
            GettingStartedButton.Margin = new Thickness(0);
            DocsButton.BackgroundColor = Color.SlateGray;
            DocsButton.TextColor = Color.Black;
            DocsButton.FontSize = 10.0;
            DocsButton.Margin = new Thickness(5);
            ReportButton.BackgroundColor = Color.SlateGray;
            ReportButton.TextColor = Color.Black;
            ReportButton.FontSize = 10.0;
            ReportButton.Margin = new Thickness(5);

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string url = _resmgr.Value.GetString("SupportStartLink", ci);
            HelpWebView.Source = url;
        }

        private void DocsButton_OnClicked(object sender, EventArgs e)
        {
            GettingStartedButton.BackgroundColor = Color.SlateGray;
            GettingStartedButton.TextColor = Color.Black;
            GettingStartedButton.FontSize = 10.0;
            GettingStartedButton.Margin = new Thickness(5);
            DocsButton.BackgroundColor = Color.Green;
            DocsButton.TextColor = Color.White;
            DocsButton.FontSize = 12.0;
            DocsButton.Margin = new Thickness(0);
            ReportButton.BackgroundColor = Color.SlateGray;
            ReportButton.TextColor = Color.Black;
            ReportButton.FontSize = 10.0;
            ReportButton.Margin = new Thickness(5);

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string url = _resmgr.Value.GetString("SupportDocsLink", ci);
            HelpWebView.Source = url;
        }

        private void ReportButton_OnClicked(object sender, EventArgs e)
        {
            GettingStartedButton.BackgroundColor = Color.SlateGray;
            GettingStartedButton.TextColor = Color.Black;
            GettingStartedButton.FontSize = 10.0;
            GettingStartedButton.Margin = new Thickness(5);
            DocsButton.BackgroundColor = Color.SlateGray;
            DocsButton.TextColor = Color.Black;
            DocsButton.FontSize = 10.0;
            DocsButton.Margin = new Thickness(5);
            ReportButton.BackgroundColor = Color.Green;
            ReportButton.TextColor = Color.White;
            ReportButton.FontSize = 12.0;
            ReportButton.Margin = new Thickness(0);

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string url = _resmgr.Value.GetString("SupporNewIssueLink", ci);
            HelpWebView.Source = url;
        }
    }
}