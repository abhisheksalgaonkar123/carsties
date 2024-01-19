using System.Net;
using MassTransit;
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit( x=> {
  x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
  x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false));
  x.UsingRabbitMq((context,cfg) => {

     cfg.ReceiveEndpoint("search-auction-created",e=> {
      e.UseMessageRetry(r => r.Interval(5,5));
      e.ConfigureConsumer<AuctionCreatedConsumer>(context);
    });
    cfg.ConfigureEndpoints(context);
  });
});
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


