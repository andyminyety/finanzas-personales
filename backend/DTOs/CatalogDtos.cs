using System.ComponentModel.DataAnnotations;

namespace FinanzasApi.DTOs;

public class CatalogItemDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string Description { get; set; } = "";
    public bool RequiresCard { get; set; }
}

public class CatalogItemResponseDto
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public bool RequiresCard { get; set; }
    public bool IsActive { get; set; }
}
