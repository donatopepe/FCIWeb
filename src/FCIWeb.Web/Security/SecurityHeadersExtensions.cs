using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FCIWeb.Web.Security
{
    /// <summary>
    /// Header di sicurezza applicati a ogni risposta: mitigano clickjacking, sniffing MIME,
    /// leak di referrer e abilitano una CSP restrittiva.
    /// </summary>
    public static class SecurityHeadersExtensions
    {
        private const string ContentSecurityPolicy =
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "frame-ancestors 'none'";

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "no-referrer";
                headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
                headers["Content-Security-Policy"] = ContentSecurityPolicy;
                headers["X-Permitted-Cross-Domain-Policies"] = "none";

                // Rimuove header che rivelano tecnologia/versioni.
                headers.Remove("Server");
                headers.Remove("X-Powered-By");

                await next();
            });
        }
    }
}
