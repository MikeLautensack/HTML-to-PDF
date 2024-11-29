namespace HTML_to_PDF.Models;

public class Estimate
{
    public string? ContractorAddress { get; set; }
    public string? ContractorAddress2 { get; set; }
    public string? ContractorCity { get; set; }
    public string? ContractorState { get; set; }
    public string? ContractorZip { get; set; }
    public string? ContractorName { get; set; }
    public string? ContractorPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public required string EstimateName { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? Message { get; set; }
    public string? ProjectAddress { get; set; }
    public string? ProjectAddress2 { get; set; }
    public string? ProjectCity { get; set; }
    public string? ProjectState { get; set; }
    public string? ProjectZip { get; set; }
    public string? Status { get; set; }
    public decimal Subtotal { get; set; } = 10.1m;
    public decimal Tax { get; set; } = 10.1m;
    public string? TaxMode { get; set; }
    public decimal TaxRate { get; set; } = 10.1m;
    public decimal Total { get; set; } = 10.1m;
    public string? DiscountMode { get; set; }
    public decimal DiscountPercentage { get; set; } = 10.1m;
    public decimal Discount { get; set; } = 10.1m;

    public List<LineItem> LineItems { get; set; } = new List<LineItem>();
}

public class LineItem
{
    public decimal Amount { get; set; } = 10.1m;
    public string? Description { get; set; }
    public string? Item { get; set; }
    public decimal Price { get; set; } = 10.1m;
    public int? Quantity { get; set; }
    public string? RateType { get; set; }
}