namespace AppLearningEnglish.Routing;

/// <summary>
/// One route table for the app. Learner pages use Vietnamese slugs as the
/// canonical URL; Identity stays at /Identity/Account/* (cookie LoginPath);
/// Admin stays at /Admin/*. Legacy /Customer/{controller}/{action} and
/// /{controller}/{action} stay mapped so old bookmarks and POST forms work.
/// </summary>
public static class AppRoutes
{
    public const string LoginPath = "/Identity/Account/Login";
    public const string AccessDeniedPath = "/Identity/Account/AccessDenied";
    public const string ErrorPath = "/Error";

    /// <summary>Learner hub prefix → Customer controller. Subpages keep action names: /tu-vung/Review.</summary>
    public static readonly LearnerHub[] Hubs =
    [
        new("courses", "khoa-hoc", "Course"),
        new("vocab", "tu-vung", "Vocabulary"),
        new("listening", "luyen-nghe", "Listening"),
        new("speaking", "luyen-noi", "Speaking"),
        new("reading", "luyen-doc", "Reading"),
        new("writing", "luyen-viet", "Writing"),
        new("grammar", "ngu-phap", "Grammar"),
        new("conversation", "hoi-thoai", "Conversation"),
        new("quiz", "quiz", "QuizHub"),
        new("coach", "ai", "Coach"),
        new("topics", "chu-de", "Topic"),
        new("streak", "chuoi-ngay", "Streak"),
        new("progress", "tien-do", "Progress"),
        new("notebook", "ghi-chu", "Notebook"),
        new("reminder", "nhac-hoc", "Reminder"),
        new("placement", "xep-lop", "Placement"),
        new("achievements", "thanh-tich", "Achievement"),
        new("points", "diem", "Point"),
        new("path", "lo-trinh", "Path"),
        new("challenge", "thu-thach", "Challenge"),
        new("notifications", "thong-bao", "Notification"),
        new("community", "cong-dong", "Community"),
        new("help", "huong-dan", "Help"),
        new("search", "tim-kiem", "Search"),
        new("lesson", "bai-hoc", "Lesson", "Details"),
        new("exercise", "bai-tap", "Exercise"),
        new("certificate", "chung-chi", "Certificate", "Verify"),
        new("language", "ngon-ngu", "Language"),
        new("theme", "giao-dien", "Theme")
    ];

    public static IReadOnlyList<AppRoute> All()
    {
        var list = new List<AppRoute>
        {
            Learner("home", "", "Home"),
            Learner("home-vi", "trang-chu", "Home"),
            Learner("error", "Error", "Home", "Error"),
            Learner("my-courses", "khoa-hoc/cua-toi", "Course", "MyCourses"),
            Learner("course-details", "khoa-hoc/{id:int}", "Course", "Details"),
            Learner("leaderboard", "bang-xep-hang", "Point", "Leaderboard"),
            Learner("skills", "tien-do/ky-nang", "Progress", "Skills"),
            Learner("charts", "tien-do/bieu-do", "Progress", "Charts"),
            Learner("vocab-review", "tu-vung/on", "Vocabulary", "Review"),
            Learner("lesson-details", "bai-hoc/{id:int}", "Lesson", "Details"),
            Learner("cert-view", "chung-chi/{id:int}", "Certificate", "Course")
        };

        foreach (var hub in Hubs)
        {
            list.Add(Learner(
                hub.Name,
                $"{hub.Slug}/{{action={hub.DefaultAction}}}/{{id?}}",
                hub.Controller,
                hub.DefaultAction));
        }

        list.Add(new AppRoute(
            "identity-login",
            "Identity/Account/Login",
            Area: "Identity",
            Controller: "Account",
            Action: "Login"));
        list.Add(new AppRoute(
            "identity",
            "Identity/{controller}/{action}/{id?}",
            Area: "Identity"));
        list.Add(new AppRoute(
            "identity-root",
            "Identity",
            Area: "Identity",
            Controller: "Account",
            Action: "Login"));
        list.Add(new AppRoute(
            "admin",
            "Admin/{controller=Dashboard}/{action=Index}/{id?}",
            Area: "Admin"));

        // Extra inbound aliases; Identity conventional route above keeps generated login URLs at /Identity/Account/Login.
        list.Add(new AppRoute("login-vi", "dang-nhap", "Identity", "Account", "Login"));
        list.Add(new AppRoute("register-vi", "dang-ky", "Identity", "Account", "Register"));
        list.Add(new AppRoute("profile-vi", "ho-so", "Identity", "Account", "Profile"));

        list.Add(new AppRoute(
            "areas",
            "{area:exists}/{controller=Home}/{action=Index}/{id?}"));
        list.Add(new AppRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}",
            Area: "Customer"));

        return list;
    }

    private static AppRoute Learner(
        string name,
        string pattern,
        string controller,
        string action = "Index") =>
        new(name, pattern, "Customer", controller, action);

    public readonly record struct LearnerHub(
        string Name,
        string Slug,
        string Controller,
        string DefaultAction = "Index");

    public sealed record AppRoute(
        string Name,
        string Pattern,
        string? Area = null,
        string? Controller = null,
        string? Action = null);
}
