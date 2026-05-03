using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ADH.Infrastructure.Repositories;

/// <summary>
/// Repository for managing application users with structured logging.
/// </summary>
public sealed class UserRepository : BaseRepository<AppUser, ApplicationDbContext>, IUserRepository
{
    public UserRepository(ApplicationDbContext context, IAppLogger<AppUser> logger, ICurrentUserService currentUserService) 
        : base(context, logger, currentUserService)
    {
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await Context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
