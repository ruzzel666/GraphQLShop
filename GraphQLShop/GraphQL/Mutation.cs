using HotChocolate;
using FluentValidation;
using GraphQLShop.Data;
using GraphQLShop.Models;
using GraphQLShop.GraphQL.Inputs;
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
}