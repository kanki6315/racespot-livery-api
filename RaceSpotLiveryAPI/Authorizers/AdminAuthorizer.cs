using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Authorizers
{
    public class AdminAuthorizer : AuthorizationHandler<AdminRequirement>
    {
        RaceSpotDBContext _context;

        public AdminAuthorizer(RaceSpotDBContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            if (await IsAdmin(context))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return;
        }

        protected async Task<bool> IsAdmin(AuthorizationHandlerContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return false;
            }
            ApplicationUser user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == context.User.Identity.Name);
            if (user == null || !user.IsAdmin)
            {
                return false;
            }
            return true;
        }

    }

}
