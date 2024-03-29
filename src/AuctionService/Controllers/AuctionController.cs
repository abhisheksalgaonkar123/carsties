using System.Data;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
public class AuctionController :ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(AuctionDbContext context, IMapper mapper,IPublishEndpoint publishEndpoint)
   {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
         var query = _context.Auctions
            .Include(x=>x.Item)
            .OrderBy(x=> x.Item.Make)
            .AsQueryable();
        if(!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdateAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        Console.WriteLine(date);
       return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

    }
      [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auctions = await _context.Auctions
            .Include(x=>x.Item)
            .FirstOrDefaultAsync(x=> x.Id == id);
        if(auctions == null){
            return NotFound();
        }
            
        return _mapper.Map<AuctionDto>(auctions);

    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "Test";
        _context.Auctions.Add(auction);
             var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        var result = await _context.SaveChangesAsync() > 0 ;
   
        if(!result) return BadRequest("Could not save the to the db");

        return CreatedAtAction(nameof(GetAuctionById),new {auction.Id},newAuction);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id,UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions.Include(x => x.Item)
         .FirstOrDefaultAsync(x => x.Id == id);
       
        if(auction == null) return NotFound();
        
        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Problem saving changes");
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FirstAsync(x => x.Id == id);
       
        if(auction == null) return NotFound();
        
        _context.Auctions.Remove(auction);
         await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});
        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Could not update Db.");
    }
}