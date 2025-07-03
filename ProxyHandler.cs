
using System.Net;
using System.Net.Http;

public class ProxyHandler
{
    public static async Task<HttpResponseMessage> ForwardAsync(HttpRequest incomingRequest, string targetUrl, bool useProxy, IConfiguration config)
    {
        var handler = new HttpClientHandler();

        if (useProxy)
        {
            var proxyUrl = config["Proxy:Url"];
            var proxyUser = config["Proxy:Username"];
            var proxyPass = config["Proxy:Password"];

            handler.Proxy = new WebProxy(proxyUrl)
            {
                Credentials = new NetworkCredential(proxyUser, proxyPass),
                BypassProxyOnLocal = false
            };
        }

        using var client = new HttpClient(handler);
        var requestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(incomingRequest.Method),
            RequestUri = new Uri(targetUrl)
        };

        foreach (var header in incomingRequest.Headers)
        {
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                requestMessage.Content ??= new StreamContent(incomingRequest.Body);
                requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        if (incomingRequest.ContentLength > 0 && incomingRequest.Body != null &&
            incomingRequest.Method != HttpMethods.Get && incomingRequest.Method != HttpMethods.Head)
        {
            using var reader = new StreamReader(incomingRequest.Body);
            var body = await reader.ReadToEndAsync();
            requestMessage.Content = new StringContent(body, System.Text.Encoding.UTF8, incomingRequest.ContentType ?? "application/json");
        }

        return await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
    }
}
