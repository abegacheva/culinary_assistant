using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Culinary_Assistant
{
    public static class UIStyles
    {
        public static Color PrimaryColor =
            ColorTranslator.FromHtml("#E85A4F");

        public static Color AccentColor =
            ColorTranslator.FromHtml("#4CAF50");

        public static Color BackgroundColor =
            ColorTranslator.FromHtml("#FFF8F0");

        public static Color TextColor =
            ColorTranslator.FromHtml("#2E2E2E");

        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = new Font("Segoe UI", 10);
        }

        public static void StyleTextBox(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = Color.White;
            tb.ForeColor = TextColor;

            tb.Font = new Font("Segoe UI", 11, FontStyle.Regular);

            tb.Height = 30;

            tb.Padding = new Padding(8);
        }

        public static void StyleButton(Button btn)
        {
            btn.BackColor = PrimaryColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            MakeRounded(btn, 15);
        }

        public static void MakeRounded(Control c, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(c.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(c.Width - radius, c.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, c.Height - radius, radius, radius, 90, 90);

            path.CloseAllFigures();

            c.Region = new Region(path);
        }

        public static Panel CreateCard()
        {
            Panel card = new Panel();

            card.Width = 230;
            card.Height = 220;

            card.BackColor = Color.White;

            card.Margin = new Padding(15);

            card.Cursor = Cursors.Hand;

            card.Paint += (s, e) =>
            {
                MakeRounded(card, 20);

                e.Graphics.SmoothingMode =
                    SmoothingMode.AntiAlias;

                using (Pen p =
                    new Pen(Color.Gainsboro, 1))
                {
                    e.Graphics.DrawPath(
                        p,
                        RoundedRect(
                            new Rectangle(
                                0,
                                0,
                                card.Width - 1,
                                card.Height - 1),
                            20));
                }
            };

            return card;
        }

        private static GraphicsPath RoundedRect(
            Rectangle bounds,
            int radius)
        {
            int d = radius * 2;

            GraphicsPath path =
                new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();

            return path;
        }
    }
}
