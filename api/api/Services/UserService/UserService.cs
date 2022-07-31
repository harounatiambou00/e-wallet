using api.Services.JwtService;
using System.Security.Cryptography;

namespace api.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly DataContext _db;
        private readonly IJwtService _jwtService;
        public UserService(DataContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        public async Task<ServiceResponse<User?>> AddUser(UserRegisterDTO request)
        {
            ServiceResponse<User?> response;
            if(_db.Users.Any(u => u.Email == request.Email))
            {
                response = new ServiceResponse<User?>
                {
                    Data = null,
                    Success = false,
                    Message = "This email is already taken"
                };

               
            }
            else
            {
                CreatePasswordHash(request.Password,
                        out byte[] passwordHash,
                        out byte[] passwordSalt
                    );

                var VerificationToken = CreateRandomToken();
                while(await _db.Users.FirstOrDefaultAsync(u => u.VerificationToken == VerificationToken) != null)
                {
                    VerificationToken = CreateRandomToken();
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    VerificationToken = CreateRandomToken(),
                    Devise = request.Devise,
                    SigneUpDate = DateTime.Now,
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                response = new ServiceResponse<User?>
                {
                    Data = user,
                    Success = true,
                    Message = "USER SIGNED UP SUCCESSSFULLY"
                };
            }
             
            return response;
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        /**
         * This CreatePasswordHash takes the user's password and return it's hash and salt by using the HMACSHA512 algorithm.
         * **/
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        /**
         * This CreatePasswordHash takes the user's password and return it's hash and salt by using the HMACSHA512 algorithm.
         * **/
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                //We encode the password passed by the user that want to loging
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                //We return the result of the comparaison(sequence by sequence that's why we use the SequenceEgual method) between the computedHash and the passwordHash stored in the database
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public Task<ServiceResponse<List<User>>> DeleteUser(int id)
        {
            throw new NotImplementedException();
        } 

        public Task<ServiceResponse<List<User>>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<User>> GetUserByEmail(string email)
        {
            var response = new ServiceResponse<User>();
            response.Data = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (response.Data == default(User))
            {
                response.Data = null;
                response.Success = false;
                response.Message = "USER NOT FOUND";
            }

            return response;
        }

        public async Task<User?> GetUserById(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);

            return user;
        }


        public async Task<ServiceResponse<string?>> Login(UserLoginDTO request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            ServiceResponse<string?> response;
            if(user == null)
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "USER NOT FOUND"
                };
            }
            else if (user.VerifiedAt == null)
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "USER NOT VERIFIED"
                };
            }
            else if(!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "INCORRECT PASSWORD"
                };
            }
            else
            {
                string  userLoginJWT = _jwtService.GenerateToken(user.UserId, request.RemenberMe);
                response = new ServiceResponse<string?>
                {
                    Data = userLoginJWT,
                    Success = true,
                    Message = "USER SIGNED IN SUCCESSFULLY"
                };
            }

            return response;
        }

        public Task<ServiceResponse<List<User>>> UpdateUser(UpdateUserDTO request)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<string?>> VerifyEmail(String token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            ServiceResponse<string?> response;
            if (user == null)
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "INVALID TOKEN"
                };
            }
            else
            {
                user.VerifiedAt = DateTime.Now;
                await _db.SaveChangesAsync();
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = true,
                    Message = "USER VERIFIED SUCCESSFULLY"
                };
            }

            return response;
        }

        public async Task<ServiceResponse<string?>> ForgotPassword(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            ServiceResponse<string?> response;
            if (user == null)
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "USER NOT FOUND"
                };
            }
            else
            {
                var PasswordResetToken = CreateRandomToken();
                while (await _db.Users.FirstOrDefaultAsync(u => u.VerificationToken == PasswordResetToken) != null)
                {
                    PasswordResetToken = CreateRandomToken();
                }
                user.PasswordResetToken = PasswordResetToken;
                user.ResetTokenExpires = DateTime.Now.AddDays(1);
                await _db.SaveChangesAsync();
                response = new ServiceResponse<string?>
                {
                    Data = PasswordResetToken,
                    Success = true,
                    Message = "YOU CAN NOW RESET YOUR PASSWORD"
                };
            }

            return response;
        }

        public async Task<ServiceResponse<string?>> ResetPassword(ResetPasswordDTO request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            ServiceResponse<string?> response;
            if (user == null)
            {
                response = new ServiceResponse<string?>
                {
                    Data = null,
                    Success = false,
                    Message = "INVALID TOKEN"
                };
            }
            else
            {
                if(user.ResetTokenExpires < DateTime.Now)
                {
                    user.PasswordResetToken = null;
                    user.ResetTokenExpires = null;
                    await _db.SaveChangesAsync();
                    response = new ServiceResponse<string?>
                    {
                        Data = null,
                        Success = false,
                        Message = "THE TOKEN IS EXPIRED"
                    };
                }
                else
                {
                    CreatePasswordHash(request.NewPassword,
                        out byte[] passwordHash,
                        out byte[] passwordSalt
                    );
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.PasswordResetToken = null;
                    user.ResetTokenExpires = null;
                    await _db.SaveChangesAsync();
                    response = new ServiceResponse<string?>
                    {
                        Data = null,
                        Success = true,
                        Message = "PASSWORD RESET SUCCESSFULLY"
                    };
                }
            }

            return response;
        }
    }
}
