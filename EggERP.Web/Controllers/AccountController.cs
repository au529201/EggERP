using EggERP.Application.Email;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace EggERP.Web.Controllers;

public class AccountController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    // Shared page shell: full-bleed background photo, marketing panel on the
    // left (sharp, readable over the photo), frosted-glass card on the right
    // (translucent + backdrop-blur, so the photo still reads through it).
    // authBody is the form/content that goes inside the card.
    private static string RenderAuthPage(string title, string authBody)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>EzEggERP - {title}</title>
    <link rel='stylesheet' href='/_content/EggERP.Shared/bootstrap/bootstrap.min.css' />
    <link rel='stylesheet' href='/_content/EggERP.Shared/app.css' />
    <style>
        html, body {{
            height: 100%;
        }}

        .auth-page {{
            position: relative;
            min-height: 100vh;
            display: flex;
            background-image:
                linear-gradient(to right, rgba(74,46,31,0.15) 0%, rgba(74,46,31,0.55) 60%, rgba(74,46,31,0.85) 100%),
                url('/images/login-bg.jpg');
            background-size: cover;
            background-position: center;
        }}

        .auth-left {{
            flex: 1 1 55%;
            display: flex;
            flex-direction: column;
            justify-content: center;
            padding: 60px 64px;
            color: #FFFFFF;
            text-shadow: 0 2px 10px rgba(0,0,0,0.45);
        }}

        .auth-left .auth-logo {{
            display: flex;
            align-items: center;
            gap: 14px;
            margin-bottom: 28px;
        }}

        .auth-left .auth-logo img {{
            height: 56px;
            width: 56px;
            object-fit: contain;
        }}

        .auth-left .auth-logo span {{
            font-family: 'Fraunces', Georgia, serif;
            font-size: 2.1rem;
            font-weight: 700;
            color: #FFFFFF;
        }}

        .auth-tagline {{
            font-size: 1.05rem;
            max-width: 480px;
            opacity: 0.95;
            margin-bottom: 0;
        }}

        .auth-right {{
            flex: 1 1 45%;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 40px;
        }}

        .auth-card {{
            width: 100%;
            max-width: 400px;
            background-color: rgba(74, 46, 31, 0.72);
            backdrop-filter: blur(14px);
            -webkit-backdrop-filter: blur(14px);
            border: 1px solid rgba(255, 255, 255, 0.18);
            border-radius: 16px;
            box-shadow: 0 20px 44px rgba(0, 0, 0, 0.35);
            padding: 40px 36px;
            color: #FFFFFF;
        }}

        .auth-card h3 {{
            text-align: center;
            margin-bottom: 6px;
            color: #FFFFFF;
        }}

        .auth-subtitle {{
            text-align: center;
            color: rgba(255, 255, 255, 0.75);
            font-size: 0.9rem;
            margin-bottom: 24px;
        }}

        .auth-card label {{
            font-weight: 500;
            color: rgba(255, 255, 255, 0.9);
        }}

        .auth-card .form-control {{
            background-color: rgba(255, 255, 255, 0.92);
            border: 1px solid rgba(255, 255, 255, 0.4);
            padding: 10px 12px;
        }}

        .auth-card .form-control:focus {{
            border-color: var(--egg-gold);
            box-shadow: 0 0 0 0.2rem rgba(200, 134, 43, 0.35);
        }}

        .auth-card .btn-primary {{
            width: 100%;
            padding: 10px;
            font-weight: 600;
        }}

        .auth-alert {{
            border-radius: 8px;
            padding: 10px 14px;
            font-size: 0.9rem;
            margin-bottom: 16px;
        }}

        .auth-alert-error {{
            background-color: rgba(251, 234, 234, 0.95);
            color: #A33A3A;
            border: 1px solid #E9C6C6;
        }}

        .auth-alert-success {{
            background-color: rgba(233, 243, 234, 0.95);
            color: var(--egg-sage);
            border: 1px solid #C9E0CB;
        }}

        .auth-links {{
            text-align: center;
            margin-top: 18px;
            font-size: 0.9rem;
        }}

        .auth-links a {{
            color: #F3D9A4;
        }}

        @media (max-width: 900px) {{
            .auth-page {{
                flex-direction: column;
            }}

            .auth-left {{
                flex: 0 0 auto;
                padding: 40px 32px 20px;
                text-align: center;
            }}

            .auth-left .auth-logo {{
                justify-content: center;
            }}

            .auth-right {{
                flex: 1 1 auto;
                padding: 20px 32px 48px;
            }}
        }}
    </style>
</head>
<body>
    <div class='auth-page'>
        <div class='auth-left'>
            <div class='auth-logo'>
                <img src='/images/logo.png' alt='EzEggERP' />
                <span>EzEggERP</span>
            </div>
            <p class='auth-tagline'>Streamline your poultry farm operations. From hatchery to harvest.</p>
        </div>
        <div class='auth-right'>
            <div class='auth-card'>
                {authBody}
            </div>Flock
        </div>
    </div>
