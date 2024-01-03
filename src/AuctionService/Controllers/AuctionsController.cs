﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(
                x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0
            );
        }
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO: Add current user and seller
        auction.Seller = "test";

        _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
            return BadRequest("Couldn't save changes");

        return CreatedAtAction(
            nameof(GetAuctionById),
            new { auction.Id },
            _mapper.Map<AuctionDto>(auction)
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateauctionDto)
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        //TODO: Check Seller == username

        auction.Item.Make = updateauctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateauctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateauctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateauctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Color = updateauctionDto.Color ?? auction.Item.Color;

        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
            return BadRequest("Couldn't save changes");

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
            return NotFound();

        // TODO: check seller == username
        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
            return BadRequest("Couldn't save changes");

        return Ok();
    }
}
