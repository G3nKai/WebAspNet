using backendRetake.Models;
using backendRetake.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backendRetake.Controllers
{
    [ApiController]
    public class UsersController : Controller
    {
        ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";
            bool isTeacher = await _context.CampusCourseUser.Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication)).AnyAsync();

            if (!isAdmin && !isTeacher)
            {
                return Forbid();
            }

            var users = await _context.User.ToListAsync();

            return Ok(users);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            UserRolesModel roles = new UserRolesModel()
            {
                isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin" ? true : false,
                isStudent = await _context.CampusCourseUser
                                .FirstOrDefaultAsync(u => u.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && 
                                u.Role == UserCampusCourseRole.Student) != null ? true : false,
                isTeacher = await _context.CampusCourseUser
                                .FirstOrDefaultAsync(u => u.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) 
                                && u.Role == UserCampusCourseRole.Teacher) != null ? true : false
            };

            return Ok(roles);
        }
    }
}
