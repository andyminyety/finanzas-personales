namespace FinanzasApi.DTOs;

public class NameAmountDto
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Percent { get; set; }
}

public class EvolutionPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class DashboardDto
{
    // Name of the user shown, or "Todos los usuarios" for all users.
    public string Scope { get; set; } = "";
    public bool IsAllUsers { get; set; }

    // Current period: the user's cut period, or the calendar month when viewing all users.
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public decimal CurrentBalance { get; set; }
    public decimal PeriodIncome { get; set; }
    public decimal PeriodExpense { get; set; }

    public bool HasLimit { get; set; }
    public decimal ExpenseLimit { get; set; }
    public decimal LimitUsedPercent { get; set; }

    public List<NameAmountDto> ExpensesByCategory { get; set; } = new();
    public List<NameAmountDto> IncomesByType { get; set; } = new();
    public List<EvolutionPointDto> Evolution { get; set; } = new();
    public List<TransactionResponseDto> LastTransactions { get; set; } = new();
}
