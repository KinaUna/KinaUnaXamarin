using System.IO;
using KinaUnaXamarin.Droid;
using KinaUnaXamarin.Helpers;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl_Android))]
namespace KinaUnaXamarin.Droid
{
    // Source: https://github.com/xamarin/xamarin-forms-samples/tree/master/WorkingWithWebview
    class BaseUrl_Android : IBaseUrl
    {
        public string Get()
        {
            return "file:///android_asset/";
        }

        public string GetQuillFile()
        {
            return "file:///android_asset/QuillEditor.html";
        }

        public string GetQuillHtml()
        {
            string html = "";
            using (var streamReader = new StreamReader(Android.App.Application.Context.Assets.Open("QuillEditor.html")))
            {
                html = streamReader.ReadToEnd();
            }

            return html;
        }
    }
}