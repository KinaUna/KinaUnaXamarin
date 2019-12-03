using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using KinaUnaXamarin.Controls;
using KinaUnaXamarin.IOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(ExtendedWebViewRenderer))]
namespace KinaUnaXamarin.IOS.Renderers
{
    class ExtendedWebViewRenderer : WebViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            try
            {
                base.OnElementChanged(e);

                if (NativeView != null)
                {
                    var webView = (UIWebView)NativeView;

                    webView.Opaque = false;
                    webView.BackgroundColor = UIColor.Clear;
                }

                Delegate = new ExtendedUIWebViewDelegate(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error at ExtendedWebViewRenderer OnElementChanged: " + ex.Message);
            }
        }
    }

    class ExtendedUIWebViewDelegate : UIWebViewDelegate
    {
        ExtendedWebViewRenderer webViewRenderer;

        public ExtendedUIWebViewDelegate(ExtendedWebViewRenderer _webViewRenderer = null)
        {
            webViewRenderer = _webViewRenderer ?? new ExtendedWebViewRenderer();
        }

        public override async void LoadingFinished(UIWebView webView)
        {
            try
            {
                var _webView = webViewRenderer.Element as ExtendedWebView;
                if (_webView != null)
                {
                    await System.Threading.Tasks.Task.Delay(100); // wait here till content is rendered
                    _webView.HeightRequest = (double)webView.ScrollView.ContentSize.Height;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error at ExtendedWebViewRenderer LoadingFinished: " + ex.Message);
            }
        }
    }
}