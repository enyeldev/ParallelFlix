var constructor = WebApplication.CreateBuilder(args);
constructor.Services.AddRazorPages();
constructor.Services.AddServerSideBlazor();
constructor.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("http://localhost:5000/")); // ajustar si cambia
var appFront = constructor.Build();
appFront.MapBlazorHub();
appFront.MapFallbackToPage("/_Host");
appFront.Run();

