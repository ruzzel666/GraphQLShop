using Microsoft.EntityFrameworkCore;
using GraphQLShop.Data;
using GraphQLShop.GraphQL;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=shop.db"));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGraphQL();

app.Run();
