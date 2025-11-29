namespace GraphQLShop.Models;
// То, что мы вернем клиенту при успешном входе
public record AuthPayload(string Token, string Username);