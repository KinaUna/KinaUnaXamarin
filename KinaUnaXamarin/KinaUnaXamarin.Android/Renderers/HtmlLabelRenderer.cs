using Android.Content;
using Android.Text;
using KinaUnaXamarin.Controls;
using KinaUnaXamarin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HtmlLabel), typeof(HtmlLabelRenderer))]
namespace KinaUnaXamarin.Droid
{
    // Source: https://stackoverflow.com/questions/46816351/is-there-any-way-i-can-add-html-into-a-xamarin-forms-page
    public class HtmlLabelRenderer : LabelRenderer
    {
        public HtmlLabelRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            //if we have a new forms element, we want to update text with font style (as specified in forms-pcl) on native control
            if (e.NewElement != null)
                UpdateTextOnControl();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //if there is change in text or font-style, trigger update to redraw control
            if (e.PropertyName == nameof(HtmlLabel.Html))
            {
                UpdateTextOnControl();
            }
        }

        void UpdateTextOnControl()
        {
            if (Control == null)
                return;

            if (Element is HtmlLabel formsElement)
            {
                var htmlAsString = formsElement.Html ?? string.Empty;      // used by WebView
                var htmlAsSpanned = Html.FromHtml(htmlAsString, FromHtmlOptions.ModeLegacy); // used by TextView

                Control.TextFormatted = htmlAsSpanned;
            }
        }
    }
}