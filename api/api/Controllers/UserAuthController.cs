using api.Services.JwtService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/user-auth")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        public UserAuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        } 

        [HttpPost("sign-up")]
        public async Task<ActionResult<ServiceResponse<User?>>> SignUp(UserRegisterDTO request)
        {
            var serviceResponse = await _userService.AddUser(request);

            return Ok(serviceResponse);
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<ServiceResponse<string?>>> SignIn(UserLoginDTO request)
        {
            //We will try to log the User
            var login = await _userService.Login(request);

            //if the User is logged succesfully, we create a cookie that contains the token of the login
            if (login.Success)
            {
                Response.Cookies.Append("userLoginJWT", login.Data, new CookieOptions
                {
                    HttpOnly = true //This means that the frontend can only get it but cannot access/modify it. 
                });
            }

            /*
             * Finally we will return a service response to inform if the login was successful or not(if not why)
             * We return a Service Response with a null data because we don't want the frontend to access the token.
            */
            return new ServiceResponse<string?>
            {
                Data = null,
                Success = login.Success,
                Message = login.Message,
            };
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<ServiceResponse<string?>>> VerifyEmail(String token)
        {
            var verification = await _userService.VerifyEmail(token);
            return verification;
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ServiceResponse<string?>>> ForgotPassword(String email)
        {
            return await _userService.ForgotPassword(email);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ServiceResponse<string?>>> ResetPassword(ResetPasswordDTO request)
        {
            return await _userService.ResetPassword(request);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<User>>> Get()
        {
            try
            {
                var userLoginJwtFromCookies = Request.Cookies["UserLoginJWT"];

                var validatedUserLoginJwt = _jwtService.Verify(userLoginJwtFromCookies);

                int userId = int.Parse(validatedUserLoginJwt.Issuer);

                var user = await _userService.GetUserById(userId);

                ServiceResponse<User?> response;

                if (user == null)
                {
                    response = new ServiceResponse<User?>
                    {
                        Data = null,
                        Success = false,
                        Message = "USER NOT FOUND"
                    };
                }
                else
                {
                    response = new ServiceResponse<User?>
                    {
                        Data = user,
                        Success = false,
                        Message = "USER FOUND"
                    };
                }
                return response;
            }
            catch (Exception _)
            {
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public ActionResult<ServiceResponse<User>> Logout()
        {
            if (Request.Cookies["userLoginJWT"] != null)
            {
                Response.Cookies.Delete("userLoginJWT");
                return (new ServiceResponse<User>
                {
                    Data = null,
                    Success = true,
                    Message = "USER LOGGED OUT SUCCESSSFULLY"
                });
            }
            else
            {
                return (new ServiceResponse<User>
                {
                    Data = null,
                    Success = false,
                    Message = "INVALID TOKEN"
                });
            }
        }
    }
}
