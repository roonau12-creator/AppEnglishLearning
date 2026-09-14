using Microsoft.AspNetCore.Http;

namespace AppLearningEnglish
{
    public static class AppUi
    {
        public const string CookieName = "ui-lang";

        public static string Lang(HttpContext http)
        {
            var cookie = http.Request.Cookies[CookieName];
            return cookie == "en" ? "en" : "vi";
        }

        public static string T(HttpContext http, string key) => T(Lang(http), key);

        public static string T(string lang, string key)
        {
            if (lang == "en" && En.TryGetValue(key, out var en))
            {
                return en;
            }

            return Vi.TryGetValue(key, out var vi) ? vi : key;
        }

        private static readonly Dictionary<string, string> Vi = new()
        {
            ["nav.home"] = "Trang chủ",
            ["nav.courses"] = "Khóa học",
            ["nav.mine"] = "Khóa của tôi",
            ["nav.practice"] = "Luyện",
            ["nav.more"] = "Thêm",
            ["nav.vocab"] = "Từ vựng",
            ["nav.quiz"] = "Quiz",
            ["nav.coach"] = "AI hỗ trợ",
            ["nav.search"] = "Tìm khóa, bài, từ..."
        };

        private static readonly Dictionary<string, string> En = new()
        {
            ["nav.home"] = "Home",
            ["nav.courses"] = "Courses",
            ["nav.mine"] = "My courses",
            ["nav.practice"] = "Practice",
            ["nav.more"] = "More",
            ["nav.vocab"] = "Vocabulary",
            ["nav.quiz"] = "Quiz",
            ["nav.coach"] = "AI coach",
            ["nav.search"] = "Search courses, lessons, words..."
        };
    }
}
