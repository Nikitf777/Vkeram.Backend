namespace Vkeram.Backend.DTOs;

public class SaveProductCharacteristicRequest
{
    public int? SizeLengthMm { get; set; }
    public int? SizeWidthMm { get; set; }
    public int? SizeHeightMm { get; set; }
    public float? WeightKg { get; set; }
    public string? StrengthGrade { get; set; }
    public string? FrostResistance { get; set; }
    public string? WaterAbsorption { get; set; }
    public float? ThermalConductivity { get; set; }
    public string? RadiationQuality { get; set; }
    public int? QuantityPerPallet { get; set; }
    public string? Standard { get; set; }
    public string? Color { get; set; }
    public string? BrickType { get; set; }
    public int? MinimumOrderQuantity { get; set; }
}
