using System;
using System.Drawing;
using System.Windows.Forms;

namespace Culinary_Assistant
{
    public partial class CustomMessageBox : Form
    {
        private Label lbl;
        private Button btnOk;
        private Panel container;

        public CustomMessageBox(string text, string title = "Info")
        {
            Text = title;

            Width = 520;
            Height = 300;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UIStyles.BackgroundColor;

            Font = new Font("Segoe UI", 11);

            container = new Panel
            {
                Width = 460,
                Height = 240,
                BackColor = Color.White
            };

            container.Left = (Width - container.Width) / 2;
            container.Top = (Height - container.Height) / 2;

            lbl = new Label
            {
                Text = text,
                ForeColor = UIStyles.TextColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 165,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 15, FontStyle.Regular)
            };

            btnOk = new Button
            {
                Text = "OK",
                Width = 150,
                Height = 50,
                BackColor = UIStyles.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            btnOk.FlatAppearance.BorderSize = 0;

            btnOk.Click += (s, e) => Close();

            btnOk.Left = (container.Width - btnOk.Width) / 2;
            btnOk.Top = 165; 

            container.Controls.Add(lbl);
            container.Controls.Add(btnOk);

            Controls.Add(container);

            Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.Gainsboro, 1))
                {
                    e.Graphics.DrawRectangle(
                        p,
                        container.Left - 1,
                        container.Top - 1,
                        container.Width + 1,
                        container.Height + 1
                    );
                }
            };
        }

        public static void Show(string text, string title = "Info")
        {
            new CustomMessageBox(text, title).ShowDialog();
        }
    }
}
