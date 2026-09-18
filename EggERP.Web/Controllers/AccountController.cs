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

    [HttpGet("/Account/Login")]
    public ContentResult LoginPage(string? error = null, string? message = null)
    {
        var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<p style='color:red'>{error}</p>";
        var messageHtml = string.IsNullOrEmpty(message) ? "" : $"<p style='color:green'>{message}</p>";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>EggERP - Login</title>
    <link rel='stylesheet' href='/_content/EggERP.Shared/bootstrap/bootstrap.min.css' />
</head>
<body style='padding:40px; max-width:400px; margin:0 auto;'>
    <h3>EggERP Login</h3>
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
        <button type='submit' class='btn btn-primary'>Log in</button>
    </form>
    <p style='margin-top:16px;'><a href='/Account/ForgotPassword'>Forgot Password?</a></p>
</body>
</html>";
        return Content(html, "text/html");
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
        var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<p style='color:red'>{error}</p>";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>EggERP - Set Password</title>
    <link rel='stylesheet' href='/_content/EggERP.Shared/bootstrap/bootstrap.min.css' />
</head>
<body style='padding:40px; max-width:400px; margin:0 auto;'>
    <h3>Set Your Password</h3>
    <p>Choose a password to activate your EggERP account.</p>
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
    </form>
</body>
</html>";
        return Content(html, "text/html");
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
        var messageHtml = string.IsNullOrEmpty(message) ? "" : $"<p style='color:green'>{message}</p>";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>EggERP - Forgot Password</title>
    <link rel='stylesheet' href='/_content/EggERP.Shared/bootstrap/bootstrap.min.css' />
</head>
<body style='padding:40px; max-width:400px; margin:0 auto;'>
    <h3>Forgot Password</h3>
    <p>Enter your email and we'll send you a link to reset your password.</p>
    {messageHtml}
    <form method='post' action='/Account/ForgotPassword'>
        <div class='mb-3'>
            <label>Email</label>
            <input type='email' name='email' class='form-control' required />
        </div>
        <button type='submit' class='btn btn-primary'>Send Reset Link</button>
    </form>
    <p style='margin-top:16px;'><a href='/Account/Login'>Back to Login</a></p>
</body>
</html>";
        return Content(html, "text/html");
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