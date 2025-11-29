using GraphQLShop.Web.GraphQL; // Наш сгенерированный клиент
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
                // Выводим ошибку на форму
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return View(model);
        }

        // 3. Получаем токен
        var token = result.Data!.Login.Token;

        // 4. СОХРАНЯЕМ ТОКЕН В КУКИ
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,   // Передавать только по HTTPS
            SameSite = SameSiteMode.Strict, // Защита от CSRF
            // Время жизни куки ставим такое же, как у токена на сервере (60 мин)
            Expires = DateTime.UtcNow.AddMinutes(60)
        };

        // "X-Access-Token" — это имя нашей куки. Запомните его.
        Response.Cookies.Append("X-Access-Token", token, cookieOptions);

        // 5. Перенаправляем на главную страницу
        return RedirectToAction("Index", "Home");
    }

    // POST: /Account/Logout
    [HttpPost]
    public IActionResult Logout()
    {
        // Для выхода просто удаляем куку
        Response.Cookies.Delete("X-Access-Token");
        return RedirectToAction("Index", "Home");
    }

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
}