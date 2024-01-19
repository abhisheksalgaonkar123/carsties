namespace SearchService;
using MassTransit;
using Contracts;
using AutoMapper;
using MongoDB.Entities;
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
 {
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {

       Console.WriteLine("--> Consuming Auction Create:" + context.Message);
       var item = _mapper.Map<Item>(context.Message);
       if(item.Model == "Foo") throw new ArgumentException("Cannot sell car with name foo");
       await item.SaveAsync();
    }

}