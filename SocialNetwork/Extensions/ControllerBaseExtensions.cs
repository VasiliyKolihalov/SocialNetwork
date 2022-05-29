using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork.Extensions;

public static class ControllerBaseExtensions
{
    public static int GetUserIdFromClaims(this ControllerBase controllerBase)
    {
        Claim userIdClaim = controllerBase.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);
        int userId = Convert.ToInt32(userIdClaim.Value);
        return userId;
    }
}