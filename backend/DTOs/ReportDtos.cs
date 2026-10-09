namespace FinanzasApi.DTOs;

public class ReportQuery
{
    /// <summary>Solo lo respeta si quien llama es administrador.</summary>
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    /// <summary>Solo para el resumen mensual. Por defecto, el año actual.</summary>
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
    /// <summary>Solo en el reporte por categoría: % del total de egresos.</summary>
    public decimal Percent { get; set; }
}

public class ReportDto
{
    /// <summary>range | by-user | by-type | by-category | monthly</summary>
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Scope { get; set; } = "";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Year { get; set; }
    public ReportTotalsDto Totals { get; set; } = new();
    public List<ReportRowDto> Rows { get; set; } = new();
    /// <summary>Solo en el reporte por rango de fechas: el detalle de transacciones.</summary>
    public List<TransactionResponseDto> Transactions { get; set; } = new();
}
