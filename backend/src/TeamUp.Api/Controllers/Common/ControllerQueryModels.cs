// Date: 2026-11-27
// Purpose: Typed [FromQuery] filter parameter records preserving exact flat camelCase query-string & OpenAPI 3.2 parameter names.

using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Api.Controllers.Common;

public sealed record EventFilterQuery(
    [property: FromQuery(Name = "search")] string? Search = null,
    [property: FromQuery(Name = "organizationId")] int? OrganizationId = null,
    [property: FromQuery(Name = "type")] EventType? Type = null,
    [property: FromQuery(Name = "from")] DateTime? From = null,
    [property: FromQuery(Name = "to")] DateTime? To = null,
    [property: FromQuery(Name = "deliveryMode")] EventDeliveryMode? DeliveryMode = null,
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "pageSize")] int PageSize = 20)
{
    public PageRequest ToPageRequest() => new(Page, PageSize);
}

public sealed record OrganizationFilterQuery(
    [property: FromQuery(Name = "search")] string? Search = null,
    [property: FromQuery(Name = "category")] string? Category = null,
    [property: FromQuery(Name = "scope")] OrganizationScope? Scope = null,
    [property: FromQuery(Name = "universityId")] int? UniversityId = null,
    [property: FromQuery(Name = "recruitingOnly")] bool? RecruitingOnly = null,
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "pageSize")] int PageSize = 20)
{
    public PageRequest ToPageRequest() => new(Page, PageSize);
}

public sealed record AuditLogFilterQuery(
    [property: FromQuery(Name = "action")] string? Action = null,
    [property: FromQuery(Name = "entityType")] string? EntityType = null,
    [property: FromQuery(Name = "actorStudentId")] int? ActorStudentId = null,
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "pageSize")] int PageSize = 20)
{
    public PageRequest ToPageRequest() => new(Page, PageSize);
}
