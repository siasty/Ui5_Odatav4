using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace Ui5_Backend
{
    public class HttpGetOrHeadAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "GET", "HEAD" };

        public HttpGetOrHeadAttribute() : base(_supportedMethods) { }

        public HttpGetOrHeadAttribute(string template) : base(_supportedMethods, template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }

    public static class ODataMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyOdataMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ODataMiddleware>();
        }
    } 

    public class ODataMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;

        public ODataMiddleware(RequestDelegate next, ILoggerFactory logFactory)
        {
             _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logFactory.CreateLogger("ODataMiddleware");
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation("ODataMiddlewaree executing..");
            
            bool methodSwitched = false;

            if (HttpMethods.IsHead(context.Request.Method))
            {
                methodSwitched = true;
                context.Request.Method = HttpMethods.Get;
                context.Response.Body = Stream.Null;
            }

            await _next(context);

            if (methodSwitched)
            {
                context.Request.Method = HttpMethods.Head;
            }
        }
    }
}
