using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.AspNetCore.Mvc;
using HTML_to_PDF.Models;
using Microsoft.Extensions.Hosting;
using HTML_to_PDF.models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HTML to PDF API", Version = "v1", Description = "An API that converts HTML content to PDF using PuppeteerSharp" });
});

builder.Services.AddLogging();


var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

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

app.MapPost("/generate-estimate-pdf", async (GenerateEstimatePDFRequest request, HttpContext context) =>
{
    var estimate = new Estimate()
    {
        EstimateName = request.EstimateName,
        ContractorName = request.ContractorName,
        ContractorAddress = request.ContractorAddress,
        ContractorPhone = request.ContractorPhone,
        ProjectAddress = request.ProjectAddress,
        CustomerFirstName = request.CustomerFirstName,
        CustomerLastName = request.CustomerLastName,
        Subtotal = request.Subtotal,
        Tax = request.Tax,
        Total = request.Total,
        LineItems = request.LineItems,
    };
    await using var htmlRenderer = new HtmlRenderer(app.Services, loggerFactory);
    var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
    {
        var dictionary = new Dictionary<string, object?>
        {
            { "Estimate", estimate }
        };

        var parameters = ParameterView.FromDictionary(dictionary);
        var output = await htmlRenderer.RenderComponentAsync<EstimatePDF>(parameters);
        return output.ToHtmlString();
    });

    string fileName = string.IsNullOrWhiteSpace(request.EstimateName) ? "document.pdf" : request.EstimateName;

    try
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);
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
});

app.MapPost("/convert-to-pdf", async (ConvertToPdfRequest request, HttpContext context) =>
{
    string htmlContent = request.HtmlContent;
    string fileName = string.IsNullOrWhiteSpace(request.FileName) ? "document.pdf" : request.FileName;

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