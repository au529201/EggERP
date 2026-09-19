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

    // Shared page shell: logo, farm-photo background, cream card.
    // authBody is the form/content that goes inside the card.
    private static string RenderAuthPage(string title, string authBody)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>EggERP - {title}</title>
    <link rel='stylesheet' href='/_content/EggERP.Shared/bootstrap/bootstrap.min.css' />
    <link rel='stylesheet' href='/_content/EggERP.Shared/app.css' />
<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css' />
    <style>
        html, body {{
            height: 100%;
        }}

.auth-page {{
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px;
    background-color: var(--egg-bg);
    background-image:
        linear-gradient(rgba(74, 46, 31, 0.55), rgba(74, 46, 31, 0.55)),
        url('/_content/EggERP.Shared/images/farm_flock.jpg');
    background-size: cover;
    background-position: center;
}}

        .auth-card {{
            width: 100%;
            max-width: 420px;
            background-color: var(--egg-card);
            border: 1px solid #E5D9BE;
            border-radius: 14px;
            box-shadow: 0 12px 32px rgba(0, 0, 0, 0.25);
            padding: 40px 36px;
        }}
.password-field-wrapper {{
    position: relative;
}}

.password-field-wrapper input {{
    padding-right: 44px;
}}

.password-toggle {{
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: var(--egg-clay);
    cursor: pointer;
    padding: 4px;
    display: flex;
    align-items: center;
}}

.password-toggle:hover {{
    color: var(--egg-brown);
}}

        .auth-logo {{
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            margin-bottom: 24px;
        }}

        .auth-logo img {{
            height: 44px;
            width: 44px;
            object-fit: contain;
        }}

        .auth-logo span {{
            font-family: 'Fraunces', Georgia, serif;
            font-size: 1.5rem;
            font-weight: 600;
            color: var(--egg-brown);
        }}

        .auth-card h3 {{
            text-align: center;
            margin-bottom: 6px;
        }}

        .auth-subtitle {{
            text-align: center;
            color: var(--egg-clay);
            font-size: 0.9rem;
            margin-bottom: 20px;
        }}

        .auth-card label {{
            font-weight: 500;
            color: var(--egg-brown);
        }}

        .auth-card .form-control {{
            border-color: #E5D9BE;
            padding: 10px 12px;
        }}

        .auth-card .form-control:focus {{
            border-color: var(--egg-gold);
            box-shadow: 0 0 0 0.2rem rgba(200, 134, 43, 0.25);
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
            background-color: #FBEAEA;
            color: #A33A3A;
            border: 1px solid #E9C6C6;
        }}

        .auth-alert-success {{
            background-color: #E9F3EA;
            color: var(--egg-sage);
            border: 1px solid #C9E0CB;
        }}

        .auth-links {{
            text-align: center;
            margin-top: 18px;
            font-size: 0.9rem;
        }}
    </style>
</head>
<body>
    <div class='auth-page'>
        <div class='auth-card'>
            <div class='auth-logo'>
<img src='/_content/EggERP.Shared/images/logoEEE.png' alt='EggERP' />
<span>EggERP</span>
            </div>
            {authBody}
        </div>
    </div>
<script>
    function togglePassword(inputId, btn) {{
        var input = document.getElementById(inputId);
        var icon = btn.querySelector('i');
        if (input.type === 'password') {{
            input.type = 'text';
            icon.classList.remove('bi-eye');
            icon.classList.add('bi-eye-slash');
        }} else {{
            input.type = 'password';
            icon.classList.remove('bi-eye-slash');
            icon.classList.add('bi-eye');
        }}
    }}
</script>
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
    <p class='auth-subtitle'>Sign in to your EggERP account</p>
    {errorHtml}
    {messageHtml}
    <form method='post' action='/Account/Login'>
        <div class='mb-3'>
            <label>Email</label>
            <input type='email' name='email' class='form-control' required />
        </div>
        <div class='mb-3'>
            <label>Password</label>
            <div class='password-field-wrapper'>
                <input type='password' name='password' id='loginPassword' class='form-control' required />
                <button type='button' class='password-toggle' onclick='togglePassword(""loginPassword"", this)'>
                    <i class='bi bi-eye'></i>
                </button>
            </div>
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
            <p class='auth-subtitle'>Choose a password to activate your EggERP account</p>
            {errorHtml}
            <form method='post' action='/Account/SetPassword'>
                <input type='hidden' name='email' value='{email}' />
                <input type='hidden' name='token' value='{token}' />
                <div class='mb-3'>
                    <label>New Password</label>
                    <input type='password' name='password' class='form-control' required />
                    <small class='form-text text-muted'>At least 8 characters, including an uppercase letter, a lowercase letter, and a number.</small>
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

            var subject = "Reset your EggERP password";
            var html = $@"
                <p>Hi {user.FullName},</p>
                <p>We received a request to reset your EggERP password. Click the link below to choose a new one:</p>
                <p><a href='{resetUrl}'>Reset your password</a></p>
                <p>If you didn't request this, you can ignore this email.</p>";

            await _emailSender.SendEmailAsync(user.Email!, subject, html);
        }

        return Redirect($"/Account/ForgotPassword?message={Uri.EscapeDataString(genericMessage)}");
    }
}