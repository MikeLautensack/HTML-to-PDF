namespace HTML_to_PDF.Models;

public class GenerateEstimatePDFRequest
{
    public required string EstimateName { get; set; }
    public required string ContractorName { get; set; }
    public required string ContractorAddress { get; set; }
    public required string ContractorPhone { get; set; }
    public required string ProjectAddress { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public List<LineItem> LineItems { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}