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
    /// <summary>Nombre del usuario mostrado, o "Todos los usuarios".</summary>
    public string Scope { get; set; } = "";
    public bool IsAllUsers { get; set; }

    /// <summary>Período actual: de corte del usuario, o mes calendario si se ven todos.</summary>
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
