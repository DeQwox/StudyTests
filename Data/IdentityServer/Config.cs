using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace StudyTests.Data.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("app_api", "App API")
        ];

    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "mvc_client",
                ClientName = "MVC Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://localhost:5001/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email" },
                AllowOfflineAccess = true
            }
        ];

    public static List<TestUser> Users =>
    [
        new TestUser
        {
            SubjectId = "1",
            Username = "kikono",
            Password = "password",
            Claims =
            [
                new System.Security.Claims.Claim("sub", "1"),
                new System.Security.Claims.Claim("name", "Kikono Test"),
                new System.Security.Claims.Claim("email", "kikono@example.com"),
                new System.Security.Claims.Claim("phone", "+380999999999"),
            ]
        }
    ];
}
