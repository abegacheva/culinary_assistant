using System;

namespace Culinary_Assistant
{
    public static class AppLanguage
    {
        public static bool IsRussian = true;

        public static event Action OnLanguageChanged;

        public static void Toggle()
        {
            IsRussian = !IsRussian;
            OnLanguageChanged?.Invoke();
        }
    }
}
