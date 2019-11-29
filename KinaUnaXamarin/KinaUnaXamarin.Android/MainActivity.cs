using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Runtime;
using Android.OS;
using Android.Util;
using Android.Widget;
using FFImageLoading.Forms.Platform;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Views;
using Newtonsoft.Json;
using PanCardView.Droid;
using Plugin.CurrentActivity;
using Xamarin;
using Xamarin.Forms;
using Configuration = FFImageLoading.Config.Configuration;

namespace KinaUnaXamarin.Droid
{
    [Activity(Label = "KinaUna Xamarin", Icon = "@mipmap/icon", Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            DependencyService.Register<ChromeCustomTabsBrowser>();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental",
                "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);  // https://github.com/amay077/Xamarin.Forms.GoogleMaps/blob/master/README.md
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            //Android.Glide.Forms.Init(); // https://github.com/jonathanpeppers/glidex
            FFImageLoading.Forms.Platform.CachedImageRenderer
                .Init(enableFastRenderer: true); // See: https://github.com/luberda-molinet/FFImageLoading/wiki/Xamarin.Forms-API
            CachedImageRenderer
                .InitImageViewHandler(); // See: https://github.com/luberda-molinet/FFImageLoading/wiki/Xamarin.Forms-API
            FFImageLoading.ImageService.Instance.Initialize(new Configuration());
            CardsViewRenderer.Preserve(); // See: https://github.com/AndreiMisiukevich/CardView
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init(); // See: https://oxyplot.readthedocs.io/en/master/getting-started/hello-xamarin-forms.html
            LoadApplication(new App());

            if (IsPlayServiceAvailable() == false)
            {
                throw new Exception("This device does not have Google Play Services and cannot receive push notifications.");
            }

            CreateNotificationChannel();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        bool _doubleBackToExitPressedOnce = false;

        public override void OnBackPressed()
        {
            if (App.Current.MainPage.Navigation.ModalStack.Count > 0)
            {
                base.OnBackPressed();
                return;

            }
            if (App.Current.MainPage is AppShell appShell)
            {
                if (appShell.CurrentItem.Title == "Home")
                {
                    if (_doubleBackToExitPressedOnce)
                    {
                        base.OnBackPressed();
                        Java.Lang.JavaSystem.Exit(0);
                        return;
                    }


                    this._doubleBackToExitPressedOnce = true;
                    Toast.MakeText(this, "Press Back again to exit", ToastLength.Short).Show(); // Todo: Translate the message.

                    new Handler().PostDelayed(() => { _doubleBackToExitPressedOnce = false; }, 2000);
                }
                else
                {
                    appShell.CurrentItem = new HomePage();
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Extras != null)
            {
                var message = intent.GetStringExtra("message");
                var title = intent.GetStringExtra("title");
                var notData = intent.GetStringExtra("notData");
                int tItemNumber = 0;
                int.TryParse(notData, out tItemNumber);
                (App.Current.MainPage as AppShell)?.AddMessage(title, message, tItemNumber);
            }

            base.OnNewIntent(intent);
        }

        bool IsPlayServiceAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    Log.Debug(AzureNotificationsConstants.DebugTag, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    Log.Debug(AzureNotificationsConstants.DebugTag, "This device is not supported");
                }
                return false;
            }
            return true;
        }

        void CreateNotificationChannel()
        {
            // Notification channels are new as of "Oreo".
            // There is no need to create a notification channel on older versions of Android.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = AzureNotificationsConstants.NotificationChannelName;
                var channelDescription = String.Empty;
                var channel = new NotificationChannel(channelName, channelName, NotificationImportance.Default)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}