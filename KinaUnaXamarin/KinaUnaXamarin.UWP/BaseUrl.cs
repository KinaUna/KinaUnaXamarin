using System;
using System.IO;
using Windows.Storage;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl))]
namespace KinaUnaXamarin.UWP
{

    public class BaseUrl : IBaseUrl
    {
        public string Get()
        {
            return "ms-appx-web:///";
        }

        public string GetQuillFile()
        {
            return "file:///Assets/QuillEditor.html";
        }

        public string GetQuillHtml()
        {
            string html = "";
            string filename = "Assets/QuillEditor.html";
            html = File.ReadAllText(filename);
            return html;
        }
    }
}
