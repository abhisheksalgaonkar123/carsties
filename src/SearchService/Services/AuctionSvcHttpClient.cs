using MongoDB.Entities;

namespace SearchService.Services;
public class AuctionSvcHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient client,IConfiguration config)
    {
        _client = client;
        _config = config;
    }
    public async Task<List<Item>> GetItemsForSearchDB()
    {
      var lastUpdated = await DB.Find<Item,string>()
                           .Sort(a=>a.Descending(a=>a.UpdateAt))
                           .Project(a => a.UpdateAt.ToString())
                           .ExecuteFirstAsync();
    var url = _config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated;
     Console.WriteLine(url);
     return await _client.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
    }
}