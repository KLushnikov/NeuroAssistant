using System.Windows;
using System.Windows.Controls;

namespace NeuroAssistant.Core
{
    public class WebBrowserHelper
    {
        public static readonly DependencyProperty HtmlContentProperty =
            DependencyProperty.RegisterAttached("HtmlContent",
                                                typeof(string),
                                                typeof(WebBrowserHelper),
                                                new PropertyMetadata(null, OnHtmlContentChanged));

        public static string GetHtmlContent(DependencyObject obj)
            => (string)obj.GetValue(HtmlContentProperty);

        public static void SetHtmlContent(DependencyObject obj, string value)
            => obj.SetValue(HtmlContentProperty, value);

        private static void OnHtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebBrowser browser && e.NewValue is string html)
            {
                browser.NavigateToString(html);
            }
        }
    }
}
