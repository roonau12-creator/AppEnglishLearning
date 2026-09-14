using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using AppLearningEnglish.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;

namespace AppLearningEnglish.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly IAppEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment environment,
            IMemoryCache cache,
            IAppEmailSender emailSender,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _environment = environment;
            _cache = cache;
            _emailSender = emailSender;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            BindExternalFlags();
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                BindExternalFlags();
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return await RedirectByRoleAsync(model.Email, returnUrl);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản chưa được phép đăng nhập.");
                BindExternalFlags();
                return View(model);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản đã bị khóa tạm thời vì đăng nhập sai nhiều lần.");
                BindExternalFlags();
                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "Email hoặc mật khẩu không đúng.");

            BindExternalFlags();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            BindExternalFlags();
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                BindExternalFlags();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                EnglishLevel = string.IsNullOrWhiteSpace(model.EnglishLevel)
                    ? null
                    : model.EnglishLevel.Trim(),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await EnsureRoleExistsAsync(AppRoles.User);
                await _userManager.AddToRoleAsync(user, AppRoles.User);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return await RedirectByRoleAsync(user.Email!, returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            BindExternalFlags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ViewBag.EmailConfirmed = user.EmailConfirmed;

            return View(new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email,
                EnglishLevel = user.EnglishLevel,
                AvatarUrl = user.AvatarUrl,
                DailyGoalMinutes = user.DailyGoalMinutes,
                VocabDailyTarget = user.VocabDailyTarget,
                ReviewIntervalPercent = user.ReviewIntervalPercent,
                ReminderEnabled = user.ReminderEnabled,
                ReminderHour = user.ReminderHour,
                TimeZoneId = user.TimeZoneId,
                PreferredTheme = user.PreferredTheme,
                LearningGoalKind = user.LearningGoalKind,
                VocabGoalTotal = user.VocabGoalTotal > 0 ? user.VocabGoalTotal : 2000,
                IeltsBandTarget = user.IeltsBandTarget,
                UiLanguage = user.UiLanguage == "en" ? "en" : "vi"
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.AvatarUrl = user.AvatarUrl;
                ViewBag.EmailConfirmed = user.EmailConfirmed;
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.EnglishLevel = string.IsNullOrWhiteSpace(model.EnglishLevel)
                ? null
                : model.EnglishLevel.Trim();
            user.DailyGoalMinutes = model.DailyGoalMinutes;
            user.VocabDailyTarget = model.VocabDailyTarget;
            user.ReviewIntervalPercent = model.ReviewIntervalPercent;
            user.ReminderEnabled = model.ReminderEnabled;
            user.ReminderHour = model.ReminderHour;
            user.TimeZoneId = string.IsNullOrWhiteSpace(model.TimeZoneId)
                ? "Asia/Ho_Chi_Minh"
                : model.TimeZoneId.Trim();
            user.PreferredTheme = model.PreferredTheme == "dark" ? "dark" : "light";
            user.UiLanguage = model.UiLanguage == "en" ? "en" : "vi";
            user.LearningGoalKind = model.LearningGoalKind switch
            {
                "ielts" => "ielts",
                "general" => "general",
                "communication" => "communication",
                "workplace" => "workplace",
                "exam" => "exam",
                _ => "vocab"
            };
            user.VocabGoalTotal = model.VocabGoalTotal > 0 ? model.VocabGoalTotal : 2000;
            user.IeltsBandTarget = model.LearningGoalKind == "ielts"
                ? model.IeltsBandTarget
                : null;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var folder = Path.Combine(_environment.WebRootPath, "avatars");
                Directory.CreateDirectory(folder);
                var fileName = $"{user.Id}{Path.GetExtension(model.AvatarFile.FileName)}";
                var path = Path.Combine(folder, fileName);
                await using (var stream = System.IO.File.Create(path))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = "/avatars/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            Response.Cookies.Append("app-theme", user.PreferredTheme, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            });
            Response.Cookies.Append(AppLearningEnglish.AppUi.CookieName, user.UiLanguage, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            });
            TempData["success"] = "Đã cập nhật hồ sơ.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Đã đổi mật khẩu.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { area = "Identity", email = user.Email, token },
                    Request.Scheme);

                _cache.Set("reset:" + user.Email, resetUrl, TimeSpan.FromHours(1));

                var sent = await _emailSender.SendAsync(
                    user.Email!,
                    "Đặt lại mật khẩu AppLearningEnglish",
                    $"<p>Xin chào {user.FullName ?? user.Email},</p>" +
                    $"<p>Nhấn vào liên kết sau để đặt lại mật khẩu (hết hạn sau 1 giờ):</p>" +
                    $"<p><a href=\"{resetUrl}\">Đặt lại mật khẩu</a></p>");

                ViewBag.EmailSent = sent;

                // Khi chưa cấu hình SMTP, hiện link ngay để môi trường demo vẫn dùng được.
                if (!sent)
                {
                    ViewBag.ResetUrl = resetUrl;
                }
            }

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["success"] = "Đặt lại mật khẩu thành công. Hãy đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            TempData[result.Succeeded ? "success" : "error"] = result.Succeeded
                ? "Email đã được xác nhận. Hãy đăng nhập để bắt đầu học."
                : "Liên kết xác nhận không hợp lệ hoặc đã hết hạn.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmation()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || user.EmailConfirmed)
            {
                ViewBag.Sent = true;
                return View(model);
            }

            var fallbackUrl = await SendEmailConfirmationAsync(user);
            ViewBag.Sent = true;
            ViewBag.ConfirmUrl = fallbackUrl;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.EmailConfirmed)
            {
                TempData["success"] = "Email của bạn đã được xác nhận.";
                return RedirectToAction(nameof(Profile));
            }

            var confirmUrl = await SendEmailConfirmationAsync(user);

            TempData["success"] = confirmUrl == null
                ? "Đã gửi email xác nhận."
                : $"SMTP chưa cấu hình. Dùng liên kết này để xác nhận: {confirmUrl}";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult DeleteAccount()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string password, string confirm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!string.Equals(confirm, "XOA", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Hãy gõ XOA để xác nhận xóa tài khoản.");
                return View();
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
                return View();
            }

            await _signInManager.SignOutAsync();
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View();
            }

            TempData["success"] = "Tài khoản đã được xóa.";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var vocab = await _context.UserVocabularies
                .AsNoTracking()
                .Include(x => x.Word)
                .Where(x => x.ApplicationUserId == user.Id)
                .Select(x => new
                {
                    x.Word!.WordText,
                    x.Status,
                    x.CorrectCount,
                    x.WrongCount,
                    x.IsFavorite,
                    x.IsHard,
                    x.NextReviewAt
                })
                .ToListAsync();

            var payload = new
            {
                exportedAt = DateTime.UtcNow,
                profile = new
                {
                    user.Email,
                    user.FullName,
                    user.EnglishLevel,
                    user.LearningGoalKind,
                    user.UiLanguage,
                    user.DailyGoalMinutes
                },
                vocabulary = vocab,
                streaks = await _context.DailyStreaks
                    .AsNoTracking()
                    .Where(x => x.ApplicationUserId == user.Id)
                    .OrderByDescending(x => x.StudyDate)
                    .Take(90)
                    .Select(x => new { x.StudyDate, x.MinutesStudied, x.PointsEarned })
                    .ToListAsync()
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "applearning-data.json");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { area = "Identity", returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                TempData["error"] = "Đăng nhập mạng xã hội thất bại.";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var signIn = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (signIn.Succeeded)
            {
                var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                return existing?.Email == null
                    ? RedirectToLocal(returnUrl)
                    : await RedirectByRoleAsync(existing.Email, returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["error"] = "Tài khoản mạng xã hội không có email.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                var created = await _userManager.CreateAsync(user);
                if (!created.Succeeded)
                {
                    TempData["error"] = created.Errors.FirstOrDefault()?.Description ?? "Không tạo được tài khoản.";
                    return RedirectToAction(nameof(Login));
                }

                await EnsureRoleExistsAsync(AppRoles.User);
                await _userManager.AddToRoleAsync(user, AppRoles.User);
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return await RedirectByRoleAsync(user.Email!, returnUrl);
        }

        private void BindExternalFlags()
        {
            ViewBag.GoogleLogin = !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]);
            ViewBag.FacebookLogin = !string.IsNullOrWhiteSpace(_configuration["Authentication:Facebook:AppId"]);
        }

        /// <summary>
        /// Gửi email xác nhận. Trả về liên kết khi không gửi được
        /// để môi trường demo vẫn xác nhận được.
        /// </summary>
        private async Task<string?> SendEmailConfirmationAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { area = "Identity", userId = user.Id, token },
                Request.Scheme);

            var sent = await _emailSender.SendAsync(
                user.Email!,
                "Xác nhận email AppLearningEnglish",
                $"<p>Xin chào {user.FullName ?? user.Email},</p>" +
                $"<p>Nhấn vào liên kết sau để xác nhận email của bạn:</p>" +
                $"<p><a href=\"{confirmUrl}\">Xác nhận email</a></p>");

            return sent ? null : confirmUrl;
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private async Task<IActionResult> RedirectByRoleAsync(
            string email,
            string? returnUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null &&
                await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
