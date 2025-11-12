using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Common.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}