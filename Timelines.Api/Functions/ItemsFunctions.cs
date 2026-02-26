using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Timelines.Api.Mappers;
using Timelines.Api.Models;
using Timelines.Api.Services;
using Timelines.Data;
using Timelines.Data.Entities;
using Timelines.Shared.Contracts;
using Timelines.Shared.Domain;

namespace Timelines.Api.Functions;

public class ItemsFunctions
{
    private readonly ILogger<ItemsFunctions> _logger;
    private readonly TimelinesDbContext _dbContext;
    private readonly DevUserContext _devUserContext;

    public ItemsFunctions(
        ILogger<ItemsFunctions> logger,
        TimelinesDbContext dbContext,
        DevUserContext devUserContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _devUserContext = devUserContext;
    }

    [Function("GetItems")]
    public async Task<HttpResponseData> GetItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/timelines/{timelineId}/items")] HttpRequestData req,
        string timelineId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(timelineId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var timeline = await _dbContext.Timelines
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId);

        if (timeline == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        // Parse query parameters
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var search = query["search"];
        var tagsParam = query["tags"];
        var lanesParam = query["lanes"];
        var typesParam = query["types"];
        var fromSortKeyParam = query["fromSortKey"];
        var toSortKeyParam = query["toSortKey"];
        var includeDraftsParam = query["includeDrafts"];

        bool includeDrafts = true; // default for owner
        if (includeDraftsParam != null && bool.TryParse(includeDraftsParam, out var parsedDrafts))
        {
            includeDrafts = parsedDrafts;
        }

        // Parse filter lists
        List<Guid>? tagIds = null;
        if (!string.IsNullOrWhiteSpace(tagsParam))
        {
            tagIds = ParseGuidList(tagsParam);
            if (tagIds == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid tags parameter" });
                return badResponse;
            }
        }

        List<Guid>? laneIds = null;
        if (!string.IsNullOrWhiteSpace(lanesParam))
        {
            laneIds = ParseGuidList(lanesParam);
            if (laneIds == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid lanes parameter" });
                return badResponse;
            }
        }

        List<int>? types = null;
        if (!string.IsNullOrWhiteSpace(typesParam))
        {
            types = ParseIntList(typesParam);
            if (types == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid types parameter" });
                return badResponse;
            }
        }

        long? fromSortKey = null;
        if (!string.IsNullOrWhiteSpace(fromSortKeyParam) && long.TryParse(fromSortKeyParam, out var from))
        {
            fromSortKey = from;
        }

        long? toSortKey = null;
        if (!string.IsNullOrWhiteSpace(toSortKeyParam) && long.TryParse(toSortKeyParam, out var to))
        {
            toSortKey = to;
        }

        // Build query
        var itemsQuery = _dbContext.TimelineItems
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .Where(i => i.TimelineId == id);

        // Filter by published status
        if (!includeDrafts)
        {
            itemsQuery = itemsQuery.Where(i => i.IsPublished);
        }

        // Filter by lanes
        if (laneIds != null && laneIds.Count > 0)
        {
            itemsQuery = itemsQuery.Where(i => laneIds.Contains(i.LaneId));
        }

        // Filter by types
        if (types != null && types.Count > 0)
        {
            itemsQuery = itemsQuery.Where(i => types.Contains(i.Type));
        }

        // Filter by sort key range
        if (fromSortKey.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.StartSortKey >= fromSortKey.Value);
        }

