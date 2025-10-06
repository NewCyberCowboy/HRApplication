using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HRApplication
{
    public class GradientButton : Button
    {
        public Color StartColor { get; set; } = Color.FromArgb(67, 97, 238);
        public Color EndColor { get; set; } = Color.FromArgb(149, 164, 252);
        public int CornerRadius { get; set; } = 5;

        public GradientButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9);
            this.Cursor = Cursors.Hand;
            this.Size = new Size(100, 30);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var path = GetRoundPath(this.ClientRectangle, CornerRadius))
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                StartColor,
                EndColor,
                45f))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
    }
}