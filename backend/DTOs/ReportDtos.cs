namespace FinanzasApi.DTOs;

public class ReportQuery
{
    // Only honored when the caller is an administrator.
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    // Monthly summary only. Defaults to the current year.
    public int? Year { get; set; }
}

public class ReportTotalsDto
{
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
    public int Count { get; set; }
}

public class ReportRowDto
{
    public string Label { get; set; } = "";
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
    public int Count { get; set; }
    // Category report only: percentage of total expenses.
    public decimal Percent { get; set; }
}

public class ReportDto
{
    // range | by-user | by-type | by-category | monthly
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Scope { get; set; } = "";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Year { get; set; }
    public ReportTotalsDto Totals { get; set; } = new();
    public List<ReportRowDto> Rows { get; set; } = new();
    // Date range report only: the transaction detail.
    public List<TransactionResponseDto> Transactions { get; set; } = new();
}
