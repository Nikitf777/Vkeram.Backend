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
    private readonly IProductImageRepository _productImageRepo;

    public ProductsController(IProductService productService, IProductPriceRepository productPriceRepo, IProductImageRepository productImageRepo)
    {
        _productService = productService;
        _productPriceRepo = productPriceRepo;
        _productImageRepo = productImageRepo;
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

    [HttpGet("{productId}/images")]
    public async Task<IActionResult> GetImages(string productId)
    {
        var images = await _productImageRepo.GetByProductIdAsync(productId);
        var result = images.Select(i => new
        {
            i.Id,
            i.ProductId,
            i.FileName,
            i.ContentType,
            i.CreatedAt
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{productId}/images/{imageId}/file")]
    public async Task<IActionResult> GetImageFile(string productId, int imageId)
    {
        var image = await _productImageRepo.GetByIdAsync(imageId);
        if (image == null || image.ProductId != productId)
            return NotFound();

        return File(image.ImageData, image.ContentType, image.FileName);
    }
}
