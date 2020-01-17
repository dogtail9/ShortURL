// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

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

            return new IdentityResource[]
            {
              new IdentityResources.OpenId(),
              new IdentityResources.Profile(),
              new IdentityResources.Email(),
              shorturlProfile
            };
        }

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("managementapi", "Management API", new[] { "shorturl.accesslevel" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
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
                    AllowedScopes = { "managementapi", "shorturl" }
                },

                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "managementguiclient",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    //AllowedGrantTypes = GrantTypes.Code,
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireConsent = true,
                    RequirePkce = true,

                    // where to redirect to after login
                    RedirectUris = { "http://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "managementapi",
                        "shorturl"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,

                    AllowOfflineAccess = true
                }
            };
    }
}