using System.ComponentModel.DataAnnotations;

namespace FinanzasApi.DTOs;

public class ExpenseItemDto
{
    [Required] public string ExpenseTypeId { get; set; } = "";
    [Required] public string CategoryId { get; set; } = "";
    [Required] public string DefaultPaymentTypeId { get; set; } = "";
    [Required, StringLength(120, MinimumLength = 2)] public string Description { get; set; } = "";
}

public class ExpenseItemResponseDto
{
    public string Id { get; set; } = "";
    public string ExpenseTypeId { get; set; } = "";
    public string ExpenseTypeName { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string DefaultPaymentTypeId { get; set; } = "";
    public string DefaultPaymentTypeName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; }
}

public class IncomeItemDto
{
    [Required] public string IncomeTypeId { get; set; } = "";
    [Required, StringLength(120, MinimumLength = 2)] public string Description { get; set; } = "";
    [Required, StringLength(120, MinimumLength = 2)] public string Source { get; set; } = "";
}

public class IncomeItemResponseDto
{
    public string Id { get; set; } = "";
    public string IncomeTypeId { get; set; } = "";
    public string IncomeTypeName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = "";
    public bool IsActive { get; set; }
}
