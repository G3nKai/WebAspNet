using backendRetake.Models;
using backendRetake.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backendRetake.Controllers
{
    [ApiController]
    public class AccountController : Controller
    {
        ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> CreateUser(UserRegisterModel userDTO)
        {
            UserModel? userFound = await _context.User.FirstOrDefaultAsync(p => p.Email == userDTO.Email);

            if (userFound != null)
            {
                Response conflictResponse = new Response
                {
                    message = "User with this email is already registered."
                };
                return Conflict(conflictResponse);
            }

            if (userDTO.Password != userDTO.ConfirmPassword)
            {
                return BadRequest("Passwords must be identical.");
            }

            if (!Regex.IsMatch(userDTO.Password, @"\d"))
            {
                return BadRequest("Password requires at least one digit.");
            }

                if (userDTO.BirthDate > DateTime.UtcNow)
            {
                return BadRequest("Birth date cannot be later than today.");
            }

            if (userDTO.FullName.Length == 0 || userDTO.FullName == null)
            {
                return BadRequest("Full name is required.");
            }


            UserModel user = new UserModel
            {
                Id = Guid.NewGuid(),
                Email = userDTO.Email,
                FullName = userDTO.FullName,
                BirthDate = userDTO.BirthDate,
                Password = userDTO.Password
            };

            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            TokenResponse response = new TokenResponse
            {
                Token = TokenService.GetClaims(user)
            };

            return Ok(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(UserLoginModel userDTO)
        {
            UserModel? user = await _context.User.FirstOrDefaultAsync(p => p.Email == userDTO.Email && p.Password == userDTO.Password);

            if (user == null)
            {
                
                Response responseNotFound = new Response
                {
                    status = null,
                    message = "Login failed."
                };
                return NotFound(responseNotFound);
            }

            TokenResponse response = new TokenResponse
            {
                Token = TokenService.GetClaims(user)
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            await _context.TokenBlackListed.AddAsync(logout);
            await _context.SaveChangesAsync();

            Response response = new Response
            {
                message = "Logged Out."
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfileUser()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }
            string userId = User.FindFirstValue(ClaimTypes.Authentication);
            UserModel user = await _context.User.FirstOrDefaultAsync(p => p.Id.ToString() == userId);

            UserProfileModel userDTO = new UserProfileModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate
            };

            return Ok(userDTO);
        }
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> PutProfileUser(EditUserProfileModel userDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            string userId = User.FindFirstValue(ClaimTypes.Authentication);

            UserModel user = await _context.User.FirstOrDefaultAsync(p => p.Id.ToString() == userId);

            user.FullName = userDTO.FullName;
            user.BirthDate = userDTO.BirthDate;

            _context.User.Update(user);
            await _context.SaveChangesAsync();

            UserProfileModel userResponse = new UserProfileModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate
            };

            return Ok(userResponse);
        }
    }
}
