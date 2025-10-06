using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HRApplication
{
    public class GradientPanel : Panel
    {
        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.LightGray;
        public float GradientAngle { get; set; } = 90f;

        public GradientPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                StartColor,
                EndColor,
                GradientAngle))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}