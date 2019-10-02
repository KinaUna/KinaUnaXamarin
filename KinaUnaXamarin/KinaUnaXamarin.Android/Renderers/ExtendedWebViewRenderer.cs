using Android.Content;
using Android.Webkit;
using KinaUnaXamarin.Controls;
using KinaUnaXamarin.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebView = Android.Webkit.WebView;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(ExtendedWebViewRenderer))]
namespace KinaUnaXamarin.Droid.Renderers
{
    // Source: http://lukealderton.com/blog/posts/2016/may/autocustom-height-on-xamarin-forms-webview-for-android-and-ios/

    public class ExtendedWebViewRenderer : WebViewRenderer
    {
        static ExtendedWebView _xwebView = null;
        WebView _webView;
        Context _context;

        public ExtendedWebViewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        class ExtendedWebViewClient : Android.Webkit.WebViewClient
        {
            public override async void OnPageFinished(WebView view, string url)
            {
                if (_xwebView != null)
                {
                    int i = 10;
                    while (view.ContentHeight == 0 && i-- > 0) // wait here till content is rendered
                        await System.Threading.Tasks.Task.Delay(100);
                    _xwebView.HeightRequest = view.ContentHeight;
                }
                view.Settings.JavaScriptEnabled = true;
                view.Settings.DomStorageEnabled = true;
                view.Settings.SetPluginState(WebSettings.PluginState.On);
                
                view.SetWebChromeClient(new WebChromeClient());
                
                base.OnPageFinished(view, url);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            _xwebView = e.NewElement as ExtendedWebView;
            if (Control != null)
            {
                Control.Settings.JavaScriptEnabled = true;
                Control.SetWebChromeClient(new WebChromeClient());
            }
            _webView = Control;
            

            if (e.OldElement == null)
            {
                _webView.SetWebViewClient(new ExtendedWebViewClient());
            }
        }
    }
}