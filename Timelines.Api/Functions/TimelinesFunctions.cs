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

public class TimelinesFunctions
{
    private readonly ILogger<TimelinesFunctions> _logger;
    private readonly TimelinesDbContext _dbContext;
    private readonly DevUserContext _devUserContext;

    public TimelinesFunctions(
        ILogger<TimelinesFunctions> logger,
        TimelinesDbContext dbContext,
        DevUserContext devUserContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _devUserContext = devUserContext;
    }

    [Function("GetTimelines")]
    public async Task<HttpResponseData> GetTimelines(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/timelines")] HttpRequestData req)
    {
        var userId = _devUserContext.GetUserId(req);
        _logger.LogInformation("Getting timelines for user {UserId}", userId);

        var timelines = await _dbContext.Timelines
            .Where(t => t.OwnerUserId == userId)
            .OrderByDescending(t => t.UpdatedUtc)
            .ToListAsync();

        var dtos = timelines.Select(MapToDto).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(dtos);
        return response;
    }

    [Function("GetTimeline")]
    public async Task<HttpResponseData> GetTimeline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/timelines/{timelineId}")] HttpRequestData req,
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

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(timeline));
        return response;
    }

    [Function("CreateTimeline")]
    public async Task<HttpResponseData> CreateTimeline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/timelines")] HttpRequestData req)
    {
        var userId = _devUserContext.GetUserId(req);
        
        CreateTimelineRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<CreateTimelineRequest>();
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

        var entity = new TimelineEntity
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Title = request.Title,
            Description = request.Description,
            IsPublic = request.IsPublic,
            IsIndexed = request.IsIndexed,
            DefaultView = (int)request.DefaultView,
            DefaultZoom = (int)request.DefaultZoom
        };

        _dbContext.Timelines.Add(entity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created timeline {TimelineId} for user {UserId}", entity.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(MapToDto(entity));
        return response;
    }

    [Function("UpdateTimeline")]
    public async Task<HttpResponseData> UpdateTimeline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/timelines/{timelineId}")] HttpRequestData req,
        string timelineId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(timelineId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        UpdateTimelineRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<UpdateTimelineRequest>();
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

        var timeline = await _dbContext.Timelines
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId);

        if (timeline == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        timeline.Title = request.Title;
        timeline.Description = request.Description;
        timeline.IsPublic = request.IsPublic;
        timeline.IsIndexed = request.IsIndexed;
        timeline.DefaultView = (int)request.DefaultView;
        timeline.DefaultZoom = (int)request.DefaultZoom;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated timeline {TimelineId}", id);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(timeline));
        return response;
    }

    [Function("DeleteTimeline")]
    public async Task<HttpResponseData> DeleteTimeline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/timelines/{timelineId}")] HttpRequestData req,
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

        _dbContext.Timelines.Remove(timeline);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted timeline {TimelineId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static TimelineDto MapToDto(TimelineEntity entity)
    {
        return new TimelineDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.IsPublic,
            entity.IsIndexed,
            (TimelineViewMode)entity.DefaultView,
            (TimelineZoomLevel)entity.DefaultZoom,
            entity.CreatedUtc,
            entity.UpdatedUtc);
    }
}
