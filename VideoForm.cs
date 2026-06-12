using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

namespace Culinary_Assistant
{
    public partial class VideoForm : Form
    {
        private WebView2 webView;
        private string _query;

        public VideoForm(string query)
        {
            InitializeComponent();

            _query = query;

            WindowState = FormWindowState.Maximized;
            Text = "Видео-рецепт";

            UIStyles.ApplyFormStyle(this);

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(webView);

            Load += VideoForm_Load;
        }

        private async void VideoForm_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            string url =
                "https://rutube.ru/search/?query=" +
                Uri.EscapeDataString("рецепт приготовления " + _query);

            webView.CoreWebView2.Navigate(url);
        }
    }
}
