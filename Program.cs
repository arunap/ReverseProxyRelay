
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

var app = builder.Build();

app.Map("/{**path}", async (HttpContext context) =>
{
    var config = app.Configuration;
    var targetKey = context.Request.Query["target"].ToString();

    if (string.IsNullOrEmpty(targetKey))
        return Results.BadRequest("Missing 'target' query parameter");

    var proxyMap = config.GetSection("ProxyMap").Get<Dictionary<string, string>>();
    if (!proxyMap.TryGetValue(targetKey, out var targetBaseUrl))
        return Results.BadRequest("Invalid target");

    var useProxy = config.GetSection("ProxyTargets").Get<List<string>>().Contains(targetKey);

    var forwardUrl = targetBaseUrl;

    if (!string.IsNullOrEmpty(context.Request.Path))
        forwardUrl += context.Request.Path;

    if (context.Request.QueryString.HasValue)
        forwardUrl += context.Request.QueryString.Value.Replace($"?target={targetKey}", "");

    var responseMessage = await ProxyHandler.ForwardAsync(context, forwardUrl, useProxy, config);

    context.Response.StatusCode = (int)responseMessage.StatusCode;

    foreach (var header in responseMessage.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    foreach (var header in responseMessage.Content.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    context.Response.Headers.Remove("transfer-encoding");

    await responseMessage.Content.CopyToAsync(context.Response.Body);
});

app.Run();
