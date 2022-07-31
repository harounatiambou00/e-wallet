namespace api.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<List<User>>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task<ServiceResponse<User?>> GetUserByEmail(string email);
        Task<ServiceResponse<User?>> AddUser(UserRegisterDTO request);
        Task<ServiceResponse<List<User>>> UpdateUser(UpdateUserDTO request);
        Task<ServiceResponse<List<User>>> DeleteUser(int id);
        Task<ServiceResponse<String?>> Login(UserLoginDTO request);
        Task<ServiceResponse<String?>> VerifyEmail(String token);
        Task<ServiceResponse<String?>> ForgotPassword(String email);
        Task<ServiceResponse<String?>> ResetPassword(ResetPasswordDTO request);
    }
}
