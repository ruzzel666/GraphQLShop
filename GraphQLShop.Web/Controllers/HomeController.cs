using GraphQLShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using StrawberryShake;
using System.Diagnostics;
using GraphQLShop.Web.GraphQL;

namespace GraphQLShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGraphQLShopClient _graphQLClient;

    public HomeController(ILogger<HomeController> logger, IGraphQLShopClient graphQLClient)
    {
        _logger = logger;
        _graphQLClient = graphQLClient;
    }

    #region Фильтрация товаров
    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        string termToSend = searchTerm ?? "";
        var result = await _graphQLClient.GetProductsForIndex.ExecuteAsync(term: termToSend);

        if (result.IsErrorResult())
        {
            return RedirectToAction("Login", "Account");
        }
        var products = result.Data!.Products!.Items;
        return View(products);
    }
    #endregion Фильтрация товаров

    #region Создание товаров
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var input = new AddProductInput
        {
            Name = model.Name,
            Price = model.Price,
            CategoryName = model.CategoryName
        };

        var result = await _graphQLClient.AddProduct.ExecuteAsync(input);

        if (result.IsErrorResult())
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Message);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
    #endregion Создание товаров

    #region Удаление товаров
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _graphQLClient.DeleteProduct.ExecuteAsync(id);

        if (result.IsErrorResult())
        {
            return View("Error");
        }

        return RedirectToAction(nameof(Index));
    }
    #endregion Удаление товаров

    #region Редактирование товаров
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _graphQLClient.GetProductById.ExecuteAsync(id);

        if (result.IsErrorResult() || result.Data?.Product == null)
        {
            return NotFound();
        }

        var productData = result.Data.Product;

        var model = new EditProductViewModel
        {
            Id = productData.Id,
            Name = productData.Name,
            Price = productData.Price,
            CategoryName = productData.Category?.Name ?? ""
        };

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

        var input = new UpdateProductInput
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price,
            CategoryName = model.CategoryName
        };

        var result = await _graphQLClient.UpdateProduct.ExecuteAsync(input);

        if (result.IsErrorResult())
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Message);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
    #endregion Редактирование товаров

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