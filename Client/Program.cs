using Client;
using IdentityModel.Client;
using System.Text.Json;

User user = new User();
await InitiateAuthenticationAsync();

async Task InitiateAuthenticationAsync()
{
    Console.WriteLine("------------OAuth Demo-----------");
    Console.WriteLine("Enter your username:");
    user.username = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("Enter your password:");
    user.password = Console.ReadLine() ?? string.Empty;

    await GetIdentityServerDiscoveryDocument();
}

async Task GetIdentityServerDiscoveryDocument()
{
    Console.WriteLine("Step 1: Getting identity server discovery document...\n");

    var client = new HttpClient();
    var doc = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
    if (doc.IsError)
    {
        Console.WriteLine(doc.Error);
        return;
    }
    else await RequestAccessToken(doc);
}

async Task RequestAccessToken(DiscoveryDocumentResponse doc)
{
    Console.WriteLine("Step 2: Requesting access token...\n");
    var client = new HttpClient();
    var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    {
        Address = doc.TokenEndpoint,
        ClientId = user.username,
        ClientSecret = user.password,
        Scope = "default"
    });

    if (tokenResponse.IsError)
    {
        Console.WriteLine("Error: " + tokenResponse.Error);
        return;
    }
    else
    {
        Console.WriteLine("Successfully got token response:\n" + JsonSerializer.Serialize(tokenResponse.Json, new JsonSerializerOptions { WriteIndented = true }));

        await CallProtectedEndpoint(tokenResponse);
    }
}

//await CallProtectedEndpoint("eyJhbGciOiJSUzI1NiIsImtpZCI6IkM0RTI4NzQ1ODRDRTc2RTdEMkQ2RDU5MzY1QkRFMjk1IiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo1MDAxIiwibmJmIjoxNjkzMjM1MzkyLCJpYXQiOjE2OTMyMzUzOTIsImV4cCI6MTY5MzIzDk5MiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NTAwMS9yZXNvdXJjZXMiLCJzY29wZSI6WyJkZWZhdWx0Il0sImNsaWVudF9pZCI6InNoYWtpbCIsImp0aSI6Ijk0QjlDNUIzMTk2Q0Q0MkZCQzI4NjBGRTBERTIxODJBIn0.MmSaQkVHRSzjyjJa3PFfviAK9pD8sF-yNyCog8IHbRofLClkvBm_ebn9hqKKOYzNBEFbHEg75py6bhlgfUdqmdUQtGVIH65KzKPZy4bsR8yqHDAO-2iMPT42PJxPp8Ae_4czcEO8e27Tpn3WRZU1H5u98avcNtSPfo2vYv4nlrTWuhkBi8Tp4NRGplEv75y2o85yHsbeJLEwaxegZ5F2DUchtbeVN7lKBGqaMAez6hGPIvQk-GdvKiKgkt7YtbV1YHF-3Xhh_qZMl5HrUTw1KhBTYf5TC502uVsFRSQKLQkAgV5TD-wZxUCURZMHTTnkG5KlvUq-WGG4hLwZj_99Q");

async Task CallProtectedEndpoint(TokenResponse token)
{
    Console.WriteLine("\nStep 3: Calling protected endpoint using access token...\n");
    var apiClient = new HttpClient();
    apiClient.SetBearerToken(token.AccessToken ?? "");

    var response = await apiClient.GetAsync("https://localhost:7240/api/identity");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Error: " + response.StatusCode);
    }
    else
    {
        var res = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Console.WriteLine("Final response:\n" + JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true }));
    }
}