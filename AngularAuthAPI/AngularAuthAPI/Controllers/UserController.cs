using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using AngularAuthAPI.Models.Dto;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;

        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserLogin userObj)
        {
            if(userObj == null) 
                return BadRequest();

            var user = await _authContext.Users.
                FirstOrDefaultAsync( x => x.Email == userObj.Email);

            if(user == null)
                return NotFound(new {Message = "User not found!"});

            if(!PasswordHasher.VerifyPassword(userObj.Password,user.Password))
            {
                return BadRequest(new { Message = "Incorrect password"});
            }

            user.Token = CreateJWT(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegister userRegisterObj)
        {
            if (userRegisterObj == null)
                return BadRequest();
            //Check Email
            if (await CheckEmailExistAsync(userRegisterObj.Email))
                return BadRequest(new { Message = "Email already exists!" });


            var userObj = new User
            {
                Nume = userRegisterObj.Nume,
                Email = userRegisterObj.Email,
                Password = PasswordHasher.HashPassword(userRegisterObj.Password),
                Facultate = userRegisterObj.Facultate,
                Specializare = userRegisterObj.Specializare,
                Role = userRegisterObj.Role,
                Token = ""
            };

            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered!"
            });
        }

        private Task<bool> CheckEmailExistAsync(string email)
            => _authContext.Users.AnyAsync( x => x.Email == email);

        private string CreateJWT(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-very-long-secret-key-here-1234567890");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.Email}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users.Any(a=>a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("your-very-long-secret-key-here-1234567890");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token,tokenValidationParameters,out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) 
                throw new SecurityTokenException("The token is invalid!");
            return principal;
            
        }

        
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto == null)
                return BadRequest("Invalid client request");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto?.RefreshToken;
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var email = principal.Identity.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid request");
            var newAccessToken = CreateJWT(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }
    }
}
