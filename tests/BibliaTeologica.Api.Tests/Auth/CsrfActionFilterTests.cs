using AwesomeAssertions;
using BibliaTeologica.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class CsrfActionFilterTests
{
    private static ActionExecutingContext Ctx(HttpContext http)
    {
        var actionCtx = new ActionContext(http, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionCtx, new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), controller: new object());
    }

    [Fact]
    public async Task Recusa403_QuandoHeaderNaoBateComCookie()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Headers["Cookie"] = "bt_csrf=abc";
        http.Request.Headers["X-CSRF-Token"] = "diferente";
        var ctx = Ctx(http);

        await new CsrfActionFilter().OnActionExecutionAsync(ctx, () => throw new Exception("não devia executar"));

        ctx.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Deixa_Passar_QuandoBate()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Headers["Cookie"] = "bt_csrf=abc";
        http.Request.Headers["X-CSRF-Token"] = "abc";
        var ctx = Ctx(http);
        var executou = false;

        await new CsrfActionFilter().OnActionExecutionAsync(ctx,
            () => { executou = true; return Task.FromResult<ActionExecutedContext>(null!); });

        executou.Should().BeTrue();
    }

    [Fact]
    public async Task Ignora_QuandoMetodoSeguro()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "GET";
        var ctx = Ctx(http);
        var executou = false;

        await new CsrfActionFilter().OnActionExecutionAsync(ctx,
            () => { executou = true; return Task.FromResult<ActionExecutedContext>(null!); });

        executou.Should().BeTrue();
    }
}
