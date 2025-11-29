using GraphQLShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using StrawberryShake;
using System.Diagnostics;
using GraphQLShop.Web.GraphQL;

namespace GraphQLShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // Объявляем поле для нашего GraphQL клиента
    private readonly IGraphQLShopClient _graphQLClient;

    // Внедряем клиент через конструктор
    public HomeController(ILogger<HomeController> logger, IGraphQLShopClient graphQLClient)
    {
        _logger = logger;
        _graphQLClient = graphQLClient;
    }


    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        // --- ИСПРАВЛЕНИЕ ---
        // Если searchTerm равен null, заменяем его на пустую строку "".
        // Фильтр { contains: "" } вернет все товары.
        string termToSend = searchTerm ?? "";
        // -------------------

        // ВЫПОЛНЯЕМ ЗАПРОС!
        // Передаем подготовленную строку termToSend
        var result = await _graphQLClient.GetProductsForIndex.ExecuteAsync(term: termToSend);

        // Вместо жесткого EnsureNoErrors(), проверяем мягко
        if (result.IsErrorResult())
        {
            // Проверяем, есть ли ошибка авторизации (код AUTH_NOT_AUTHENTICATED или сообщение)
            // Обычно GraphQL возвращает специфический код, но для простоты проверим наличие ошибок
            // и перенаправим на логин.

            // Если хотите, можете проверить текст ошибки: 
            // if (result.Errors.Any(e => e.Message.Contains("not authorized"))) ...

            return RedirectToAction("Login", "Account");
        }
        var products = result.Data!.Products!.Items;
        return View(products);
    }

    // 1. GET-метод: Просто показывает пустую форму
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 2. POST-метод: Принимает данные формы и отправляет на сервер
    [HttpPost]
    // ValidateAntiForgeryToken защищает от межсайтовых атак
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {
        // Проверка базовой валидации MVC (атрибуты в ViewModel)
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Маппинг: превращаем ViewModel из формы в Input-тип для GraphQL
        var input = new AddProductInput
        {
            Name = model.Name,
            Price = model.Price,
            CategoryName = model.CategoryName
        };

        // Выполняем мутацию
        var result = await _graphQLClient.AddProduct.ExecuteAsync(input);

        // Проверка на ошибки от GraphQL сервера (например, валидация FluentValidation на бэкенде)
        if (result.IsErrorResult())
        {
            // Добавляем все ошибки от сервера на форму
            foreach (var error in result.Errors)
            {
                // Используем пустой ключ "", чтобы ошибка была общей для формы
                ModelState.AddModelError("", error.Message);
            }
            // Возвращаем ту же форму с ошибками
            return View(model);
        }

        // Успех! Перенаправляем на главную страницу, чтобы увидеть новый товар.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Метод принимает только ID товара
    public async Task<IActionResult> Delete(int id)
    {
        // Выполняем мутацию
        var result = await _graphQLClient.DeleteProduct.ExecuteAsync(id);

        if (result.IsErrorResult())
        {
            // Если произошла ошибка (например, товар уже не существует), 
            // можно показать её на отдельной странице или через TempData.
            // Для простоты пока вернем стандартную страницу ошибки.
            return View("Error");
        }

        // После успешного удаления возвращаемся к списку
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // 1. Запрашиваем товар с сервера по ID
        var result = await _graphQLClient.GetProductById.ExecuteAsync(id);

        // 2. Проверяем: если ошибка или товар не найден (product == null)
        if (result.IsErrorResult() || result.Data?.Product == null)
        {
            return NotFound(); // Возвращаем 404
        }

        var productData = result.Data.Product;

        // 3. Превращаем данные из GraphQL в ViewModel для формы
        var model = new EditProductViewModel
        {
            Id = productData.Id,
            Name = productData.Name,
            Price = productData.Price,
            // Используем оператор ?., так как категории может не быть
            CategoryName = productData.Category?.Name ?? ""
        };

        // 4. Отдаем форму с данными
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Создаем Input для мутации (не забываем ID!)
        var input = new UpdateProductInput
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price,
            CategoryName = model.CategoryName
        };

        // 2. Выполняем мутацию обновления
        var result = await _graphQLClient.UpdateProduct.ExecuteAsync(input);

        // 3. Обработка ошибок (так же, как в Create)
        if (result.IsErrorResult())
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Message);
            }
            return View(model);
        }

        // Успех
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}