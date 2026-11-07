using Parqueo.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5195/";
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) ||
    (apiUri.Scheme != Uri.UriSchemeHttp && apiUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException(
        "Api:BaseUrl debe ser una URL absoluta HTTP o HTTPS.");
}

if (!apiBaseUrl.EndsWith("/", StringComparison.Ordinal))
{
    apiUri = new Uri($"{apiBaseUrl}/", UriKind.Absolute);
}

builder.Services.AddHttpClient<AutomovilesApiClient>(
    client => client.BaseAddress = apiUri);

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Automoviles}/{action=Index}/{id?}");

app.Run();
