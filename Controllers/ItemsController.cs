using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        var items = (await _repository.GetAllAsync())
            .Select(item => item.AsDto());
        
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

        await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));
        
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
        
        await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await _repository.GetAsync(id);

        await _repository.RemoveAsync(item.Id);
        
        await _publishEndpoint.Publish(new CatalogItemDeleted(id));
        
        return NoContent();
    }
}