using AppLearningEnglish.Business.Services;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Identity;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AppLearningEnglish.Business.Service;
using AppLearningEnglish.Business.IService;
using AppLearningEnglish.Routing;
using AppLearningEnglish.Services;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.local.json",
        optional: true,
        reloadOnChange: true);
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new AdminAreaAuthorizationConvention());
}).AddRazorRuntimeCompilation();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<
    IWordMeaningService,
    WordMeaningService>();
builder.Services.AddScoped<
    IWordExampleService,
    WordExampleService>();
builder.Services.AddScoped<
    IExerciseService,
    ExerciseService>();
builder.Services.AddScoped<
    IQuestionService,
    QuestionService>();
builder.Services.AddScoped<
    IAnswerService,
    AnswerService>();
builder.Services.AddScoped<
    IListeningLessonService,
    ListeningLessonService>();
builder.Services.AddScoped<
    IListeningQuestionService,
    ListeningQuestionService>();
builder.Services.AddScoped<IPronunciationScoringService, PronunciationScoringService>();
builder.Services.AddScoped<IUserLessonService, UserLessonService>();
builder.Services.AddScoped<
    IUserVocabularyService,
    UserVocabularyService>();
builder.Services.AddScoped<
    IUserCourseService,
    UserCourseService>();
builder.Services.AddScoped<
    IExerciseAttemptService,
    ExerciseAttemptService>();
builder.Services.AddScoped<
    IAchievementService,
    AchievementService>();
builder.Services.AddScoped<
    IDailyStreakService,
    DailyStreakService>();
builder.Services.AddScoped<IUserPointService, UserPointService>();
builder.Services.AddScoped<ILearningAccessService, LearningAccessService>();
builder.Services.AddScoped<
    IStudyActivityService,
    StudyActivityService>();
builder.Services.AddScoped<IAppEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IStudyReminderService, StudyReminderService>();
builder.Services.AddScoped<ILearnerWorkspaceService, LearnerWorkspaceService>();
builder.Services.AddScoped<IProductHubService, ProductHubService>();
builder.Services.AddHostedService<StudyReminderHostedService>();
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore;
});
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequireDigit = true;

        options.Password.RequiredLength = 6;

        options.Password.RequireNonAlphanumeric = false;

        options.Password.RequireUppercase = false;

        options.Password.RequireLowercase = false;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedEmail = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

var googleId = builder.Configuration["Authentication:Google:ClientId"];
var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleId;
        options.ClientSecret = googleSecret;
    });
}

var facebookId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
{
    builder.Services.AddAuthentication().AddFacebook(options =>
    {
        options.AppId = facebookId;
        options.AppSecret = facebookSecret;
    });
}
builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath = AppRoutes.LoginPath;
        options.AccessDeniedPath = AppRoutes.AccessDeniedPath;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var root = AppContext.BaseDirectory;
    var refs = Path.Combine(root, "refs");
    Directory.CreateDirectory(refs);
    foreach (var name in new[]
    {
        "AppLearningEnglish.Models.dll",
        "AppLearningEnglish.Business.dll",
        "AppLearningEnglish.DataAccess.dll"
    })
    {
        var source = Path.Combine(root, name);
        if (File.Exists(source))
        {
            File.Copy(source, Path.Combine(refs, name), overwrite: true);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(AppRoutes.ErrorPath);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

await IdentityDataSeeder.SeedAsync(app.Services);
await ApplicationDataSeeder.SeedAsync(app.Services);

app.MapStaticAssets();
app.MapAppRoutes();

app.Run();
