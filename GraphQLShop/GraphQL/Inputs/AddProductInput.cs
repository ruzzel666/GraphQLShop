namespace GraphQLShop.GraphQL.Inputs;

// Это наш контейнер для данных.
// Мы просто перечисляем, что хотим получить от клиента.
public record AddProductInput(string Name, double Price, string CategoryName);