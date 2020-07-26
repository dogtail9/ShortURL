// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;

namespace ShortUrl.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids()
        {
            var shorturlProfile = new IdentityResource
            (
                name: "shorturl",
                displayName: "ShortUrl Profile",
                claimTypes: new[] { "shorturl.accesslevel" }
            );

            var groupsProfile = new IdentityResource
            (
                name: "groups",
                displayName: "groups Profile",
                claimTypes: new[] { "groups" }
            );

            return new IdentityResource[]
            {
              new IdentityResources.OpenId(),
              new IdentityResources.Profile(),
              new IdentityResources.Email(),
              shorturlProfile,
              groupsProfile
            };
        }

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("managementapi", "Management API", new[] { "shorturl.accesslevel", "groups" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new Client[]
            {
                new Client
                {
                    ClientId = "managementapiclient",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "managementapi", "shorturl", "groups" }
                },

                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "managementguiclient",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    //AllowedGrantTypes = GrantTypes.Code,
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireConsent = false,
                    RequirePkce = true,
                    //AccessTokenLifetime=70,

                    // where to redirect to after login
                    RedirectUris = { $"{configuration["ManagementGuiUrl"]}/admin/signin-oidc" }, // ,{"http://shorturl/admin/signin-oidc" }

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { $"{configuration["ManagementGuiUrl"]}/admin/signout-callback-oidc" }, // { ,{ "http://shorturl/admin/signout-callback-oidc" }

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "managementapi",
                        "shorturl",
                        "groups"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,

                    AllowOfflineAccess = true
                }
            };
        }
    }
}