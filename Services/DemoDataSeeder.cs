using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Data;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Services;

public class DemoDataSeeder : IDemoDataSeeder
{
    private readonly AppDbContext _db;
    private readonly IProductService _productService;
    private readonly IBuyersService _buyersService;
    private readonly IInviteCodeRepository _inviteRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IProductPriceRepository _productPriceRepo;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IBillStatusService _billStatusService;
    private readonly ILogger<DemoDataSeeder> _logger;
    private readonly Random _rng = new();

    private static readonly string[] FirstNames = ["Иван", "Пётр", "Сергей", "Алексей", "Дмитрий", "Андрей", "Максим", "Владимир", "Александр", "Евгений", "Николай", "Михаил", "Павел", "Артём", "Виктор"];
    private static readonly string[] LastNames = ["Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Зайцев", "Соколов", "Михайлов", "Фёдоров", "Морозов", "Волков", "Алексеев", "Лебедев"];

    public DemoDataSeeder(
        AppDbContext db,
        IProductService productService,
        IBuyersService buyersService,
        IInviteCodeRepository inviteRepo,
        IUserRepository userRepo,
        IOrderRepository orderRepo,
        IProductPriceRepository productPriceRepo,
        IOrderConfirmationService orderConfirmationService,
        IBillStatusService billStatusService,
        ILogger<DemoDataSeeder> logger)
    {
        _db = db;
        _productService = productService;
        _buyersService = buyersService;
        _inviteRepo = inviteRepo;
        _userRepo = userRepo;
        _orderRepo = orderRepo;
        _productPriceRepo = productPriceRepo;
        _orderConfirmationService = orderConfirmationService;
        _billStatusService = billStatusService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!await CheckTablesEmptyAsync())
        {
            _logger.LogInformation("Tables are not empty, skipping seed.");
            return;
        }

        _logger.LogInformation("Starting demo data seeding...");
        var sw = Stopwatch.StartNew();

        var products = await FetchProductsAsync();
        if (products == null || products.Count == 0)
        {
            _logger.LogWarning("No products fetched from external API, seeding aborted.");
            return;
        }
        _logger.LogInformation("Fetched {Count} products", products.Count);

        var buyers = await FetchBuyersAsync();
        if (buyers == null || buyers.Count == 0)
        {
            _logger.LogWarning("No buyers fetched from external API, seeding aborted.");
            return;
        }
        _logger.LogInformation("Fetched {Count} buyers", buyers.Count);

        var priceMap = await CreateProductPricesAsync(products);
        _logger.LogInformation("Created {Count} product prices", priceMap.Count);

        var users = await CreateUsersAsync(buyers);
        _logger.LogInformation("Created {Count} users", users.Count);

        await CreateOrdersAsync(users, products, priceMap);
        _logger.LogInformation("Created 1000 orders");

