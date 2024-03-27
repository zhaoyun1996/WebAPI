using DemoWebAPI.Constant;
using DemoWebAPI.Core.Cache;
using DemoWebAPI.Core.Model;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace DemoWebAPI.Core.Extensions
{
    public class SetAuthContextHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public SetAuthContextHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool endRequest = false;
            try
            {
                endRequest = await SetHeaders(context);
            }
            catch (Exception ex)
            {
                endRequest = false;
                byte[] data = UTF8Encoding.UTF8.GetBytes($"Internal Server Error. {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Headers.ContentLength = data.Length;
                await context.Response.Body.WriteAsync(data, 0, data.Length);
            }

            if (!endRequest)
            {
                try
                {
                    await _next(context);
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (ex != null)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Data), Encoding.UTF8);
                    }
                }
            }
        }

        private async Task<bool> SetHeaders(HttpContext context)
        {
            var httpContext = context;

            context.Response.Headers.TryAdd("Access-Control-Allow-Origin", "*");
            context.Response.Headers.TryAdd("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            context.Response.Headers.TryAdd("Access-Control-Allow-Headers", "Content-Type, Content-Encoding, Accept, Authorization, Origin");

            if(context.Request?.Method.ToUpper() == "OPTIONS" && !context.Response.HasStarted)
            {
                context.Response.Headers.TryAdd("Access-Control-Max-Age", "86400");
                byte[] data = UTF8Encoding.UTF8.GetBytes("OK");
                context.Response.StatusCode = 200;
                context.Response.Headers.ContentLength = data.Length;
                await context.Response.Body.WriteAsync(data, 0, data.Length);

                return true;
            }

            var session = new SessionData();
            string? contextString = httpContext.Request?.Headers?[Keys.HeaderContext];
            if(!string.IsNullOrWhiteSpace(contextString))
            {
                var contextData = JsonConvert.DeserializeObject<ContextData>(contextString);
                session.SessionId = contextData.SessionId;
            }

            string authHeader = httpContext.Request?.Headers?[Keys.HeaderAuthorization];
            if(!string.IsNullOrWhiteSpace(authHeader))
            {
                string token = authHeader.Split(new char[] { ' ' })[1];
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var payload = jsonToken.Payload;

                //TODO
                if(payload.ContainsKey("aid") && payload["aid"] != null)
                {
                    string aid = payload["aid"].ToString().StartsWith("Auth") ? payload["aid"].ToString().Replace("Auth", "") : payload["aid"].ToString();
                    
                    var accountId = Guid.Parse(aid);
                }
            }

            httpContext.Items["Context"] = session;

            return false;
        }
    }

    public static class SetAuthContextHandlerExtensions
    {
        public static List<string> UrlChecks = null;

        public static IApplicationBuilder UseSetAuthContextHandler(this IApplicationBuilder builder, List<string> urlChecks)
        {
            if (urlChecks != null && urlChecks.Count > 0)
            {
                UrlChecks = urlChecks;
            }

            return builder.UseMiddleware<SetAuthContextHandlerMiddleware>();
        }
    }
}
