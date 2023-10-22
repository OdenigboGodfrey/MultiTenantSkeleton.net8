using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapi_80.src.Shared.Contract;
using webapi_80.src.User.Models;
using NLog;
using webapi_80.src.Shared.ViewModels;
using webapi_80.src.Tenant.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace webapi_80.src.Tenant.Controllers
{
    [Route("api/[controller]")]
    [Produces("Application/json")]
    [ApiController]
    [Authorize]

    public class TenantController(IUnitofwork unitofwork, IConfiguration config, IPasswordHasher<UserModel> _passwordHasher) : ControllerBase
    {

        private readonly IUnitofwork Services_Repo = unitofwork;
        public readonly IConfiguration config = config;
        private NLog.ILogger Log = LogManager.GetLogger("TenantController");

        [HttpGet]
        [Route("GetAllTenants")]
        public async Task<IActionResult> GetAllTenants(
           [FromQuery] int pageSize = 20,
           [FromQuery] int pageNumber = 1,
           [FromQuery] string search = null)
        {
            try
            {
                var result = await this.Services_Repo.TenantServices.GetAllTenants(pageNumber, pageSize, search);
                return Ok(new ApiResponse<Page<TenantModelVM>>
                {
                    ResponseMessage = "Successful",
                    ResponseCode = "00",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<Page<TenantModelVM>>
                {
                    ResponseMessage = ex.Message,
                    ResponseCode = "500",
                });
            }

        }


    }
}