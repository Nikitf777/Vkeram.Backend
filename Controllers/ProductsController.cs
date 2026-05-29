using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductPriceRepository _productPriceRepo;
    private readonly IProductImageRepository _productImageRepo;
    private readonly IProductCharacteristicRepository _productCharacteristicRepo;

    public ProductsController(IProductService productService, IProductPriceRepository productPriceRepo, IProductImageRepository productImageRepo, IProductCharacteristicRepository productCharacteristicRepo)
    {
        _productService = productService;
        _productPriceRepo = productPriceRepo;
        _productImageRepo = productImageRepo;
        _productCharacteristicRepo = productCharacteristicRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductCharacteristicInclude include = ProductCharacteristicInclude.None)
    {
        var products = await _productService.GetAllAsync();

        var latestPrices = await _productPriceRepo.GetLatestPerProductAsync();
        var priceMap = latestPrices.ToDictionary(p => p.ProductId);

        List<ProductCharacteristicDto>? characteristics = null;
        if (include != ProductCharacteristicInclude.None)
        {
            var all = await Task.WhenAll(products.Select(p =>
                _productCharacteristicRepo.GetByProductIdAsync(p.Id)));
            characteristics = all.Where(c => c != null)
                .Select(c => MapCharacteristic(c!, include == ProductCharacteristicInclude.Partial))
                .ToList();
        }

        var charMap = characteristics?.ToDictionary(c => c.ProductId);

        var result = products.Select(p =>
        {
            var dto = new ProductWithPriceAndCharacteristicsDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = priceMap.TryGetValue(p.Id, out var pp) ? pp.Price : null
            };
            if (charMap != null && charMap.TryGetValue(p.Id, out var c))
                dto.Characteristics = c;
            return dto;
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, [FromQuery] bool includeCharacteristics = false)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        var latestPrice = await _productPriceRepo.GetLatestForProductAsync(id);

        var result = new ProductWithPriceAndCharacteristicsDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = latestPrice?.Price
        };

        if (includeCharacteristics)
        {
            var characteristic = await _productCharacteristicRepo.GetByProductIdAsync(id);
            if (characteristic != null)
                result.Characteristics = MapCharacteristic(characteristic, partial: false);
        }

        return Ok(result);
    }

    private static ProductCharacteristicDto MapCharacteristic(ProductCharacteristic c, bool partial)
    {
        var dto = new ProductCharacteristicDto
        {
            Id = c.Id,
            ProductId = c.ProductId
        };

        if (partial)
        {
            dto.Color = c.Color;
            dto.BrickType = c.BrickType;
            dto.SizeLengthMm = c.SizeLengthMm;
            dto.SizeWidthMm = c.SizeWidthMm;
            dto.SizeHeightMm = c.SizeHeightMm;
            dto.WeightKg = c.WeightKg;
            dto.StrengthGrade = c.StrengthGrade;
        }
        else
        {
            dto.SizeLengthMm = c.SizeLengthMm;
            dto.SizeWidthMm = c.SizeWidthMm;
            dto.SizeHeightMm = c.SizeHeightMm;
            dto.WeightKg = c.WeightKg;
            dto.StrengthGrade = c.StrengthGrade;
            dto.FrostResistance = c.FrostResistance;
            dto.WaterAbsorption = c.WaterAbsorption;
            dto.ThermalConductivity = c.ThermalConductivity;
            dto.RadiationQuality = c.RadiationQuality;
            dto.QuantityPerPallet = c.QuantityPerPallet;
            dto.Standard = c.Standard;
            dto.Color = c.Color;
            dto.BrickType = c.BrickType;
            dto.MinimumOrderQuantity = c.MinimumOrderQuantity;
        }

        return dto;
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
