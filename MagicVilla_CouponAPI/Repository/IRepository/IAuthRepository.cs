namespace MagicVilla_CouponAPI.Repository.IRepository;

public interface IAuthRepository
{
    bool IsUniqueUser(string username);
    Task<LoginResponseDTO> Login(LoginRequestDTO loginReqeustDTO);
    Task<UserDTO> Register(RegistrationRequestDTO requestDTO);
}
