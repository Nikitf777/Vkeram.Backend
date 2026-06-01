using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IHideProductsWithoutPriceRepository
{
    Task<HideProductsWithoutPrice?> GetAsync();
    Task UpdateAsync(HideProductsWithoutPrice setting);
}
