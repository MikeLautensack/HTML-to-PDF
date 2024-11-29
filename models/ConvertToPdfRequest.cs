// Define the model class for the request

namespace HTML_to_PDF.Models;
public class ConvertToPdfRequest
{
    public required string HtmlContent { get; set; }
    public required string FileName { get; set; }
};