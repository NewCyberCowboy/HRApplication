using System.Drawing;

namespace HRApplication.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Reviewed,
        Interview,
        Rejected,
        Accepted
    }

    public static class DesignColors
    {
        public static Color PrimaryColor = Color.FromArgb(74, 107, 255);
        public static Color SecondaryColor = Color.FromArgb(149, 164, 252);
        public static Color BackgroundColor = Color.FromArgb(248, 249, 253);
        public static Color CardColor = Color.White;
        public static Color SuccessColor = Color.FromArgb(46, 204, 113);
        public static Color WarningColor = Color.FromArgb(241, 196, 15);
        public static Color DangerColor = Color.FromArgb(231, 76, 60);
        public static Color TextColor = Color.FromArgb(52, 73, 94);
        public static Color LightTextColor = Color.FromArgb(132, 146, 166);
    }
}
