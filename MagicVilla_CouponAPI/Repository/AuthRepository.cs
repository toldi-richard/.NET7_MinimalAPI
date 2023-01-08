using MagicVilla_CouponAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private string secretKey;
    public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
    }

    public bool IsUniqueUser(string username)
    {
        var user = _db.Users.SingleOrDefault(u => u.Name == username);
        if (user is null)
        {
            return true;
        }
        return false;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginReqeustDTO)
    {
        var user = _db.Users.SingleOrDefault(u => u.UserName == loginReqeustDTO.UserName && u.Password == loginReqeustDTO.Password);
        if (user is null)
        {
            return null;
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Role,user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        LoginResponseDTO loginResponseDTO = new()
        {
            User = _mapper.Map<UserDTO>(user),
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
        return loginResponseDTO;
    }

    public async Task<UserDTO> Register(RegistrationRequestDTO requestDTO)
    {
        // TODO: pass salt+hash
        var user = _mapper.Map<LocalUser>(requestDTO);
        _db.Users.Add(user);
        _db.SaveChanges();
        user.Password = "";
        return _mapper.Map<UserDTO>(user);
    }
}
