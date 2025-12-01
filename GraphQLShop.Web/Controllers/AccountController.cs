using GraphQLShop.Web.GraphQL;
using GraphQLShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using StrawberryShake;

namespace GraphQLShop.Web.Controllers;

public class AccountController : Controller
{
    private readonly IGraphQLShopClient _graphQLClient;

    public AccountController(IGraphQLShopClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }
    #region Авторизация пользователя
    [HttpGet]
    public IActionResult Login()
    {
        if (Request.Cookies.ContainsKey("X-Access-Token"))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Вызываем мутацию логина на бэкенде
        var result = await _graphQLClient.Login.ExecuteAsync(model.Username, model.Password);

        // 2. Проверяем ошибки
        if (result.IsErrorResult())
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return View(model);
        }

        // 3. Получаем токен
        var token = result.Data!.Login.Token;

        // 4. Сохраняем токен в куки
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(60)
        };

        Response.Cookies.Append("X-Access-Token", token, cookieOptions);

        // 5. Перенаправляем на главную страницу
        return RedirectToAction("Index", "Home");
    }
    
    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("X-Access-Token");
        return RedirectToAction("Index", "Home");
    }
    #endregion Авторизация пользователя

    #region Регистрация пользователя
    [HttpGet]
    public IActionResult Register()
    {
        if (Request.Cookies.ContainsKey("X-Access-Token"))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Вызываем мутацию регистрации
        var result = await _graphQLClient.Register.ExecuteAsync(model.Username, model.Password);

        if (result.IsErrorResult())
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return View(model);
        }

        // Если успех - получаем токен и сразу логиним пользователя
        var token = result.Data!.Register.Token;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(60)
        };

        Response.Cookies.Append("X-Access-Token", token, cookieOptions);

        return RedirectToAction("Index", "Home");
    }
    #endregion Регистрация пользователя
}