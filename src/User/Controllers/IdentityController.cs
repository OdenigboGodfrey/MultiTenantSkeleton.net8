using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi_80.src.Shared.Contract;
using webapi_80.src.User.Models;
using NLog;
using webapi_80.src.Shared.ViewModels;
using webapi_80.src.User.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using webapi_80.src.Shared.Utilities;
using System.Text;

namespace webapi_80.src.User.Controllers
{
    [Route("api/[controller]")]
    [Produces("Application/json")]
    [ApiController]
    // [Authorize]
    [Authorize(Policy = "SubdomainPolicy")]

    public class IdentityController(IUnitofwork unitofwork, IConfiguration config, IPasswordHasher<UserModel> _passwordHasher) : ControllerBase
    {

        private readonly IUnitofwork Services_Repo = unitofwork;
        public readonly IConfiguration config = config;
        private NLog.ILogger Log = LogManager.GetLogger("IdentityController");
        private readonly IPasswordHasher<UserModel> _passwordHasher = _passwordHasher;

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
        [AllowAnonymous]
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
        [AllowAnonymous]
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
                    var accessToken = await GenerateJwtTokenAsync(Model.Email, user, unitofwork.subdomain);
                    if (accessToken == null)
                    {
                        return BadRequest(new ApiResponse<String>
                        {
                            ResponseCode = "400",
                            ResponseMessage = "Token generation failed.",
                            Data = ""
                        });
                    }
                    return Ok(new ApiResponse<String>
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Login ok",
                        Data = accessToken.ToString(),
                        user = user,
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<String>
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Email/password incorrect.",
                        Data = ""
                    });
                }
            }
        }

        private async Task<object> GenerateJwtTokenAsync(string email, UserModel user, String subdomain)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.GroupSid, subdomain),
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtokenOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                JwtokenOptions.Issuer,
               JwtokenOptions.Issuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}