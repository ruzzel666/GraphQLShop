using FluentValidation;
using GraphQLShop.Data;
using GraphQLShop.GraphQL.Inputs;
using GraphQLShop.Models;
using GraphQLShop.Services;
using HotChocolate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraphQLShop.GraphQL;

public class Mutation
{
    public async Task<Product> AddProduct(
        [Service] AppDbContext db,
        [Service] IValidator<AddProductInput> validator, 
        AddProductInput input)
    {
        // 1. Сначала валидация!
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            throw new GraphQLException(validationResult.Errors.First().ErrorMessage);
        }

        // 2. Логика поиска категории (используем input.CategoryName)
        var category = db.Categories.FirstOrDefault(c => c.Name == input.CategoryName);
        if (category == null)
        {
            category = new Category { Name = input.CategoryName };
            db.Categories.Add(category);
        }

        // 3. Создание товара (распаковываем input)
        var newProduct = new Product
        {
            Name = input.Name,
            Price = input.Price,
            Category = category
        };

        db.Products.Add(newProduct);
        await db.SaveChangesAsync();

        return newProduct;
    }

    public async Task<Product> UpdateProduct(
        [Service] AppDbContext db,
        UpdateProductInput input)
    {
        // Ищем товар по ID. Важно: подгружаем категорию, если планируем её менять/возвращать
        var product = await db.Products
                              .Include(p => p.Category)
                              .FirstOrDefaultAsync(p => p.Id == input.Id);

        // Если товар не найден — кидаем ошибку
        if (product == null)
        {
            throw new GraphQLException("Товар с таким ID не найден!");
        }

        // Обновляем простые поля
        product.Name = input.Name;
        product.Price = input.Price;

        // Обновляем категорию (если имя изменилось)
        if (product.Category.Name != input.CategoryName)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == input.CategoryName);
            if (category == null)
            {
                category = new Category { Name = input.CategoryName };
                db.Categories.Add(category);
            }
            product.Category = category;
        }

        // Сохраняем изменения
        await db.SaveChangesAsync();

        return product;
    }

    public async Task<bool> DeleteProduct(
        [Service] AppDbContext db,
        int id)
    {
        var product = await db.Products.FindAsync(id);

        if (product == null)
        {
            throw new GraphQLException("Товар не найден, удаление невозможно.");
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return true; // Возвращаем true, если удаление прошло успешно
    }

    // НОВАЯ МУТАЦИЯ ЛОГИНА
    public async Task<AuthPayload> Login(
        [Service] AppDbContext db,
        [Service] ITokenService tokenService, // Наш новый сервис
        LoginInput input)
    {
        // 1. Ищем пользователя по имени
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == input.Username);

        if (user == null)
        {
            // Никогда не пишите "Пользователь не найден", пишите общую ошибку для безопасности
            throw new GraphQLException("Неверное имя пользователя или пароль.");
        }

        // 2. Проверяем пароль (сравниваем введенный пароль с хешем в базе)
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new GraphQLException("Неверное имя пользователя или пароль.");
        }

        // 3. Если всё ок, генерируем токен
        var token = tokenService.GenerateToken(user);

        // 4. Возвращаем токен клиенту
        return new AuthPayload(token, user.Username);
    }

    public async Task<AuthPayload> Register(
    [Service] AppDbContext db,
    [Service] ITokenService tokenService,
    RegisterInput input)
    {
        // 1. Проверяем, существует ли уже такой пользователь
        if (await db.Users.AnyAsync(u => u.Username == input.Username))
        {
            throw new GraphQLException("Пользователь с таким именем уже существует.");
        }

        // 2. Создаем нового пользователя
        var user = new User
        {
            Username = input.Username,
            Role = "User" // По умолчанию все новые - просто пользователи
        };

        // 3. Хешируем пароль
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, input.Password);

        // 4. Сохраняем в БД
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // 5. Сразу генерируем токен, чтобы пользователь был "залогинен"
        var token = tokenService.GenerateToken(user);

        return new AuthPayload(token, user.Username);
    }
}