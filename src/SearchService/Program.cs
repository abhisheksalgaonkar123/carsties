using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Lifetime.ApplicationStarted.Register(async () => {
        try
        {
        await DBInitilalizer.DbInit(app);
        }
        catch(Exception ex)
        {
                Console.WriteLine(ex);
        }

});
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

app.Run();


