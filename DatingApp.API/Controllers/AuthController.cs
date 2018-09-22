using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }
        [HttpPost("register")]
        public async Task<IActionResult> register(UserForRegisterDto userDto)
        {
            if (await _repo.UserExists(userDto.Username))
            {
                return BadRequest("Username already exists");
            }


            var UserToCreate = new User
            {
                Username = userDto.Username
            };

            var CreatedUser = await _repo.Register(UserToCreate, userDto.Password);

            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForlogin)
        {

            var userFromRepo = await _repo.Login(userForlogin.Username.ToLower(), userForlogin.Password);

            if (userFromRepo == null)
                return Unauthorized();
            //Our token is gonna contain two claims, userId and Username
            var claims = new[] {
                    new Claim (ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name,userFromRepo.Username)
                };
            
            //IN order to make sur that our tokens are valid tokens 
            // when it comes back to server needs to sign the token

            //Creating a security key,  and encrypting this key
            //
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            // we re using this key as part of the signing Credentials and encrypted this key with a hashing algorithm,
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            
            // we create the token, we start by creating a token descriptor, and we passing claims, we give it an expiry date
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler =  new JwtSecurityTokenHandler () ;

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(
                new {
                    token = tokenHandler.WriteToken(token)
                });

        }


    }
}