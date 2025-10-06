using System.Drawing;

namespace HRApplication
{
    public static class DesignColors
    {
        // Светлая тема (профессиональная)
        public static class LightTheme
        {
            public static Color PrimaryColor = Color.FromArgb(41, 128, 185);    // Профессиональный синий
            public static Color SecondaryColor = Color.FromArgb(52, 152, 219);  // Светлее синий
            public static Color SuccessColor = Color.FromArgb(39, 174, 96);     // Зеленый
            public static Color BackgroundColor = Color.FromArgb(248, 249, 250); // Светло-серый
            public static Color CardColor = Color.White;                        // Белый
            public static Color TextColor = Color.FromArgb(51, 51, 51);         // Темно-серый
            public static Color BorderColor = Color.FromArgb(222, 226, 230);    // Светлая граница
        }

        // Темная тема (профессиональная)
        public static class DarkTheme
        {
            public static Color PrimaryColor = Color.FromArgb(41, 128, 185);    // Тот же синий
            public static Color SecondaryColor = Color.FromArgb(52, 152, 219);  // Светлее синий
            public static Color SuccessColor = Color.FromArgb(39, 174, 96);     // Зеленый
            public static Color BackgroundColor = Color.FromArgb(40, 44, 52);   // Темный фон
            public static Color CardColor = Color.FromArgb(55, 60, 70);         // Темные карточки
            public static Color TextColor = Color.FromArgb(220, 220, 220);      // Светлый текст
            public static Color BorderColor = Color.FromArgb(70, 75, 85);       // Темная граница
        }

        // Текущая тема
        public static bool IsDarkTheme = false;

        public static Color PrimaryColor => IsDarkTheme ? DarkTheme.PrimaryColor : LightTheme.PrimaryColor;
        public static Color SecondaryColor => IsDarkTheme ? DarkTheme.SecondaryColor : LightTheme.SecondaryColor;
        public static Color SuccessColor => IsDarkTheme ? DarkTheme.SuccessColor : LightTheme.SuccessColor;
        public static Color BackgroundColor => IsDarkTheme ? DarkTheme.BackgroundColor : LightTheme.BackgroundColor;
        public static Color CardColor => IsDarkTheme ? DarkTheme.CardColor : LightTheme.CardColor;
        public static Color TextColor => IsDarkTheme ? DarkTheme.TextColor : LightTheme.TextColor;
        public static Color BorderColor => IsDarkTheme ? DarkTheme.BorderColor : LightTheme.BorderColor;
    }
}