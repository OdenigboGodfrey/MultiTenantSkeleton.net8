using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using webapi_80.src.Shared.Contract;
using webapi_80.src.User.Models;
using NLog;
using webapi_80.src.Tenant.SchemaTenant;
using webapi_80.src.Shared.ViewModels;
using webapi_80.src.User.ViewModels;
using webapi_80.src.Shared.Utilities;
using Microsoft.AspNetCore.Identity;

namespace webapi_80.src.User.Controllers
{
    [Route("api/[controller]")]
    [Produces("Application/json")]
    [ApiController]

    public class IdentityController : ControllerBase
    {

        private readonly IUnitofwork Services_Repo;
        public readonly IConfiguration config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private NLog.ILogger Log = LogManager.GetLogger("IdentityController");
        private ITenantSchema tenantSchema;
        private readonly IPasswordHasher<UserModel> _passwordHasher;

        public IdentityController(IUnitofwork unitofwork, IConfiguration config, IHttpContextAccessor httpContextAccessor, ITenantSchema tenantSchema, IPasswordHasher<UserModel> _passwordHasher)

        {
            this.Services_Repo = unitofwork;
            this.config = config;
            _httpContextAccessor = httpContextAccessor;
            this.tenantSchema = tenantSchema;
            this._passwordHasher = _passwordHasher;

        }
        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(
           [FromQuery] int pageSize = 20,
           [FromQuery] int pageNumber = 1,
           [FromQuery] string search = null)
        {
            try
            {
                var result = await this.Services_Repo.UserServices.GetAllUsers(pageNumber, pageSize, search);
                return Ok(new ApiResponse<Page<UserModel>>
                {
                    ResponseMessage = "Successful",
                    ResponseCode = "00",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<Page<UserModel>>
                {
                    ResponseMessage = ex.Message,
                    ResponseCode = "500",
                    Data = null
                });
            }

        }

        [HttpPost]
        [Route("UserSignup")]
        public async Task<IActionResult> UserSignup([FromBody] UserSignupModel Model)
        {

            var User = await this.Services_Repo.UserServices.GetUserByEmail(Model.Email);
            if (User == null)
            {

                UserModel NewUser = new UserModel
                {
                    Email = Model.Email,
                    FirstName = Model.FirstName,
                    LastName = Model.LastName,
                    Created_At = DateTime.Now,
                    Status = "Active",
                    Password = _passwordHasher.HashPassword(null, Model.Password)
                };

                var Result = await this.Services_Repo.UserServices.SaveUser(NewUser);

                Log.Info("Create user response " + JsonConvert.SerializeObject(Result));
                if (Result)
                {
                    var user = this.Services_Repo.UserServices.GetUserByEmail(Model.Email);
                    return Ok(new ApiResponse<Boolean>
                    {
                        ResponseCode = "00",
                        ResponseMessage = "User creation was successful",
                        Data = Result,
                        user = user.Result
                    });

                }
                else
                {
                    return BadRequest(new ApiResponse<Boolean>
                    {
                        ResponseCode = "400",
                        ResponseMessage = "User creation failed",
                        Data = Result
                    });
                }
            }
            else
            {
                return BadRequest(new ApiResponse<Boolean>
                {
                    ResponseCode = "409",
                    ResponseMessage = "Email Already Exist",
                    Data = false
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> UserSignup([FromBody] LoginModel Model)
        {

            var user = await this.Services_Repo.UserServices.GetUserByEmail(Model.Email);
            if (user == null)
            {

                return NotFound(new ApiResponse<Boolean>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Email/password incorrect.",
                    Data = false
                });
            }
            else
            {
                var verifyHashedPasswordResult = _passwordHasher.VerifyHashedPassword(null, user.Password, Model.Password);
                if (verifyHashedPasswordResult == PasswordVerificationResult.Success)
                {
                    return Ok(new ApiResponse<Boolean>
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Login ok",
                        Data = true,
                        user = user
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<Boolean>
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Email/password incorrect.",
                        Data = false
                    });
                }
            }
        }

        // [HttpGet]
        // [Route("GetAllUsers")]
        // public async Task<IActionResult> GetUsers(
        //     [FromQuery] int pageSize = 20,
        //    [FromQuery] int pageNumber = 1)
        // {

        //     Page<UserModel> users = await this.Services_Repo.UserServices.GetAllUsers(pageNumber, pageSize, null);
        //     if (users.Items.Count() < 1)
        //     {

        //         return NotFound(new ApiResponse<Page<UserModel>>
        //         {
        //             ResponseCode = "400",
        //             ResponseMessage = "No records found.",
        //             Data = users,
        //         });
        //     }
        //     else
        //     {
        //         return Ok(new ApiResponse<Page<UserModel>>
        //         {
        //             ResponseCode = "200",
        //             ResponseMessage = "Login ok",
        //             Data = users,
        //         });
        //     }
        // }
    }
}