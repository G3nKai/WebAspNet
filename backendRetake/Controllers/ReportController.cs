using backendRetake.Models;
using backendRetake.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backendRetake.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(Semesters? semester, Guid[] campusGroupIds)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            

            //Forbid - student&&not main teacher&&everyone except admin???
            IQueryable<CampusCourseUser> campusCourseUsers = _context.CampusCourseUser
                                                                           .Include(p => p.User)
                                                                           .Include(p => p.CampusCourse)
                                                                                .ThenInclude(p => p.CampusGroup)
                                                                            .Where(p => p.CampusCourse.Status == CourseStatuses.Finished)
                                                                            .AsQueryable();


            if (semester != null)
            {
                campusCourseUsers = from p in campusCourseUsers where p.CampusCourse.Semester == semester select p;
            }

            if (campusGroupIds.Length > 0)
            {
                campusCourseUsers = from p in campusCourseUsers where campusGroupIds.Contains(p.CampusCourse.CampusGroupId) select p;
            }

            //group by unique user
            List < TeacherReportRecordModel > response = (from userTemp in campusCourseUsers
                                                          where userTemp.isMain == true
                                                          group userTemp by userTemp.UserId into user
                                                          select new TeacherReportRecordModel()
                                                          {
                                                              FullName = user.First().User.FullName,
                                                              Id = user.Key,//group by unique group
                                                              CampusGroupReports = (from temp in campusCourseUsers
                                                                                    where temp.CampusCourse.MainTeacherId == user.Key
                                                                                    group temp by temp.CampusCourse.CampusGroupId into g
                                                                                    select new CampusGroupReportModel()
                                                                                    {
                                                                                        Name = g.First().CampusCourse.CampusGroup.Name,
                                                                                        Id = g.Key,
                                                                                        AveragePassed = g.Where(passed => passed.FinalResult == StudentMarks.Passed).Count(),
                                                                                        AverageFailed = g.Where(passed => passed.FinalResult == StudentMarks.Failed).Count(),
                                                                                    }).ToList()
                                                          }).ToList();

            return Ok(response);
        }
    }
}
