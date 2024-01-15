using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Services;

namespace SearchService;

public class DBInitilalizer 
{
  public static async Task DbInit(WebApplication app){
        app.UseAuthorization();
    app.MapControllers();
    await DB.InitAsync("SearchDb",MongoClientSettings
            .FromConnectionString(app.Configuration
            .GetConnectionString("MongoDbConnection")));
    await DB.Index<Item>()
    .Key(x=>x.Make,KeyType.Text)
    .Key(x=>x.Model,KeyType.Text)
    .Key(x=>x.Color,KeyType.Text)
    .CreateAsync();
    var count = await DB.CountAsync<Item>();
    using var scope = app.Services.CreateScope();
    var HttpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
    var items = await HttpClient.GetItemsForSearchDB();
    Console.WriteLine(items.Count + "returned from new Auction Service");
    if(items.Count > 0)  await DB.SaveAsync(items);
  }
    

}