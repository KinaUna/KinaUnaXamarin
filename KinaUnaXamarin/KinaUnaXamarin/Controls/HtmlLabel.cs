using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace KinaUnaXamarin.Controls
{
    // Source: https://stackoverflow.com/questions/46816351/is-there-any-way-i-can-add-html-into-a-xamarin-forms-page
    public class HtmlLabel : Label
    {
        public static readonly BindableProperty HtmlProperty =
            BindableProperty.Create(
                "Html", typeof(string), typeof(HtmlLabel),
                defaultValue: default(string));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }
    }
}
