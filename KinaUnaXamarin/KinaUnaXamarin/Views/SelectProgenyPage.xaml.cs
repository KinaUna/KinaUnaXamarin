using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Models.KinaUna;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectProgenyPage : ContentPage
    {
        private readonly ObservableCollection<Progeny> _progenyList;
        public SelectProgenyPage(ObservableCollection<Progeny> progenyList)
        {
            InitializeComponent();
            _progenyList = progenyList;
            ProgenyListCollectionView.ItemsSource = _progenyList;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (!internetAccess)
            {
                foreach (Progeny progeny in _progenyList)
                {
                    string documentsPath = FileSystem.CacheDirectory;
                    string localFilename = "progenyprofile" + progeny.Id + ".jpg";
                    string progenyProfileFile = Path.Combine(documentsPath, localFilename);
                    if (File.Exists(progenyProfileFile))
                    {
                        progeny.PictureLink = progenyProfileFile;
                    }
                    else
                    {
                        progeny.PictureLink = "childicon.png";
                    }
                }
            }
        }

        private async void ProgenyListCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int childId = Constants.DefaultChildId;
            var selected = ProgenyListCollectionView.SelectedItem;
            foreach (Progeny progeny in _progenyList)
            {
                if (progeny.NickName == (selected as Progeny)?.NickName)
                {
                    childId = progeny.Id;
                }

                var networkAccess = Connectivity.NetworkAccess;
                bool internetAccess = networkAccess == NetworkAccess.Internet;
                if (internetAccess)
                {
                    string documentsPath = FileSystem.CacheDirectory;
                    string localFilename = "progenyprofile" + progeny.Id + ".jpg";
                    string progenyProfileFile = Path.Combine(documentsPath, localFilename);
                    var webClient = new WebClient();
                    webClient.DownloadDataCompleted += (s, ev) => {
                            var bytes = ev.Result; // get the downloaded data

                            File.WriteAllBytes(progenyProfileFile, bytes); // writes to local storage
                    };
                    var progenyProfileUrl = new Uri(progeny.PictureLink);
                    webClient.DownloadDataAsync(progenyProfileUrl);
                }
                
            }
            
            await SecureStorage.SetAsync(Constants.UserViewChildKey, childId.ToString());
            MessagingCenter.Send(this, "Reload");
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}