using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.CustomTabs;
using IdentityModel.OidcClient.Browser;
using Plugin.CurrentActivity;

namespace KinaUnaXamarin.Droid
{
    // Source: https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/master/XamarinForms/XamarinFormsClient/XamarinFormsClient.Android/ChromeCustomTabsBrowser.cs
    public class ChromeCustomTabsBrowser : IBrowser
    {
        private readonly Activity _context;
        private readonly CustomTabsActivityManager _manager;

        public ChromeCustomTabsBrowser() : this(CrossCurrentActivity.Current.Activity) { }


        public ChromeCustomTabsBrowser(Activity context)
        {
            _context = context;
            _manager = new CustomTabsActivityManager(_context);
        }

        public Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            var task = new TaskCompletionSource<BrowserResult>();

            var builder = new CustomTabsIntent.Builder(_manager.Session)
                .SetToolbarColor(Color.Argb(255, 52, 152, 219))
                .SetShowTitle(true)
                .EnableUrlBarHiding()
                .SetStartAnimations(_context, Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight)
                .SetExitAnimations(_context, Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);

            var customTabsIntent = builder.Build();

            // ensures the intent is not kept in the history stack, which makes
            // sure navigating away from it will close it
            customTabsIntent.Intent.AddFlags(ActivityFlags.NoHistory);

            Action<string> callback = null;
            callback = url =>
            {
                OidcCallbackActivity.Callbacks -= callback;

                task.SetResult(new BrowserResult()
                {
                    Response = url
                });
            };

            OidcCallbackActivity.Callbacks += callback;

            customTabsIntent.LaunchUrl(_context, Android.Net.Uri.Parse(options.StartUrl));

            return task.Task;
        }
    }
}