using AutoMapper;
using Contracts;
using SearchService;

public class MappingProfiles:Profile
{
   public MappingProfiles()
   {
      CreateMap<AuctionCreated,Item>();
      CreateMap<AuctionUpdated,Item>();
      CreateMap<AuctionDeleted,Item>();
   }
}