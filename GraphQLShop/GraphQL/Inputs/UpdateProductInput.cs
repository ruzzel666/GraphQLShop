namespace GraphQLShop.GraphQL.Inputs;

public record UpdateProductInput(int Id, string Name, double Price, string CategoryName);