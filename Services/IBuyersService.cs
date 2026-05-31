using System.Text.Json;

namespace Vkeram.Backend.Services;

public class BuyerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public interface IBuyersService
{
    Task<List<BuyerDto>> GetAllAsync();
    Task<JsonElement?> GetByIdAsync(string id);
}
