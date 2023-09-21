using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Shared.Utilities;
using webapi_80.src.User.Contract;
using webapi_80.src.User.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using webapi_80.src.Shared.ViewModels;

namespace webapi_80.src.User.Services
{
    public class UserServices : IUserServices
    {

        private ApplicationDbContext publicSchemaContext;
        private IHttpContextAccessor _httpContextAccessor;
        private UserManager<UserModel> UserManager;
        ApplicationDbContext subdomainSchemaContext;

        public UserServices(ApplicationDbContext _db, IHttpContextAccessor httpContextAccessor, ApplicationDbContext subdomainSchemaContext)
        {
            this.publicSchemaContext = _db;
            _httpContextAccessor = httpContextAccessor;
            this.subdomainSchemaContext = subdomainSchemaContext;
        }

        public async Task<Page<UserModel>> GetAllUsers(int pageNumber, int pageSize, string searchparam)
        {

            IQueryable<UserModel> user = this.subdomainSchemaContext.Users;

            if (searchparam != null)
            {
                user = user.Where(x => EF.Functions.Like(x.Email, $"%{searchparam}%") ||
                                       EF.Functions.Like(x.FirstName, $"%{searchparam}%") ||
                                       EF.Functions.Like(x.LastName, $"%{searchparam}%"));

            };

            var res = await user.ToPageListAsync<UserModel>(pageNumber, pageSize);
            return res;
        }

        public async Task<UserModel> GetUserById(string ID)
        {
            return await this.subdomainSchemaContext.Users.FindAsync(ID);
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            return await this.subdomainSchemaContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> SaveChangesAsync()
        {
            var result = await subdomainSchemaContext.SaveChangesAsync();
            if (result > 0) { return true; } else { return false; }
        }

        public Task<bool> SaveNewUserProfile(UserModel user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveUser(UserModel user)
        {
            await this.subdomainSchemaContext.AddAsync(user);
            int result = await subdomainSchemaContext.SaveChangesAsync();
            if (result > 0) { return true; } else { return false; }
        }

    }
}

