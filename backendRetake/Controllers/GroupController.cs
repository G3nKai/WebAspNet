using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendRetake.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using backendRetake.Services;
using System.ComponentModel.DataAnnotations;

namespace backendRetake.Controllers
{
    [Authorize]
    [ApiController]
    public class GroupController : Controller
    {
        ApplicationDbContext _context;
        public GroupController(ApplicationDbContext context) 
        {
            _context = context;
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            var groups = await _context.CampusGroup.ToListAsync();

            return Ok(groups);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup(CreateCampusGroupModel groupDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusGroupModel group = new CampusGroupModel
            {
                Id = Guid.NewGuid(),
                Name = groupDTO.Name
            };

            await _context.CampusGroup.AddAsync(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusGroupModel? group = await _context.CampusGroup.FirstOrDefaultAsync(p => p.Id == id);

            if (group == null)
            {
                Response response = new Response
                {
                    message = $"Campus group with id = {id} does not exist.",
                };
                return NotFound(response);
            }

            _context.CampusGroup.Remove(group);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("groups/{id}")]
        public async Task<IActionResult> PutGroup(Guid id, EditCampusGroupModel groupDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusGroupModel? group = await _context.CampusGroup.FirstOrDefaultAsync(p => p.Id == id);

            if (group == null)
            {
                Response response = new Response
                {
                    message = $"Campus group with id = {id} does not exist.",
                };
                return NotFound(response);
            }

            group.Name = groupDTO.Name;

            _context.CampusGroup.Update(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [HttpGet("groups/{id}")]//remaining students is incorrectly being counted
        public async Task<IActionResult> GetGroupCourses(Guid id)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusGroupModel? group = await _context.CampusGroup
                .Include(c => c.CampusCourses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (group == null)
            {
                Response response = new Response
                {
                    message = $"Campus group with id = {id} does not exist.",
                };
                return NotFound(response);
            }

            List<CampusCoursePreviewModel> courses = new List<CampusCoursePreviewModel>();

            foreach ( CampusCourseModel course in group.CampusCourses)
            {
                CampusCoursePreviewModel coursePreview = new CampusCoursePreviewModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    StartYear = course.StartYear,
                    MaximumStudentsCount = course.MaximumStudentsCount,
                    RemainingSlotsCount = course.RemainingSlotsCount,
                    Status = course.Status,
                    Semester = course.Semester
                };
                courses.Add(coursePreview);
            }

            return Ok(courses);
        }
    }
}
