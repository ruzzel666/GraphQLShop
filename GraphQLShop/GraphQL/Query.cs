using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types; 
using Microsoft.EntityFrameworkCore;
using GraphQLShop.Data;   
using GraphQLShop.Models;

namespace GraphQLShop.GraphQL;

public class Query
{
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] AppDbContext db)
    {
        return db.Products.Include(p => p.Category);
    }

    public Product GetProduct([Service] AppDbContext db, string name)
    {
        return db.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Name == name);
    }
}