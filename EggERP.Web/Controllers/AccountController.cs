using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EggERP.Web.Controllers;

public class AccountController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("/Account/Login")]
    public ContentResult LoginPage(string? error = null)
    {
        var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<p style='color:red'>{error}</p>";
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
}