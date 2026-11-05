using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Controllers.Common;

/// <summary>
/// Documents a 200 OK response returning a typed BaseResponse&lt;T&gt; envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesOkAttribute<T>() : ProducesResponseTypeAttribute(typeof(BaseResponse<T>), StatusCodes.Status200OK);

/// <summary>
/// Documents a 200 OK response returning an untyped BaseResponse message envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesOkAttribute() : ProducesResponseTypeAttribute(typeof(BaseResponse), StatusCodes.Status200OK);

/// <summary>
/// Documents a 201 Created response returning a typed BaseResponse&lt;T&gt; envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesCreatedAttribute<T>() : ProducesResponseTypeAttribute(typeof(BaseResponse<T>), StatusCodes.Status201Created);

/// <summary>
/// Documents a 204 No Content response with no payload.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesNoContentAttribute() : ProducesResponseTypeAttribute(StatusCodes.Status204NoContent);

/// <summary>
/// Documents a 404 Not Found response returning a BaseResponse envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesNotFoundAttribute() : ProducesResponseTypeAttribute(typeof(BaseResponse), StatusCodes.Status404NotFound);

/// <summary>
/// Documents a 409 Conflict response returning a BaseResponse envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesConflictAttribute() : ProducesResponseTypeAttribute(typeof(BaseResponse), StatusCodes.Status409Conflict);

/// <summary>
/// Documents a 403 Forbidden response returning a BaseResponse envelope.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesForbiddenAttribute() : ProducesResponseTypeAttribute(typeof(BaseResponse), StatusCodes.Status403Forbidden);

/// <summary>
/// Documents a 400 Bad Request response returning a BaseResponse envelope with validation errors.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ProducesBadRequestAttribute() : ProducesResponseTypeAttribute(typeof(BaseResponse), StatusCodes.Status400BadRequest);
