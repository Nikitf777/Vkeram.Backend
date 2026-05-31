using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vkeram.Backend.Data;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IWorkDayRepository, WorkDayRepository>();
builder.Services.AddScoped<IDefaultWorkingHoursRepository, DefaultWorkingHoursRepository>();
builder.Services.AddScoped<IDefaultBreakRepository, DefaultBreakRepository>();
builder.Services.AddScoped<IMinimumBookingDaysRepository, MinimumBookingDaysRepository>();
builder.Services.AddScoped<IMinimumDeliveryDaysRepository, MinimumDeliveryDaysRepository>();
builder.Services.AddScoped<IMaximumBookingDaysRepository, MaximumBookingDaysRepository>();
builder.Services.AddScoped<IMaximumDeliveryDaysRepository, MaximumDeliveryDaysRepository>();
builder.Services.AddScoped<IAllowBookingRepository, AllowBookingRepository>();
builder.Services.AddScoped<IAllowDeliveryRepository, AllowDeliveryRepository>();
builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IProductImagePreviewRepository, ProductImagePreviewRepository>();
builder.Services.AddScoped<IReservationDurationRepository, ReservationDurationRepository>();
builder.Services.AddScoped<IProductCharacteristicRepository, ProductCharacteristicRepository>();
builder.Services.AddScoped<IImagePreviewService, ImagePreviewService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OneC:BaseUrl"]! + builder.Configuration["OneC:ProductsPath"]);
    client.DefaultRequestHeaders.Add("Authorization", builder.Configuration["OneC:AuthHeader"]);
});

builder.Services.AddHttpClient<IBuyersService, BuyersService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OneC:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Authorization", builder.Configuration["OneC:AuthHeader"]);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
