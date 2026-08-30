using BibliaTeologica.Application.Auth.Dtos;

namespace BibliaTeologica.Api.Auth;

public class CookieWriter
{
    public const string Access = "bt_access";
    public const string Refresh = "bt_refresh";
    public const string Csrf = "bt_csrf";

    public void Escrever(HttpResponse res, TokensGerados t)
    {
        res.Cookies.Append(Access, t.AccessToken, Base(t.AccessExpiraEm, httpOnly: true, path: "/"));
        res.Cookies.Append(Refresh, t.RefreshToken, Base(t.RefreshExpiraEm, httpOnly: true, path: "/api/auth"));
        res.Cookies.Append(Csrf, t.Csrf, Base(t.RefreshExpiraEm, httpOnly: false, path: "/"));
    }

    public void Limpar(HttpResponse res)
    {
        foreach (var (nome, path) in new[] { (Access, "/"), (Refresh, "/api/auth"), (Csrf, "/") })
            res.Cookies.Append(nome, "", new CookieOptions
            {
                Expires = DateTimeOffset.UnixEpoch,
                HttpOnly = nome != Csrf,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = path
            });
    }

    private static CookieOptions Base(DateTime expira, bool httpOnly, string path) => new()
    {
        Expires = new DateTimeOffset(expira, TimeSpan.Zero),
        HttpOnly = httpOnly,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = path
    };
}
