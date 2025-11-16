using NetCore_Learning.API.Helper.ServiceExtensions;
using NetCore_Learning.API.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Register all services
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();
app.UseExceptionHandler();


#region The middleware order
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Hivuong API";
        options.Theme = ScalarTheme.Default;
    });
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseCustomMiddlewares(); // Register all custom middleware
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapControllers();
#endregion The middleware order


app.Run();
