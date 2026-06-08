using System.Security.Claims;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface ICreatorHelper
{
    Task<int?> GetCreatorProfileIdAsync(ClaimsPrincipal user);
    Task<CreatorProfile?> GetCreatorProfileAsync(ClaimsPrincipal user);
}
