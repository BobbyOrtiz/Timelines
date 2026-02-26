using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Timelines.Api.Models;
using Timelines.Api.Services;
using Timelines.Data;
using Timelines.Data.Entities;
using Timelines.Shared.Contracts;

namespace Timelines.Api.Functions;

public class TagsFunctions
{
    private readonly ILogger<TagsFunctions> _logger;
    private readonly TimelinesDbContext _dbContext;
    private readonly DevUserContext _devUserContext;

    public TagsFunctions(
        ILogger<TagsFunctions> logger,
        TimelinesDbContext dbContext,
        DevUserContext devUserContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _devUserContext = devUserContext;
    }

    [Function("GetTags")]
    public async Task<HttpResponseData> GetTags(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/timelines/{timelineId}/tags")] HttpRequestData req,
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

        var tags = await _dbContext.Tags
            .Where(t => t.TimelineId == id)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var dtos = tags.Select(MapToDto).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(dtos);
        return response;
    }

    [Function("CreateTag")]
    public async Task<HttpResponseData> CreateTag(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/timelines/{timelineId}/tags")] HttpRequestData req,
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

        CreateTagRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<CreateTagRequest>();
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name is required" });
            return badResponse;
        }

        if (request.Name.Length > 100)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name must be 100 characters or less" });
            return badResponse;
        }

        var entity = new TagEntity
        {
            Id = Guid.NewGuid(),
            TimelineId = id,
            Name = request.Name
        };

        _dbContext.Tags.Add(entity);
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("Duplicate tag name {TagName} for timeline {TimelineId}", request.Name, id);
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { error = "A tag with this name already exists in this timeline" });
            return conflictResponse;
        }

        _logger.LogInformation("Created tag {TagId} in timeline {TimelineId}", entity.Id, id);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(MapToDto(entity));
        return response;
    }

    [Function("UpdateTag")]
    public async Task<HttpResponseData> UpdateTag(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/tags/{tagId}")] HttpRequestData req,
        string tagId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(tagId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        UpdateTagRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<UpdateTagRequest>();
        }
        catch
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name is required" });
            return badResponse;
        }

        if (request.Name.Length > 100)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name must be 100 characters or less" });
            return badResponse;
        }

        var tag = await _dbContext.Tags
            .Include(t => t.Timeline)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null || tag.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        tag.Name = request.Name;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("Duplicate tag name {TagName} for timeline {TimelineId}", request.Name, tag.TimelineId);
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { error = "A tag with this name already exists in this timeline" });
            return conflictResponse;
        }

        _logger.LogInformation("Updated tag {TagId}", id);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(tag));
        return response;
    }

    [Function("DeleteTag")]
    public async Task<HttpResponseData> DeleteTag(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/tags/{tagId}")] HttpRequestData req,
        string tagId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(tagId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var tag = await _dbContext.Tags
            .Include(t => t.Timeline)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null || tag.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted tag {TagId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static TagDto MapToDto(TagEntity entity)
    {
        return new TagDto(
            entity.Id,
            entity.TimelineId,
            entity.Name,
            entity.CreatedUtc,
            entity.UpdatedUtc);
    }
}
