﻿using System;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FFImageLoading.Config;
using FFImageLoading.Forms;

namespace KinaUnaXamarin.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //InitNotificationsAsync();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                Xamarin.Forms.Forms.SetFlags("Shell_UWP_Experimental");
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Should add UWP side assembly to rendererAssemblies
                var rendererAssemblies = new[]
                {
                    typeof(Xamarin.Forms.GoogleMaps.UWP.MapRenderer).GetTypeInfo().Assembly,
                    typeof(CachedImage).GetTypeInfo().Assembly,
                    typeof(FFImageLoading.Forms.Platform.CachedImageRenderer).GetTypeInfo().Assembly
                };
                Xamarin.Forms.Forms.Init(e, rendererAssemblies);

                Xamarin.FormsGoogleMaps.Init(SecretKeys.BingMapsKey); // See: https://github.com/amay077/Xamarin.Forms.GoogleMaps
                FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
                FFImageLoading.ImageService.Instance.Initialize(new Configuration());
                OxyPlot.Xamarin.Forms.Platform.UWP.PlotViewRenderer.Init();

                Xamarin.Forms.DependencyService.Register<WabBrowser>();
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        //private async void InitNotificationsAsync()
        //{
        //    var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
        //    Debug.WriteLine($"Received token: {channel.Uri}");
        //    await SecureStorage.SetAsync("PnsHandle", channel.Uri).ConfigureAwait(false);

        //    var hub = new NotificationHub(AzureNotificationsConstants.NotificationHubName, AzureNotificationsConstants.ListenConnectionString);
        //    var result = await hub.RegisterNativeAsync(channel.Uri).ConfigureAwait(false);

        //    if (result.RegistrationId != null)
        //    {
        //        await SecureStorage.SetAsync("RegistrationId", result.RegistrationId).ConfigureAwait(false);
        //    }
        //}
    }
}