        sw.Stop();
        _logger.LogInformation("Demo data seeding completed in {Elapsed}", sw.Elapsed);
    }

    private async Task<bool> CheckTablesEmptyAsync()
    {
        var hasOrders = await _db.Orders.AnyAsync();
        var hasUsers = await _db.Users.AnyAsync();
        var hasInvites = await _db.InviteCodes.AnyAsync();
        var hasPrices = await _db.ProductPrices.AnyAsync();
        return !hasOrders && !hasUsers && !hasInvites && !hasPrices;
    }

    private async Task<List<ProductDto>> FetchProductsAsync()
    {
        try
        {
            return await _productService.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch products from external API");
            return [];
        }
    }

    private async Task<List<BuyerDto>> FetchBuyersAsync()
    {
        try
        {
            return await _buyersService.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch buyers from external API");
            return [];
        }
    }

    private async Task<Dictionary<string, ProductPrice>> CreateProductPricesAsync(List<ProductDto> products)
    {
        var map = new Dictionary<string, ProductPrice>();
        foreach (var product in products)
        {
            var price = new ProductPrice
            {
                ProductId = product.Id,
                Price = Math.Round((decimal)(_rng.NextDouble() * 250 + 50), 2),
                CreatedAt = DateTime.UtcNow.AddDays(-_rng.Next(30, 400))
            };
            await _productPriceRepo.AddAsync(price);
            map[product.Id] = price;
        }
        return map;
    }

    private async Task<List<User>> CreateUsersAsync(List<BuyerDto> buyers)
    {
        var users = new List<User>();
        var allInvites = new List<InviteCode>();

        foreach (var buyer in buyers)
        {
            var inviteCount = _rng.Next(2, 6);
            for (int i = 0; i < inviteCount; i++)
            {
                allInvites.Add(new InviteCode
                {
                    Code = $"SEED-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                    BuyerId = buyer.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(365),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _inviteRepo.CreateRangeAsync(allInvites);

        var userIndex = 1;
        foreach (var buyer in buyers)
        {
            var userCount = _rng.Next(1, 4);
            var buyerInvites = allInvites.Where(i => i.BuyerId == buyer.Id).ToList();

            for (int i = 0; i < userCount && i < buyerInvites.Count; i++)
            {
                var firstName = FirstNames[_rng.Next(FirstNames.Length)];
                var lastName = LastNames[_rng.Next(LastNames.Length)];
                var email = $"demo.{lastName.ToLowerInvariant()}.{firstName.ToLowerInvariant()}.{userIndex}@example.com";

                var user = new User
                {
                    BuyerId = buyer.Id,
                    ContactEmail = email,
                    ContactName = $"{firstName} {lastName}",
                    Phone = $"+7{_rng.Next(900, 999)}{_rng.Next(1000000, 9999999)}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123456"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-_rng.Next(1, 400))
                };

                var createdUser = await _userRepo.CreateAsync(user);

                var invite = buyerInvites[i];
                invite.IsUsed = true;
                invite.UsedByUserId = createdUser.Id;
                invite.UsedAt = DateTime.UtcNow;

                users.Add(createdUser);
                userIndex++;
            }
        }

        return users;
    }

    private async Task CreateOrdersAsync(List<User> users, List<ProductDto> products, Dictionary<string, ProductPrice> priceMap)
    {
        var now = DateTime.UtcNow;

        for (int i = 0; i < 1000; i++)
        {
            var user = users[_rng.Next(users.Count)];
            var daysAgo = _rng.Next(3, 366);
            var createdAt = now.AddDays(-daysAgo).AddMinutes(-_rng.Next(0, 1440));

            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = createdAt,
                IsConfirmed = false
            };

            var reservationCount = _rng.Next(0, 5);
            var deliveryCount = _rng.Next(0, 3);

            for (int r = 0; r < reservationCount; r++)
            {
                var day = DateOnly.FromDateTime(createdAt.AddDays(r * _rng.Next(1, 8)));
                var startHour = _rng.Next(8, 17);
                var durationMinutes = 30 * _rng.Next(1, 5);

                var reservation = new OrderReservation
                {
                    Day = day,
                    StartTime = new TimeOnly(startHour, 0),
                    EndTime = new TimeOnly(startHour, 0).AddMinutes(durationMinutes),
                    Picked = daysAgo > 7 && _rng.NextDouble() < 0.4,
                };

                var productCount = _rng.Next(1, 4);
                var usedProductIds = new HashSet<string>();
                for (int p = 0; p < productCount; p++)
                {
                    var product = products[_rng.Next(products.Count)];
                    if (!usedProductIds.Add(product.Id)) continue;

                    if (!priceMap.TryGetValue(product.Id, out var priceEntry)) continue;

                    reservation.ProductReservations.Add(new ProductReservation
                    {
                        ProductId = product.Id,
                        Quantity = _rng.Next(1, 11),
                        ProductPriceId = priceEntry.Id,
                        Vat = product.Vat
                    });
                }

                if (reservation.ProductReservations.Count > 0)
                    order.Reservations.Add(reservation);
            }

            for (int d = 0; d < deliveryCount; d++)
            {
                var deliveryDay = createdAt.AddDays(d * _rng.Next(1, 15) + 1);
                var delivery = new OrderDelivery
                {
                    DeliveryTime = deliveryDay,
                    Delivered = daysAgo > 14 && _rng.NextDouble() < 0.3,
                };

                var productCount = _rng.Next(1, 4);
                var usedProductIds = new HashSet<string>();
                for (int p = 0; p < productCount; p++)
                {
                    var product = products[_rng.Next(products.Count)];
                    if (!usedProductIds.Add(product.Id)) continue;

                    if (!priceMap.TryGetValue(product.Id, out var priceEntry)) continue;

                    delivery.ProductReservations.Add(new ProductReservation
                    {
                        ProductId = product.Id,
                        Quantity = _rng.Next(1, 11),
                        ProductPriceId = priceEntry.Id,
                        Vat = product.Vat
                    });
                }

                if (delivery.ProductReservations.Count > 0)
                    order.Deliveries.Add(delivery);
            }

            if (order.Reservations.Count == 0 && order.Deliveries.Count == 0)
            {
                var reservation = new OrderReservation
                {
                    Day = DateOnly.FromDateTime(createdAt),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0),
                    Picked = false,
                };
                var product = products[_rng.Next(products.Count)];
                if (priceMap.TryGetValue(product.Id, out var priceEntry))
                {
                    reservation.ProductReservations.Add(new ProductReservation
                    {
                        ProductId = product.Id,
                        Quantity = _rng.Next(1, 6),
                        ProductPriceId = priceEntry.Id,
                        Vat = product.Vat
                    });
                }
                order.Reservations.Add(reservation);
            }

            var savedOrder = await _orderRepo.CreateAsync(order);

            if (daysAgo > 7 && _rng.NextDouble() < 0.5)
                await ConfirmAndVaryStatusAsync(savedOrder.Id);
        }
    }

    private async Task ConfirmAndVaryStatusAsync(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.User == null) return;

        try
        {
            var billId = await _orderConfirmationService.ConfirmOrderAsync(order);
            if (billId == null) return;

            await _orderRepo.UpdateAsync(order);

            var paymentRoll = _rng.NextDouble();
            var shipmentRoll = _rng.NextDouble();

            string paymentStatus;
            if (paymentRoll < 0.3)
                paymentStatus = "paid";
            else if (paymentRoll < 0.5)
                paymentStatus = "partial";
            else
                paymentStatus = "unpaid";

            string shipmentStatus;
            if (shipmentRoll < 0.2)
                shipmentStatus = "shipped";
            else if (shipmentRoll < 0.4)
                shipmentStatus = "partial";
            else
                shipmentStatus = "unshipped";

            await _billStatusService.UpdateBillStatusAsync(new UpdateBillStatusRequest
            {
                BillId = billId,
                PaymentStatus = paymentStatus,
                ShipmentStatus = shipmentStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to confirm order {OrderId}", orderId);
        }
    }
}
