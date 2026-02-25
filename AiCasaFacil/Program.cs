using AiCasaFacil.Application.Interfaces;
using AiCasaFacil.Application.Services;
using AiCasaFacil.Components;
using AiCasaFacil.Infrastructure.AI;
using AiCasaFacil.Infrastructure.Data;
using AiCasaFacil.Infrastructure.Import;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IProdutoRepository, ProdutoRepositoryEmMemoria>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IAnalisePedidosService, AnalisePedidosService>();
builder.Services.AddScoped<ProdutoIAService>();
builder.Services.AddScoped<PedidoIAService>();
builder.Services.AddScoped<CsvPedidoImportService>();
builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("AI")
);

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10); 
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
