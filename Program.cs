using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to enable Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HTML to PDF API",
        Version = "v1",
        Description = "An API that converts HTML content to PDF using PuppeteerSharp",
    });
});

var app = builder.Build();

// Use Swagger in development or production
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HTML to PDF API v1");
    });
}

app.MapGet("/test", (HttpRequest request, HttpResponse response) =>
{
    return "testing";
}).WithName("Test");

// Minimal API Endpoint to convert HTML to PDF
app.MapPost("/convert-to-pdf", async (HttpRequest request, HttpContext context) =>
{
    // Read HTML content directly from the request body
    string htmlContent;
    using (var reader = new StreamReader(context.Request.Body))
    {
        htmlContent = await reader.ReadToEndAsync();
    }

    // Use Playwright to convert HTML to PDF
    try
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent);

        var pdfBytes = await page.PdfAsync(new PagePdfOptions { Format = "A4" });
        await browser.CloseAsync();

        context.Response.ContentType = "application/pdf";
        context.Response.Headers.Append("Content-Disposition", "attachment; filename=document.pdf");
        await context.Response.Body.WriteAsync(pdfBytes);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
    }
})
.WithName("ConvertToPdf")
.Accepts<string>("text/html")
.Produces<FileContentResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    RequestBody = new OpenApiRequestBody
    {
        Content =
        {
            ["text/html"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "html"
                }
            }
        },
        Required = true,
        Description = "The HTML content to be converted to PDF"
    }
});

app.Run();
