using GraphQLShop.Models;

namespace GraphQLShop.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}