using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Location;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/locations")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminLocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public AdminLocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminLocationDto>>>> GetLocations([FromQuery] string? search, [FromQuery] int? type, [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var locations = await _locationService.GetAdminLocationsAsync(search, type, status, page, pageSize);
        return Ok(new ApiResponse<List<AdminLocationDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.ADMIN_LOCATIONS_RETRIEVED_SUCCESS,
            Data = locations.Items,
            MetaData = locations.MetaData
        });
    }

    [HttpGet("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> GetLocationDetail(Guid locationId)
    {
        var location = await _locationService.GetAdminLocationDetailAsync(locationId);
        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.ADMIN_LOCATION_DETAIL_RETRIEVED_SUCCESS,
            Data = location
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> CreateLocation([FromBody] AdminUpsertLocationRequest request)
    {
        var location = await _locationService.CreateAdminLocationAsync(request);
        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.ADMIN_LOCATION_CREATED_SUCCESS,
            Data = location
        });
    }

    [HttpPut("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> UpdateLocation(Guid locationId, [FromBody] AdminUpsertLocationRequest request)
    {
        var location = await _locationService.UpdateAdminLocationAsync(locationId, request);
        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.ADMIN_LOCATION_UPDATED_SUCCESS,
            Data = location
        });
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> HideLocation(Guid locationId)
    {
        var location = await _locationService.HideAdminLocationAsync(locationId);
        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.ADMIN_LOCATION_HIDDEN_SUCCESS,
            Data = location
        });
    }
}