</body>
</html>";
    }

    [HttpGet("/Account/Login")]
    public ContentResult LoginPage(string? error = null, string? message = null)
    {
        var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<div class='auth-alert auth-alert-error'>{error}</div>";
        var messageHtml = string.IsNullOrEmpty(message) ? "" : $"<div class='auth-alert auth-alert-success'>{message}</div>";
        var body = $@"
            <h3>Welcome Back</h3>
            <p class='auth-subtitle'>Sign in to your EzEggERP account</p>
            {errorHtml}
            {messageHtml}
            <form method='post' action='/Account/Login'>
                <div class='mb-3'>
                    <label>Email</label>
                    <input type='email' name='email' class='form-control' required />
                </div>
                <div class='mb-3'>
                    <label>Password</label>
                    <input type='password' name='password' class='form-control' required />
                </div>
                <button type='submit' class='btn btn-primary'>Log In</button>
            </form>
            <div class='auth-links'>
                <a href='/Account/ForgotPassword'>Forgot Password?</a>
            </div>";
        return Content(RenderAuthPage("Login", body), "text/html");
    }

    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return Redirect("/Account/Login?error=Invalid login attempt.");
        }

        if (!user.EmailConfirmed)
        {
            return Redirect("/Account/Login?error=Please set your password using the link in your invitation email before logging in.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return Redirect("/Account/Login?error=This account has been deactivated.");
        }
        if (!result.Succeeded)
        {
            return Redirect("/Account/Login?error=Invalid login attempt.");
        }

        return Redirect("/");
    }

    [HttpPost("/Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/Account/Login");
    }

    [HttpGet("/Account/SetPassword")]
    public ContentResult SetPasswordPage(string email, string token, string? error = null)
    {
        var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<div class='auth-alert auth-alert-error'>{error}</div>";
        var body = $@"
            <h3>Set Your Password</h3>
            <p class='auth-subtitle'>Choose a password to activate your EzEggERP account</p>
            {errorHtml}
            <form method='post' action='/Account/SetPassword'>
                <input type='hidden' name='email' value='{email}' />
                <input type='hidden' name='token' value='{token}' />
                <div class='mb-3'>
                    <label>New Password</label>
                    <input type='password' name='password' class='form-control' required />
                    <small class='form-text' style='color: rgba(255,255,255,0.7);'>At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</small>
                </div>
                <div class='mb-3'>
                    <label>Confirm Password</label>
                    <input type='password' name='confirmPassword' class='form-control' required />
                </div>
                <button type='submit' class='btn btn-primary'>Set Password</button>
            </form>";
        return Content(RenderAuthPage("Set Password", body), "text/html");
    }

    [HttpPost("/Account/SetPassword")]
    public async Task<IActionResult> SetPassword(string email, string token, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            return Redirect($"/Account/SetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&error=Passwords do not match.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Redirect("/Account/Login?error=Invalid or expired link.");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            return Redirect("/Account/Login?error=Invalid or expired link.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return Redirect($"/Account/SetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&error={Uri.EscapeDataString(errors)}");
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Redirect("/Account/Login?message=Your password has been set. You can now log in.");
    }

    [HttpGet("/Account/ForgotPassword")]
    public ContentResult ForgotPasswordPage(string? message = null)
    {
        var messageHtml = string.IsNullOrEmpty(message) ? "" : $"<div class='auth-alert auth-alert-success'>{message}</div>";
        var body = $@"
            <h3>Forgot Password</h3>
            <p class='auth-subtitle'>Enter your email and we'll send you a link to reset your password</p>
            {messageHtml}
            <form method='post' action='/Account/ForgotPassword'>
                <div class='mb-3'>
                    <label>Email</label>
                    <input type='email' name='email' class='form-control' required />
                </div>
                <button type='submit' class='btn btn-primary'>Send Reset Link</button>
            </form>
            <div class='auth-links'>
                <a href='/Account/Login'>Back to Login</a>
            </div>";
        return Content(RenderAuthPage("Forgot Password", body), "text/html");
    }

    [HttpPost("/Account/ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // Always show the same message whether or not the account exists,
        // so this endpoint can't be used to discover which emails are registered.
        const string genericMessage = "If an account with that email exists, a reset link has been sent.";

        if (user is not null && user.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var encodedEmail = Uri.EscapeDataString(user.Email!);

            var baseUrl = (_configuration["ApiBaseUrl"] ?? "https://localhost:7062/").TrimEnd('/');
            var resetUrl = $"{baseUrl}/Account/SetPassword?email={encodedEmail}&token={encodedToken}";

            var subject = "Reset your EzEggERP password";
            var html = $@"
                <p>Hi {user.FullName},</p>
                <p>We received a request to reset your EzEggERP password. Click the link below to choose a new one:</p>
                <p><a href='{resetUrl}'>Reset your password</a></p>
                <p>If you didn't request this, you can ignore this email.</p>";

            await _emailSender.SendEmailAsync(user.Email!, subject, html);
        }

        return Redirect($"/Account/ForgotPassword?message={Uri.EscapeDataString(genericMessage)}");
    }
}