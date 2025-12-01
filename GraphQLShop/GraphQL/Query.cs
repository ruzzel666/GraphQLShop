using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types; 
using Microsoft.EntityFrameworkCore;
using GraphQLShop.Data;   
using GraphQLShop.Models;
using HotChocolate.Authorization;

namespace GraphQLShop.GraphQL;

public class Query
{
    [Authorize]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] AppDbContext db)
    {
        return db.Products.Include(p => p.Category);
    }

    public List<Product> GetAllProductsRaw([Service] AppDbContext db)
    {
        return db.Products
            .Include(p => p.Category) 
            .ToList();
    }

    public Product GetProduct([Service] AppDbContext db, int id)
    {
        return db.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id); // Ищем по Id
    }
}