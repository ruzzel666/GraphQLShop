using System.Net.Http.Headers;

namespace GraphQLShop.Web.Handlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Пытаемся достать куку с токеном из текущего запроса браузера
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Cookies.TryGetValue("X-Access-Token", out var token))
        {
            // 2. Если кука есть, добавляем токен в заголовок Authorization
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 3. Отправляем запрос дальше по цепочке (на сервер)
        return await base.SendAsync(request, cancellationToken);
    }
}