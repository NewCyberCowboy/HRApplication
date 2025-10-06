using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HRApplication
{
    public class ModernTextBox : TextBox
    {
        public string PlaceholderText { get; set; } = "";
        public int CornerRadius { get; set; } = 8;
        public Color BorderColor { get; set; } = Color.LightGray;
        public int BorderSize { get; set; } = 1;

        public ModernTextBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.Font = new Font("Segoe UI", 10);
            this.Padding = new Padding(10, 8, 10, 8);
            this.BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Для TextBox стандартный OnPaint не переопределяется так легко
            // Вместо этого используем OnPaintBackground
            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (var path = GetRoundPath(this.ClientRectangle, CornerRadius))
            using (var brush = new SolidBrush(this.BackColor))
            using (var pen = new Pen(BorderColor, BorderSize))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Обработка сообщения отрисовки
            if (m.Msg == 0xF || m.Msg == 0x133) // WM_PAINT или WM_CTLCOLOREDIT
            {
                if (string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(PlaceholderText) && !this.Focused)
                {
                    using (var graphics = this.CreateGraphics())
                    using (var brush = new SolidBrush(Color.Gray))
                    {
                        graphics.DrawString(PlaceholderText, this.Font, brush,
                            new PointF(Padding.Left, Padding.Top));
                    }
                }
            }
        }
    }
}