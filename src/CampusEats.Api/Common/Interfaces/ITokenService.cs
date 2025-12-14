using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Common.Interfaces;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
}