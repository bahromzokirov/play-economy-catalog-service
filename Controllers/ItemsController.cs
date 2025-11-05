using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _repository;
    public static int RequestCounter;
    
    public ItemsController(IRepository<Item> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    static ItemsController()
    {
        RequestCounter = 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        RequestCounter++;
        Console.WriteLine($"Request{RequestCounter}: Starting...");

        if (RequestCounter <= 2)
        {
            Console.WriteLine("Request {requestCounter}: Delaying...}");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        if (RequestCounter <= 4)
        {
            Console.WriteLine($"Request {RequestCounter}: 500 (Internal Server Error).");
            return StatusCode(500);
        }
        
        var items = (await _repository.GetAllAsync())
            .Select(item => item.AsDto());
        
        Console.WriteLine($"Request {RequestCounter}: 200 (OK).");
        
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetAsync(id);

        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        var item = new Item
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };
        
        await _repository.CreateAsync(item);
        
        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var existingItem = await _repository.GetAsync(id);

        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;
        
        await _repository.UpdateAsync(existingItem);
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await _repository.GetAsync(id);

        await _repository.RemoveAsync(item.Id);
        
        return NoContent();
    }
}