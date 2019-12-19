using FFImageLoading.Config;
using FFImageLoading.Forms.Platform;
using Foundation;
using PanCardView.iOS;
using UIKit;
using Xamarin.Forms;

namespace KinaUnaXamarin.IOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Forms.SetFlags("CollectionView_Experimental");
            Forms.Init();
            Xamarin.FormsGoogleMaps.Init(SecretKeys.GoogleMapsIOSKey);
            CachedImageRenderer.Init();
            FFImageLoading.ImageService.Instance.Initialize(new Configuration());
            CachedImageRenderer.InitImageSourceHandler();
            CardsViewRenderer.Preserve();
            OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
            DependencyService.Register<ASWebAuthenticationSessionBrowser>();

            LoadApplication(new App());

            return base.FinishedLaunching(application, launchOptions);

            //// create a new window instance based on the screen size
            //Window = new UIWindow(UIScreen.MainScreen.Bounds);
            //Window.RootViewController = new UIViewController();

            //// make the window visible
            //Window.MakeKeyAndVisible();

            //return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background execution this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transition from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}


