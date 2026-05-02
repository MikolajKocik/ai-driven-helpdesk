using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Core.Entities;

namespace ADH.Core.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task AddAsync(AppUser user);
    Task UpdateAsync(AppUser user);
}
