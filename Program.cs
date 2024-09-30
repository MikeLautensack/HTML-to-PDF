using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HTML to PDF API", Version = "v1", Description = "An API that converts HTML content to PDF using PuppeteerSharp" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HTML to PDF API v1");
    });
}

app.MapGet("/", (HttpRequest request, HttpResponse response) =>
{
    return "Welcome to HTML to PDF converter!";
}).WithName("Welcome");

app.MapPost("/convert-to-pdf", async (ConvertToPdfRequest request, HttpContext context) =>
{
    string htmlContent = request.htmlContent;
    string fileName = string.IsNullOrWhiteSpace(request.fileName) ? "document.pdf" : request.fileName;

    try
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent);
        var pdfBytes = await page.PdfAsync(new PagePdfOptions { Format = "A4" });
        await browser.CloseAsync();

        context.Response.ContentType = "application/pdf";
        context.Response.Headers.Append("Content-Disposition", $"attachment; filename={fileName}");
        await context.Response.Body.WriteAsync(pdfBytes);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
    }
})
.WithName("ConvertToPdf")
.Accepts<ConvertToPdfRequest>("application/json")
.Produces<FileContentResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithOpenApi(operation => new OpenApiOperation(operation)
{
    RequestBody = new OpenApiRequestBody
    {
        Content = {
            ["application/json"] = new OpenApiMediaType {
                Schema = new OpenApiSchema {
                    Type = "object",
                    Properties = {
                        ["htmlContent"] = new OpenApiSchema { Type = "string", Description = "The HTML content to be converted to PDF" },
                        ["fileName"] = new OpenApiSchema { Type = "string", Description = "The desired filename for the PDF" }
                    },
                    Required = new HashSet<string> { "htmlContent", "fileName" }
                }
            }
        },
        Required = true,
        Description = "The HTML content and desired filename for the PDF"
    }
});

app.Run();