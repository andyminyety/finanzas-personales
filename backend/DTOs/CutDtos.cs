namespace FinanzasApi.DTOs;

public class CutResponseDto
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CutDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal FinalBalance { get; set; }
    public string Source { get; set; } = "";
}

public class CutQuery
{
    public string? UserId { get; set; }
    public int? Year { get; set; }
}

public class RunCutsDto
{
    /// <summary>Solo admin: cortar a un usuario específico. Vacío = todos los usuarios activos.</summary>
    public string? UserId { get; set; }
}

public class RunCutsResultDto
{
    public List<CutResponseDto> Created { get; set; } = new();
    public int UsersProcessed { get; set; }
    public string Message { get; set; } = "";
}
