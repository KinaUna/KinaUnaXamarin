using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Webkit;
using KinaUnaXamarin.Controls;
using KinaUnaXamarin.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebView = Android.Webkit.WebView;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(ExtendedWebViewRenderer))]
namespace KinaUnaXamarin.Droid.Renderers
{
    // Source: https://github.com/xamarin/Xamarin.Forms/issues/1711
    public class ExtendedWebViewRenderer : WebViewRenderer
    {
        public static int _webViewHeight;
        static ExtendedWebView _xwebView = null;
        WebView _webView;

        public ExtendedWebViewRenderer(Context context) : base(context)
        {
        }

        class ExtendedWebViewClient : WebViewClient
        {
            WebView _webView;
            public override async void OnPageFinished(WebView view, string url)
            {
                try
                {
                    _webView = view;
                    if (_xwebView != null)
                    {

                        view.Settings.JavaScriptEnabled = true;
                        await Task.Delay(100);
                        string result = await _xwebView.EvaluateJavaScriptAsync("(function(){return document.body.scrollHeight;})()");
                        _xwebView.HeightRequest = Convert.ToDouble(result) + 25.0;
                    }
                    base.OnPageFinished(view, url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            
        }
        
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            _xwebView = e.NewElement as ExtendedWebView;
            _webView = Control;

            if (e.OldElement == null)
            {
                _webView.SetWebViewClient(new ExtendedWebViewClient());
            }

        }
    }
}