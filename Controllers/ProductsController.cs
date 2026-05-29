using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductPriceRepository _productPriceRepo;

    public ProductsController(IProductService productService, IProductPriceRepository productPriceRepo)
    {
        _productService = productService;
        _productPriceRepo = productPriceRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();

        var latestPrices = await _productPriceRepo.GetLatestPerProductAsync();
        var priceMap = latestPrices.ToDictionary(p => p.ProductId);

        var result = products.Select(p => new ProductWithPriceDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = priceMap.TryGetValue(p.Id, out var pp) ? pp.Price : null
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        var latestPrice = await _productPriceRepo.GetLatestForProductAsync(id);

        var result = new ProductWithPriceDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = latestPrice?.Price
        };

        return Ok(result);
    }
}
