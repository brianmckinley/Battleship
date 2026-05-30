using Battleship.Api.DTOs;
using Battleship.Core.Validation;

namespace Battleship.Api.Mapping;

/// <summary>
/// Maps validation error codes to HTTP results (spec §9). Centralizes the
/// code → status mapping so endpoints only forward a failed result.
/// </summary>
public static class ApiErrorMapper
{
    public static int ToStatusCode(string errorCode) => errorCode switch
    {
        ValidationErrorCode.GameNotFound => StatusCodes.Status404NotFound,
        ValidationErrorCode.GameOver => StatusCodes.Status409Conflict,
        ValidationErrorCode.LocationAlreadyPlayed => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };

    public static IResult ToResult(string errorCode, string errorMessage)
    {
        var error = new ErrorResponse { Code = errorCode, Message = errorMessage };
        return Results.Json(error, statusCode: ToStatusCode(errorCode));
    }
}
