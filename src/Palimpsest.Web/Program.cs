using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Application.Interfaces.Services;
using Palimpsest.Infrastructure.Data;
using Palimpsest.Infrastructure.Repositories;
using Palimpsest.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PalimpsestDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => 
    {
        npgsqlOptions.UseVector();
        npgsqlOptions.MigrationsAssembly("Palimpsest.Infrastructure");
    }));

// Register repositories
builder.Services.AddScoped<IUniverseRepository, UniverseRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IEntityRepository, EntityRepository>();
builder.Services.AddScoped<IEntityAliasRepository, EntityAliasRepository>();
builder.Services.AddScoped<IEntityMentionRepository, EntityMentionRepository>();
builder.Services.AddScoped<IQuestionableItemRepository, QuestionableItemRepository>();
builder.Services.AddScoped<IAssertionRepository, AssertionRepository>();
builder.Services.AddScoped<ISegmentRepository, SegmentRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();

// Register services
builder.Services.AddScoped<IUniverseContextService, UniverseContextService>();
builder.Services.AddScoped<IIngestionService, IngestionService>();
builder.Services.AddScoped<IEntityMentionService, EntityMentionService>();
builder.Services.AddScoped<IEntityResolutionService, EntityResolutionService>();
builder.Services.AddScoped<ILLMProvider, StubLLMProvider>();
builder.Services.AddScoped<IEmbeddingService, StubEmbeddingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapGet("/", () => Results.Redirect("/universes"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Universes}/{action=Index}/{id?}");

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