        if (toSortKey.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.StartSortKey <= toSortKey.Value);
        }

        // Filter by tags (items with ANY of the specified tags)
        if (tagIds != null && tagIds.Count > 0)
        {
            itemsQuery = itemsQuery.Where(i => i.ItemTags.Any(it => tagIds.Contains(it.TagId)));
        }

        // Get items
        var items = await itemsQuery.ToListAsync();

        // Apply search filter in memory (search across title, description, tag names, lane name)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            items = items.Where(i =>
                (i.Title?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Description?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false) ||
                i.ItemTags.Any(it => it.Tag.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase)) ||
                (i.Lane.Name?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }

        // Order by StartSortKey then DisplayOrderTiebreaker
        items = items.OrderBy(i => i.StartSortKey)
            .ThenBy(i => i.DisplayOrderTiebreaker)
            .ToList();

        var dtos = items.Select(MapToDto).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(dtos);
        return response;
    }

    [Function("CreateItem")]
    public async Task<HttpResponseData> CreateItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/timelines/{timelineId}/items")] HttpRequestData req,
        string timelineId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(timelineId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var timeline = await _dbContext.Timelines
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId);

        if (timeline == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        CreateItemRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<CreateItemRequest>();
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Title is required" });
            return badResponse;
        }

        if (request.Title.Length > 500)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Title must be 500 characters or less" });
            return badResponse;
        }

        // Validate lane belongs to timeline
        var lane = await _dbContext.Lanes.FirstOrDefaultAsync(l => l.Id == request.LaneId && l.TimelineId == id);
        if (lane == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Lane does not belong to this timeline" });
            return badResponse;
        }

        // Validate start date
        var startValidation = request.StartDate.Validate();
        if (!startValidation.IsValid)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = $"Invalid start date: {startValidation.Error}" });
            return badResponse;
        }

        // Validate end date if provided
        if (request.EndDate != null)
        {
            var endValidation = request.EndDate.Validate();
            if (!endValidation.IsValid)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = $"Invalid end date: {endValidation.Error}" });
                return badResponse;
            }
        }

        // Validate tags if provided
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            var validTags = await _dbContext.Tags
                .Where(t => t.TimelineId == id && request.TagIds.Contains(t.Id))
                .CountAsync();

            if (validTags != request.TagIds.Count)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "One or more tags do not belong to this timeline" });
                return badResponse;
            }
        }

        var entity = new TimelineItemEntity
        {
            Id = Guid.NewGuid(),
            TimelineId = id,
            LaneId = request.LaneId,
            Type = (int)request.Type,
            Title = request.Title,
            Description = request.Description,
            StartDate = DateMapper.ToDateComponent(request.StartDate),
            EndDate = request.EndDate != null ? DateMapper.ToDateComponent(request.EndDate) : null,
            DisplayOrderTiebreaker = request.DisplayOrderTiebreaker,
            IsPublished = false
        };

        _dbContext.TimelineItems.Add(entity);

        // Add item tags
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            foreach (var tagId in request.TagIds)
            {
                _dbContext.ItemTags.Add(new ItemTagEntity
                {
                    ItemId = entity.Id,
                    TagId = tagId
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created item {ItemId} in timeline {TimelineId}", entity.Id, id);

        // Reload to get computed values
        var createdItem = await _dbContext.TimelineItems
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .FirstAsync(i => i.Id == entity.Id);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(MapToDto(createdItem));
        return response;
    }

    [Function("GetItem")]
    public async Task<HttpResponseData> GetItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/items/{itemId}")] HttpRequestData req,
        string itemId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(itemId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var item = await _dbContext.TimelineItems
            .Include(i => i.Timeline)
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(item));
        return response;
    }

    [Function("UpdateItem")]
    public async Task<HttpResponseData> UpdateItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/items/{itemId}")] HttpRequestData req,
        string itemId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(itemId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        UpdateItemRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<UpdateItemRequest>();
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Title is required" });
            return badResponse;
        }

        if (request.Title.Length > 500)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Title must be 500 characters or less" });
            return badResponse;
        }

        var item = await _dbContext.TimelineItems
            .Include(i => i.Timeline)
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        // Validate lane belongs to same timeline
        var lane = await _dbContext.Lanes
            .FirstOrDefaultAsync(l => l.Id == request.LaneId && l.TimelineId == item.TimelineId);
        if (lane == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Lane does not belong to this timeline" });
            return badResponse;
        }

        // Validate start date
        var startValidation = request.StartDate.Validate();
        if (!startValidation.IsValid)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = $"Invalid start date: {startValidation.Error}" });
            return badResponse;
        }

        // Validate end date if provided
        if (request.EndDate != null)
        {
            var endValidation = request.EndDate.Validate();
            if (!endValidation.IsValid)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = $"Invalid end date: {endValidation.Error}" });
                return badResponse;
            }
        }

        // Validate tags if provided
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            var validTags = await _dbContext.Tags
                .Where(t => t.TimelineId == item.TimelineId && request.TagIds.Contains(t.Id))
                .CountAsync();

            if (validTags != request.TagIds.Count)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "One or more tags do not belong to this timeline" });
                return badResponse;
            }
        }

        // Update item properties
        item.LaneId = request.LaneId;
        item.Type = (int)request.Type;
        item.Title = request.Title;
        item.Description = request.Description;
        item.StartDate = DateMapper.ToDateComponent(request.StartDate);
        item.EndDate = request.EndDate != null ? DateMapper.ToDateComponent(request.EndDate) : null;
        item.DisplayOrderTiebreaker = request.DisplayOrderTiebreaker;

        // Replace item tags
        _dbContext.ItemTags.RemoveRange(item.ItemTags);

        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            foreach (var tagId in request.TagIds)
            {
                _dbContext.ItemTags.Add(new ItemTagEntity
                {
                    ItemId = item.Id,
                    TagId = tagId
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated item {ItemId}", id);

        // Reload to get updated values
        var updatedItem = await _dbContext.TimelineItems
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .FirstAsync(i => i.Id == id);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(updatedItem));
        return response;
    }

    [Function("DeleteItem")]
    public async Task<HttpResponseData> DeleteItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/items/{itemId}")] HttpRequestData req,
        string itemId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(itemId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var item = await _dbContext.TimelineItems
            .Include(i => i.Timeline)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _dbContext.TimelineItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted item {ItemId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("PublishItem")]
    public async Task<HttpResponseData> PublishItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/items/{itemId}/publish")] HttpRequestData req,
        string itemId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(itemId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var item = await _dbContext.TimelineItems
            .Include(i => i.Timeline)
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        if (!item.IsPublished)
        {
            item.IsPublished = true;
            item.PublishedUtc = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Published item {ItemId}", id);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(item));
        return response;
    }

    [Function("UnpublishItem")]
    public async Task<HttpResponseData> UnpublishItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/items/{itemId}/unpublish")] HttpRequestData req,
        string itemId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(itemId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var item = await _dbContext.TimelineItems
            .Include(i => i.Timeline)
            .Include(i => i.Lane)
            .Include(i => i.ItemTags)
                .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        if (item.IsPublished)
        {
            item.IsPublished = false;
            item.PublishedUtc = null;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Unpublished item {ItemId}", id);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(item));
        return response;
    }

    private static TimelineItemDto MapToDto(TimelineItemEntity entity)
    {
        return new TimelineItemDto(
            entity.Id,
            entity.TimelineId,
            entity.LaneId,
            (TimelineItemType)entity.Type,
            entity.Title,
            entity.Description,
            DateMapper.ToTimelineDate(entity.StartDate),
            entity.EndDate != null ? DateMapper.ToTimelineDate(entity.EndDate) : null,
            entity.IsPublished,
            entity.PublishedUtc,
            entity.DisplayOrderTiebreaker,
            entity.CreatedUtc,
            entity.UpdatedUtc);
    }

    private static List<Guid>? ParseGuidList(string input)
    {
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<Guid>();
        
        foreach (var part in parts)
        {
            if (!Guid.TryParse(part, out var guid))
            {
                return null;
            }
            result.Add(guid);
        }
        
        return result;
    }

    private static List<int>? ParseIntList(string input)
    {
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<int>();
        
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var value))
            {
                return null;
            }
            result.Add(value);
        }
        
        return result;
    }
}
