using System.ComponentModel.DataAnnotations;

namespace FinanzasApi.DTOs;

public class CreateTransactionDto
{
    [Required, RegularExpression("Income|Expense")] public string Type { get; set; } = "Expense";

    // Only honored when the caller is an administrator. Otherwise the token's user is used.
    public string? UserId { get; set; }

    [Required] public string ItemId { get; set; } = "";
    [Required] public string PaymentTypeId { get; set; } = "";
    [Required] public DateTime TransactionDate { get; set; }
    [Range(0.01, 1_000_000_000_000)] public decimal Amount { get; set; }
    public string? CardNumber { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
}

public class UpdateTransactionDto
{
    [Required] public string ItemId { get; set; } = "";
    [Required] public string PaymentTypeId { get; set; } = "";
    [Required] public DateTime TransactionDate { get; set; }
    [Range(0.01, 1_000_000_000_000)] public decimal Amount { get; set; }
    public string? CardNumber { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
}

public class TransactionQuery
{
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Type { get; set; }
    public string? ExpenseTypeId { get; set; }
    public string? CategoryId { get; set; }
    public string? PaymentTypeId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TransactionResponseDto
{
    public string Id { get; set; } = "";
    public long Number { get; set; }
    public string Type { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string ItemDescription { get; set; } = "";
    public string? ExpenseTypeId { get; set; }
    public string? ExpenseTypeName { get; set; }
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? IncomeTypeName { get; set; }
    public string? Source { get; set; }
    public string PaymentTypeId { get; set; } = "";
    public string PaymentTypeName { get; set; } = "";
    public DateTime TransactionDate { get; set; }
    public DateTime RegisteredAt { get; set; }
    public decimal Amount { get; set; }
    public string? CardLast4 { get; set; }
    public string? Comment { get; set; }
    public bool IsActive { get; set; }
}

public class LimitWarningDto
{
    public string Message { get; set; } = "";
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Percent { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class TransactionResultDto
{
    public TransactionResponseDto Transaction { get; set; } = new();
    public LimitWarningDto? Warning { get; set; }
}

public class TransactionPageDto
{
    public List<TransactionResponseDto> Items { get; set; } = new();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    // Totals of ALL active transactions matching the filter (not just the page).
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}
