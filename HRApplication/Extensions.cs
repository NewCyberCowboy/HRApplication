using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HRApplication
{
    public static class ControlExtensions
    {
        public static void SetBorderRadius(this Control control, int radius)
        {
            control.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius));
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        internal static extern System.IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}