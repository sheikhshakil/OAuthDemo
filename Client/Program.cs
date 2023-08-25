using IdentityModel.Client;
using System.Text.Json;

var client = new HttpClient();
var doc = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (doc.IsError)
{
    Console.WriteLine(doc.Error); 
    return;
}

Console.WriteLine("Requesting token...\n");

var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = doc.TokenEndpoint,
    ClientId = "client",
    ClientSecret = "secret",
    Scope = "default"
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}
else
{
    Console.WriteLine("\nSuccessfully got token response:\n" + JsonSerializer.Serialize(tokenResponse.Json, new JsonSerializerOptions { WriteIndented = true}));

    //call the api
    Console.WriteLine("\nCalling for protected API response using token...");
    var apiClient = new HttpClient();
    apiClient.SetBearerToken(tokenResponse.AccessToken ?? "");

    var response = await apiClient.GetAsync("https://localhost:7240/api/identity");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Error: " + response.StatusCode);
    }
    else
    {
        var res = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Console.WriteLine("\nResponse:\n" + JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true}));
    }
}