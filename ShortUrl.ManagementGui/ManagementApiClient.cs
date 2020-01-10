using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace ShortUrl.ManagementGui
{
    public partial class ManagementApiClient : IManagementApiClient
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {
            //var accessToken = Microsoft.AspNetCore.Http.HttpContext.GetTokenAsync("access_token");
            //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}
