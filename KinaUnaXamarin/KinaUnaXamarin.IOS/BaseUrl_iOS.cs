using System.IO;
using Xamarin.Forms;
using Foundation;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.IOS;

[assembly: Dependency(typeof(BaseUrl_iOS))]
namespace KinaUnaXamarin.IOS
{

    // Soruce: https://github.com/xamarin/xamarin-forms-samples/tree/master/WorkingWithWebview/iOS
    public class BaseUrl_iOS : IBaseUrl
    {
        public string Get()
        {
            return NSBundle.MainBundle.BundlePath;
        }

        public string GetQuillFile()
        {
            return NSBundle.MainBundle.PathForResource("QuillEditor", "html");
        }

        public string GetQuillHtml()
        {
            string html;
            using (var streamReader = new StreamReader(GetQuillFile()))
            {
                html = streamReader.ReadToEnd();
            }

            return html;
        }
    }
}