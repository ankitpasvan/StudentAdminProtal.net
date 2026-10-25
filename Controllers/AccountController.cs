using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentAdminPortal.Data;
using StudentAdminPortal.Models;

namespace StudentAdminPortal.Controllers;


public class AccountController : Controller
{
    private readonly AdminUserRepository _admins;
    private readonly PasswordHasher<AdminUser> _passwordHasher = new();

    public AccountController(AdminUserRepository admins)
    {
        _admins = admins;
    }

    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        AdminUser? admin;
        try
        {
            admin = await _admins.GetByUsernameOrEmailAsync(model.UsernameOrEmail);
        }
        catch (SqlException)
        {
            ModelState.AddModelError(string.Empty, "Could not connect to the database. Please try again later.");
            return View(model);
        }

        // One generic message for both cases: never reveal whether the
        // username/email or the password was the wrong part.
        var passwordOk = admin is not null &&
            _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password)
                is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        if (!passwordOk)
        {
            ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin!.Id.ToString()),
            new(ClaimTypes.Name, admin.Username),
            new(ClaimTypes.Email, admin.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Dashboard");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Setup()
    {
        int count;
        try
        {
            count = await _admins.GetAdminCountAsync();
        }
        catch (SqlException)
        {

            ViewData["DatabaseError"] = true;
            return View();
        }
        if (count > 0)
        {

            return RedirectToAction("Login");
        }
        return View();
    }


    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(SetupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            if (await _admins.GetAdminCountAsync() > 0)
            {
                return RedirectToAction("Login");
            }
            if (await _admins.UsernameExistsAsync(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                return View(model);
            }
            if (await _admins.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            var admin = new AdminUser { Username = model.Username, Email = model.Email };

            admin.PasswordHash = _passwordHasher.HashPassword(admin, model.Password);
            await _admins.CreateAsync(admin);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {

            ModelState.AddModelError(string.Empty, "This username or email is already registered.");
            return View(model);
        }
        catch (SqlException)
        {
            ModelState.AddModelError(string.Empty,
                "Could not connect to the database. Check that SQL Server is running and the database exists, then try again.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Administrator account created. Please log in.";
        return RedirectToAction("Login");
    }
}
