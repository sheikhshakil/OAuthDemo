using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebClient.Pages
{
    public class ProfileModel : PageModel
    {
        public string? access_token { get; set; }
        public IDictionary<string, string?>? auth_properties { get; set; }

        public async void OnGet()
        {
            var authResult = await HttpContext.AuthenticateAsync();
            if (authResult.Succeeded)
            {
                access_token = authResult.Properties.GetTokenValue("access_token");
                auth_properties = authResult.Properties.Items;
            }
        }
    }
}
