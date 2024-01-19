namespace SearchService;
using MassTransit;
using Contracts;
using AutoMapper;
using MongoDB.Entities;
public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
 {
    private readonly IMapper _mapper;
    public AuctionDeletedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {

       Console.WriteLine("--> Consuming Auction Create:" + context.Message);
       var item = _mapper.Map<Item>(context.Message);
       Console.WriteLine("Deleteing Item with Id :" + item.ID);
       await DB.DeleteAsync<Item>(item.ID);
    }

}