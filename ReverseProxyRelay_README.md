
# ReverseProxyRelay

This is a lightweight .NET 8 reverse proxy application designed to be deployed on a staging server. It forwards HTTP requests to one of multiple middle-tier servers, using a proxy when needed.

## 🔧 Features

- Supports all HTTP methods (GET, POST, PUT, DELETE, PATCH, etc.)
- Automatically applies a proxy when routing to specific targets
- Minimal API-based, easy to deploy
- Custom headers, query strings, and body are all forwarded

---

## 🚀 Setup Instructions

1. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

   Or publish for deployment:
   ```bash
   dotnet publish -c Release -o out
   cd out
   dotnet ReverseProxyRelay.dll
   ```

2. **Configure Proxy and Targets**
   Edit `appsettings.json`:

   ```json
   {
     "Proxy": {
       "Url": "http://your-proxy-url:port",
       "Username": "proxy-user",
       "Password": "proxy-password"
     },
     "ProxyTargets": ["MiddleTierA"],
     "ProxyMap": {
       "MiddleTierA": "https://middle-tier-a.com",
       "MiddleTierB": "https://middle-tier-b.com"
     }
   }
   ```

---

## 📥 Example Usage (From Another App)

Make an HTTP request to the proxy relay, passing the `target` as a query parameter.

### `GET` Example:

```bash
curl "http://<staging-server-ip>:5000/api/status?target=MiddleTierA"
```

### `POST` Example (JSON Payload):

```bash
curl -X POST http://<staging-server-ip>:5000/api/data?target=MiddleTierB \
  -H "Content-Type: application/json" \
  -d '{"message": "hello"}'
```

### Using From Another .NET App:

```csharp
var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, "http://<staging-ip>:5000/api/data?target=MiddleTierA");
request.Content = new StringContent("{"message":"hello"}", Encoding.UTF8, "application/json");

var response = await client.SendAsync(request);
var result = await response.Content.ReadAsStringAsync();
```

> Replace `<staging-ip>` and `target` with appropriate values.

---

## 🛡️ Security Recommendation

If the staging server is exposed to public or internal networks, consider:
- Adding basic authentication
- Whitelisting IPs
- Using HTTPS with a reverse proxy (like Nginx)

---

## 📁 Folder Structure

```
ReverseProxyRelay/
├── Program.cs
├── ProxyHandler.cs
├── appsettings.json
└── ReverseProxyRelay.csproj
```
