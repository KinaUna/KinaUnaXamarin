using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Runtime;
using Android.OS;
using Android.Widget;
using FFImageLoading.Forms.Platform;
using PanCardView.Droid;
using Xamarin.Forms;
using Configuration = FFImageLoading.Config.Configuration;

namespace KinaUnaXamarin.Droid
{
    [Activity(Label = "KinaUna Xamarin", Icon = "@mipmap/icon", Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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
            //Android.Glide.Forms.Init(); // https://github.com/jonathanpeppers/glidex
            FFImageLoading.Forms.Platform.CachedImageRenderer
                .Init(enableFastRenderer: true); // See: https://github.com/luberda-molinet/FFImageLoading/wiki/Xamarin.Forms-API
            CachedImageRenderer
                .InitImageViewHandler(); // See: https://github.com/luberda-molinet/FFImageLoading/wiki/Xamarin.Forms-API
            FFImageLoading.ImageService.Instance.Initialize(new Configuration());
            CardsViewRenderer.Preserve(); // See: https://github.com/AndreiMisiukevich/CardView
            LoadApplication(new App());
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
            else
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
        }
    }
}