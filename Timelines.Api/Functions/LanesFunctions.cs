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

public class LanesFunctions
{
    private readonly ILogger<LanesFunctions> _logger;
    private readonly TimelinesDbContext _dbContext;
    private readonly DevUserContext _devUserContext;

    public LanesFunctions(
        ILogger<LanesFunctions> logger,
        TimelinesDbContext dbContext,
        DevUserContext devUserContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _devUserContext = devUserContext;
    }

    [Function("GetLanes")]
    public async Task<HttpResponseData> GetLanes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/timelines/{timelineId}/lanes")] HttpRequestData req,
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

        var lanes = await _dbContext.Lanes
            .Where(l => l.TimelineId == id)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name)
            .ToListAsync();

        var dtos = lanes.Select(MapToDto).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(dtos);
        return response;
    }

    [Function("CreateLane")]
    public async Task<HttpResponseData> CreateLane(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/timelines/{timelineId}/lanes")] HttpRequestData req,
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

        CreateLaneRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<CreateLaneRequest>();
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

        if (request.Name.Length > 200)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name must be 200 characters or less" });
            return badResponse;
        }

        var entity = new LaneEntity
        {
            Id = Guid.NewGuid(),
            TimelineId = id,
            Name = request.Name,
            SortOrder = request.SortOrder
        };

        _dbContext.Lanes.Add(entity);
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("Duplicate lane name {LaneName} for timeline {TimelineId}", request.Name, id);
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { error = "A lane with this name already exists in this timeline" });
            return conflictResponse;
        }

        _logger.LogInformation("Created lane {LaneId} in timeline {TimelineId}", entity.Id, id);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(MapToDto(entity));
        return response;
    }

    [Function("UpdateLane")]
    public async Task<HttpResponseData> UpdateLane(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/lanes/{laneId}")] HttpRequestData req,
        string laneId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(laneId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        UpdateLaneRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<UpdateLaneRequest>();
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

        if (request.Name.Length > 200)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { error = "Name must be 200 characters or less" });
            return badResponse;
        }

        var lane = await _dbContext.Lanes
            .Include(l => l.Timeline)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lane == null || lane.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        lane.Name = request.Name;
        lane.SortOrder = request.SortOrder;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("Duplicate lane name {LaneName} for timeline {TimelineId}", request.Name, lane.TimelineId);
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { error = "A lane with this name already exists in this timeline" });
            return conflictResponse;
        }

        _logger.LogInformation("Updated lane {LaneId}", id);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(MapToDto(lane));
        return response;
    }

    [Function("DeleteLane")]
    public async Task<HttpResponseData> DeleteLane(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/lanes/{laneId}")] HttpRequestData req,
        string laneId)
    {
        var userId = _devUserContext.GetUserId(req);
        
        if (!Guid.TryParse(laneId, out var id))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var lane = await _dbContext.Lanes
            .Include(l => l.Timeline)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lane == null || lane.Timeline.OwnerUserId != userId)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _dbContext.Lanes.Remove(lane);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted lane {LaneId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static LaneDto MapToDto(LaneEntity entity)
    {
        return new LaneDto(
            entity.Id,
            entity.TimelineId,
            entity.Name,
            entity.SortOrder,
            entity.CreatedUtc,
            entity.UpdatedUtc);
    }
}
