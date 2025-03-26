using backendRetake.Models;
using backendRetake.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.Linq;
using System.Security.Claims;

namespace backendRetake.Controllers
{
    [Authorize]
    [ApiController]
    public class CourseController : Controller
    {
        ApplicationDbContext _context;
        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("courses/{id}/details")]
        public async Task<IActionResult> GetDetailsCampusCourse(Guid id)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                .Include(c => c.CampusCourseUsers)
                    .ThenInclude(u => u.User)
                .Include(n => n.CampusCourseNotifications)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (course == null)
            {
                return NotFound();
            }

            CampusCourseDetailsModel courseDetails = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(student => student.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(student => student.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                //if user is not admin and they're a student - they can not see results of other students and non-accepted students
                //!!!results should be fixed in a way that student can see only they're own results!!!
                Students = course.CampusCourseUsers
                .Where(u => u.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && User.FindFirstValue(ClaimTypes.Role) == "User" && u.Role == UserCampusCourseRole.Student)
                .Any() ? course.CampusCourseUsers
                .Where(u => u.Role == UserCampusCourseRole.Student && u.Status == StudentStatuses.Accepted)
                .Select(u => new CampusCourseStudentModel()
                {
                    Id = u.User.Id,
                    Name = u.User.FullName,
                    Email = u.User.Email,
                    Status = u.Status,
                    MidtermResult = (u.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication)) ? u.MidtermResult : null,
                    FinalResult = (u.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication)) ? u.FinalResult : null,
                }).ToList() : course.CampusCourseUsers
                .Where(u => u.Role == UserCampusCourseRole.Student)
                .Select(u => new CampusCourseStudentModel()
                {
                    Id = u.User.Id,
                    Name = u.User.FullName,
                    Email = u.User.Email,
                    Status = u.Status,
                    MidtermResult = u.MidtermResult,
                    FinalResult = u.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers
                .Where(u => u.Role == UserCampusCourseRole.Teacher)
                .Select(u => new CampusCourseTeacherModel()
                {
                    Name = u.User.FullName,
                    Email = u.User.Email,
                    isMain = u.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications
                .Where(n => n.CampusCourseId == id)
                .Select(u => new CampusCourseNotificationModel()
                {
                    Text = u.Text,
                    IsImportant = u.IsImportant,
                }).ToList()
            };

            return Ok(courseDetails);
        }
        [HttpPost("courses/{id}/sign-up")]
        public async Task<IActionResult> SignUpCampusCourse(Guid id)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                .Include(c => c.CampusCourseUsers)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                Response responseNotFound = new Response()
                {
                    message = $"Campus course with id = {id} does not exist."
                };

                return NotFound(responseNotFound);
            }

            bool isTeacher = course.CampusCourseUsers.Any(user =>
                    user.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication));

            if (isTeacher)
            {
                Response responseIsTeacher = new Response()
                {
                    message = "You can not sign up to this course. You already are: either a teacher or a student."
                };

                return BadRequest(responseIsTeacher);
            }

            bool isClosed = course.Status != CourseStatuses.OpenForAssigning;

            if (isClosed)
            {
                Response responseIsClosed = new Response()
                {
                    message = "Campus course is not open for signing up."
                };

                return BadRequest(responseIsClosed);
            }

            bool isOverCrowded = course.RemainingSlotsCount == 0;

            if (isOverCrowded)
            {
                Response responseIsOverCrowded = new Response()
                {
                    message = "Campus course does not have any remaining slots."
                };

                return BadRequest(responseIsOverCrowded);
            }

            UserModel student = await _context.User.FirstAsync(user => user.Id.ToString() == User.FindFirstValue(ClaimTypes.Authentication));

            CampusCourseUser courseUser = new CampusCourseUser()
            {
                Id = Guid.NewGuid(),
                UserId = student.Id,
                User = student,
                CampusCourseId = course.Id,
                CampusCourse = course,
                Role = UserCampusCourseRole.Student,
                Status = StudentStatuses.InQueue,
                MidtermResult = StudentMarks.NotDefined,
                FinalResult = StudentMarks.NotDefined
            };

            await _context.CampusCourseUser.AddAsync(courseUser);

            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("courses/{id}/student-status/{studentId}")]
        public async Task<IActionResult> EditStudentStatus(Guid id, Guid studentId, EditCourseStudentStatusModel editStatusDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                                        .Include(p => p.CampusCourseUsers)
                                        .ThenInclude(p => p.User)
                                        .Include(p => p.CampusCourseNotifications)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (course == null)
            {
                Response responseNotFoundCourse = new Response
                {
                    message = $"Campus course with Id = {id} does not exist."
                };

                return BadRequest(responseNotFoundCourse);
            }

            UserModel? student = course.CampusCourseUsers
                .Where(p => p.UserId == studentId)
                .Select(p => p.User)
                .FirstOrDefault();

            if (student == null)
            {
                Response responseNotFoundStudent = new Response
                {
                    message = $"Student with Id = {studentId} does not exist."
                };

                return NotFound(responseNotFoundStudent);
            }

            bool isTeacher = course.CampusCourseUsers
                .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && p.Role == UserCampusCourseRole.Teacher)
                .Any();
            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";

            if (!isTeacher && !isAdmin)
            { 
                return Forbid();
            }

            bool isNotInQueue = student.CampusCourseUsers.Where(p => p.Status != StudentStatuses.InQueue).Any();

            if (isNotInQueue)
            {
                Response responseNotInQueue = new Response()
                {
                    message = "This student is not in queue. Their status cannot be changed."
                };

                return BadRequest(responseNotInQueue);
            }

            bool isRemainingNone = course.RemainingSlotsCount == 0 && editStatusDTO.Status == StudentStatuses.Accepted;

            if (isRemainingNone)
            {
                Response responseRemainingNone = new Response()
                {
                    message = "Can not be accepted - there is no remaining slots."
                };

                return BadRequest(responseRemainingNone);
            }

            CampusCourseUser campusCourseUser = student.CampusCourseUsers
                .Where(p => p.CampusCourseId == course.Id)
                .First();

            campusCourseUser.Status = editStatusDTO.Status;

            course.RemainingSlotsCount = campusCourseUser.Status == StudentStatuses.Accepted ?  course.RemainingSlotsCount - 1 : course.RemainingSlotsCount;

            _context.CampusCourseUser.Update(campusCourseUser);
            _context.CampusCourse.Update(course);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel response = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers
                .Where(p => p.Role == UserCampusCourseRole.Student)
                .Select(p => new CampusCourseStudentModel()
                {
                    Id = p.User.Id,
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    Status = (StudentStatuses)p.Status,
                    MidtermResult = (StudentMarks)p.MidtermResult,
                    FinalResult = (StudentMarks)p.FinalResult,
                }).ToList(),
                Teachers = course.CampusCourseUsers
                .Where(p => p.Role == UserCampusCourseRole.Teacher)
                .Select(p => new CampusCourseTeacherModel()
                {
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    isMain = (bool)p.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications
                .Select(p => new CampusCourseNotificationModel()
                {
                    Text = p.Text,
                    IsImportant = p.IsImportant
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPost("courses/{id}/status")]
        public async Task<IActionResult> UpdateCampusCourseStatus(Guid id, EditCourseStatusModel courseDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                .Include(c => c.CampusCourseUsers)
                    .ThenInclude(c => c.User)
                .Include(c => c.CampusCourseNotifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (course == null)
            {
                return NotFound($"Course with Id = {id} does not exist.");
            }

            if (!course.CampusCourseUsers
                .Any(role => role.Role == UserCampusCourseRole.Teacher && 
                role.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication)) && 
                User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Forbid();
            }

            if (courseDTO.Status < course.Status)
            {
                return BadRequest("Course status cannot be changed to a previous one.");
            }

            course.Status = courseDTO.Status;

            _context.CampusCourse.Update(course);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel courseResponse = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsInQueueCount = course.CampusCourseUsers.Where(student => student.Status == StudentStatuses.InQueue).Count(),
                StudentsEnrolledCount = course.CampusCourseUsers.Where(student => student.Status == StudentStatuses.Accepted).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers
                .Where(student => student.Role == UserCampusCourseRole.Student)
                .Select(student => new CampusCourseStudentModel()
                {
                    Id = student.User.Id,
                    Name = student.User.FullName,
                    Email = student.User.Email,
                    Status = (StudentStatuses)student.Status,
                    MidtermResult = (StudentMarks)student.MidtermResult,
                    FinalResult = (StudentMarks)student.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers
                .Where(teacher => teacher.Role == UserCampusCourseRole.Teacher)
                .Select(teacher => new CampusCourseTeacherModel()
                {
                    Name = teacher.User.FullName,
                    Email = teacher.User.Email,
                    isMain = (bool)teacher.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications
                .Where(notification => notification.CampusCourseId == id)
                .Select(notification => new CampusCourseNotificationModel()
                {
                    Text = notification.Text,
                    IsImportant = (bool)notification.IsImportant
                }).ToList()
            };

            return Ok(courseResponse);
        }
        [HttpPost("courses/{id}/notifications")]
        public async Task<IActionResult> CreateNotificationCampusCourse(Guid id, AddCampusCourseNotificationModel notificationDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                .Include(c => c.CampusCourseUsers)
                    .ThenInclude(c => c.User)
                .Include(c => c.CampusCourseNotifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (course == null)
            {
                Response responseNotFound = new Response()
                {
                    message = $"Course with Id = {id} does not exist."
                };
                return NotFound(responseNotFound);
            }

            bool isTeacher = course.CampusCourseUsers
                .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication))
                .Any();
            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";

            if (!isTeacher && !isAdmin)
            {
                return Forbid();
            }

            if (notificationDTO.Text.Length == 0 || notificationDTO.Text == null)
            {
                Response responseText = new Response()
                {
                    message = "Text field is required"
                };

                return BadRequest(responseText);
            }

            CampusCourseNotificationModel notification = new CampusCourseNotificationModel()
            {
                Id = Guid.NewGuid(),
                Text = notificationDTO.Text,
                IsImportant = notificationDTO.isImportant,
                CampusCourseId = course.Id,
                CampusCourse = course
            };

            course.CampusCourseNotifications.Add(notification);
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel response = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers
                .Where(p => p.Role == UserCampusCourseRole.Student)
                .Select(p => new CampusCourseStudentModel()
                {
                    Id = p.User.Id,
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    Status = (StudentStatuses)p.Status,
                    MidtermResult = (StudentMarks)p.MidtermResult,
                    FinalResult = (StudentMarks)p.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers
                .Where(p => p.Role == UserCampusCourseRole.Teacher)
                .Select(p => new CampusCourseTeacherModel()
                {
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    isMain = (bool)p.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications
                .Select(p => new CampusCourseNotificationModel()
                {
                    Text = p.Text,
                    IsImportant = p.IsImportant
                }).ToList()
            };

            return Ok(response);
        }
        [HttpPost("courses/{id}/marks/{studentId}")]
        public async Task<IActionResult> EditStudentMarkCampusCourse(Guid id, Guid studentId, EditCourseStudentMarkModel markDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                .Include(p => p.CampusCourseUsers)
                    .ThenInclude(p => p.User)
                .Include(p => p.CampusCourseNotifications)
                .Where(p => p.Id == id).FirstOrDefaultAsync();

            if (course == null)
            {
                Response responseNotFound = new Response()
                {
                    message = $"Campus course with Id = {id} does not exist."
                };

                return NotFound(responseNotFound);
            }

            UserModel? user = await _context.User.Where(p => p.Id == studentId).FirstOrDefaultAsync();

            if (user == null)
            {
                Response responseNotFound = new Response()
                {
                    message = $"User with Id = {studentId} does not exist."
                };

                return NotFound(responseNotFound);
            }

            bool isStudent = course.CampusCourseUsers.Where(p => p.UserId == user.Id && p.Role == UserCampusCourseRole.Student).Any();

            if (!isStudent)
            {
                Response responseNotStudent = new Response()
                {
                    message = $"User with Id = {studentId} is not a student."
                };

                return BadRequest(responseNotStudent);
            }

            //forbid case
            bool isTeacher = course.CampusCourseUsers
                .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && p.Role == UserCampusCourseRole.Teacher)
                .Any();
            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";

            if (!isAdmin && !isTeacher)
            {
                return Forbid();
            }

            CampusCourseUser campusCourseUser = course.CampusCourseUsers.Where(p => p.UserId == user.Id && p.CampusCourseId == id).First();

            if (markDTO.MarkType == MarkType.Midterm)
            {
                campusCourseUser.MidtermResult = markDTO.Mark;
            }
            else
            {
                campusCourseUser.FinalResult = markDTO.Mark;
            }

            _context.CampusCourseUser.Update(campusCourseUser);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel response = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Student).Select(p => new CampusCourseStudentModel()
                {
                    Id = p.User.Id,
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    Status = p.Status,
                    MidtermResult = p.MidtermResult,
                    FinalResult = p.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Teacher).Select(p => new CampusCourseTeacherModel()
                {
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    isMain = p.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications.Select(p => new CampusCourseNotificationModel() {
                    Text = p.Text,
                    IsImportant = p.IsImportant
                }).ToList()
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("groups/{groupId}")]
        public async Task<IActionResult> CreateCampusCourse(Guid groupId, CreateCampusCourseModel campusCourseDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusGroupModel? campusGroup = await _context.CampusGroup.FirstOrDefaultAsync(c => c.Id == groupId);

            if (campusGroup == null)
            {
                return NotFound();
            }

            UserModel? teacher = await _context.User
                .FirstOrDefaultAsync(t => t.Id == campusCourseDTO.mainTeacherId);

            if (teacher == null)
            {
                Response response = new Response()
                {
                    message = $"User with id = {campusCourseDTO.mainTeacherId} does not exist."
                };

                return NotFound(response);
            }

            CampusCourseModel campusCourse = new CampusCourseModel()
            {
                Id = Guid.NewGuid(),
                Name = campusCourseDTO.Name,
                StartYear = campusCourseDTO.StartYear,
                MaximumStudentsCount = campusCourseDTO.MaximumStudentsCount,
                RemainingSlotsCount = campusCourseDTO.MaximumStudentsCount,
                Status = CourseStatuses.Created,
                Semester = campusCourseDTO.Semester,
                Requirements = campusCourseDTO.Requirements,
                Annotations = campusCourseDTO.Annotation,
                MainTeacherId = teacher.Id,
                CampusGroupId = groupId,
                CampusGroup = campusGroup,
                CreationTime = DateTime.UtcNow
            };

            CampusCourseUser campusCourseUser = new CampusCourseUser()
            {
                Id = Guid.NewGuid(),
                UserId = teacher.Id,
                CampusCourseId = campusCourse.Id,
                Role = UserCampusCourseRole.Teacher,
                isMain = true
            };

            await _context.CampusCourse.AddAsync(campusCourse);
            await _context.CampusCourseUser.AddAsync(campusCourseUser);
            await _context.SaveChangesAsync();

            List<CampusCoursePreviewModel> courses = new List<CampusCoursePreviewModel>();

            foreach(CampusCourseModel course in _context.CampusCourse.Where(c => c.CampusGroupId == groupId))
            {
                CampusCoursePreviewModel courseDTO = new CampusCoursePreviewModel()
                {
                    Id = course.Id,
                    Name = course.Name,
                    StartYear = course.StartYear,
                    MaximumStudentsCount = course.MaximumStudentsCount,
                    RemainingSlotsCount = course.RemainingSlotsCount,
                    Status = course.Status,
                    Semester = course.Semester
                };

                courses.Add(courseDTO);
            }

            return Ok(courses);
        }
        [HttpPut("courses/{id}/requrements-and-annotations")]
        public async Task<IActionResult> EditCampusCourseAnnotationsAndRequrements(Guid id, EditCampusCourseRequirementsAndAnnotationsModel courseDTO)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            
            CampusCourseModel? course = await _context.CampusCourse
                                            .Where(p => p.Id == id)
                                            .Include(p => p.CampusCourseUsers)
                                                .ThenInclude(p => p.User)
                                            .Include(p => p.CampusCourseNotifications)
                                            .FirstOrDefaultAsync();

            //Not Found course
            if (course == null)
            {
                Response responseNotFound = new Response
                {
                    message = $"Campus course with Id = {id} does not exist."
                };

                return NotFound(responseNotFound);
            }

            //Forbid
            bool isTeacher = course.CampusCourseUsers
                    .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && p.Role == UserCampusCourseRole.Teacher)
                    .Any();
            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";

            if (!isTeacher && !isAdmin)
            {
                return Forbid();
            }
            
            course.Requirements = courseDTO.Requirements;
            course.Annotations = courseDTO.Annotations;

            _context.CampusCourse.Update(course);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel response = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Student).Select(p => new CampusCourseStudentModel()
                {
                    Id = p.User.Id,
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    Status = p.Status,
                    MidtermResult = p.MidtermResult,
                    FinalResult = p.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Teacher).Select(p => new CampusCourseTeacherModel()
                {
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    isMain = p.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications.Select(p => new CampusCourseNotificationModel()
                {
                    Text = p.Text,
                    IsImportant = p.IsImportant
                }).ToList()
            };

            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> EditCampusCourse(Guid id, EditCampusCourseModel courseDTO)
        {
            //consider case when Admin can decrease maximum students count - but not below than already enrolled students on course
            //main teacher should not be the student of this course - if he is the teacher, we should swap isMain field between them
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse
                                            .Where(p => p.Id == id)
                                            .Include(p => p.CampusCourseUsers)
                                                .ThenInclude(p => p.User)
                                            .Include(p => p.CampusCourseNotifications)
                                            .FirstOrDefaultAsync();

            //Not Found course
            if (course == null)
            {
                Response responseNotFound = new Response
                {
                    message = $"Campus course with Id = {id} does not exist."
                };

                return NotFound(responseNotFound);
            }

            //Teacher cases
            UserModel? userSearch = await _context.User.Where(p => p.Id == courseDTO.MainTeacherId).FirstOrDefaultAsync();

            //doesn't exist
            if (userSearch == null) 
            {
                Response responseBad = new Response()
                {
                    message = $"User with Id = {courseDTO.MainTeacherId} does not exist."
                };

                return BadRequest(responseBad);
            }

            //this teacher is already ARE main teacher
            CampusCourseUser? userSearchMain = course.CampusCourseUsers.Where(p => p.UserId == courseDTO.MainTeacherId && p.isMain == true).FirstOrDefault();

            if (userSearchMain != null)
            {
                course.MainTeacherId = courseDTO.MainTeacherId;//doesn't need to be updated
            }

            //not main but teacher
            CampusCourseUser? userSearchJustTeacher = course.CampusCourseUsers
                                                    .Where(p => p.UserId == courseDTO.MainTeacherId && p.Role == UserCampusCourseRole.Teacher)
                                                    .FirstOrDefault();
            if (userSearchJustTeacher != null)
            {
                CampusCourseUser teacherMainChange = course.CampusCourseUsers.Where(p => p.isMain == true).First();
                teacherMainChange.isMain = false;
                userSearchJustTeacher.isMain = true;
                course.MainTeacherId = courseDTO.MainTeacherId;

                _context.CampusCourseUser.UpdateRange(new[] {teacherMainChange, userSearchJustTeacher });
            }

            //bad request - a student
            CampusCourseUser? userSearchStudent = course.CampusCourseUsers
                                                        .Where(p => p.UserId == courseDTO.MainTeacherId && p.Role == UserCampusCourseRole.Student)
                                                        .FirstOrDefault();
            if (userSearchStudent != null)
            {
                Response responseBad = new Response()
                {
                    message = $"User with Id = {courseDTO.MainTeacherId} is a student of this course and can not become the main teacher."
                };

                return BadRequest(responseBad);
            }

            //user is no teacher but not a student - should be added
            CampusCourseUser? userSearchNew = course.CampusCourseUsers
                                                    .Where(p => p.UserId == courseDTO.MainTeacherId)
                                                    .FirstOrDefault();
            if (userSearchNew == null)
            {
                userSearchNew = new CampusCourseUser()
                {
                    Id = Guid.NewGuid(),
                    UserId = userSearch.Id,
                    User = userSearch,
                    CampusCourseId = course.Id,
                    CampusCourse = course,
                    Role = UserCampusCourseRole.Teacher,
                    isMain = true
                };//change the other teacher to isMain = false

                CampusCourseUser campusCourseMainTeacher = course.CampusCourseUsers.Where(p => p.isMain == true).First();
                campusCourseMainTeacher.isMain = false;

                course.MainTeacherId = courseDTO.MainTeacherId;

                //changes in DB

                _context.CampusCourseUser.Update(campusCourseMainTeacher);
                _context.CampusCourseUser.Add(userSearchNew);
            }
            

            //Divergence of students in course
            if (courseDTO.MaximumStudentsCount < course.MaximumStudentsCount - course.RemainingSlotsCount)
            {
                Response responseBad = new Response()
                {
                    message = "Maximum students count can not be lower than amount of already enrolled students."
                };

                return BadRequest(responseBad);
            }

            course.Name = courseDTO.Name;
            course.StartYear = courseDTO.StartYear;
            course.MaximumStudentsCount = courseDTO.MaximumStudentsCount;
            course.RemainingSlotsCount = course.MaximumStudentsCount - course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count();
            course.Semester = courseDTO.Semester;
            course.Requirements = courseDTO.Requirements;
            course.Annotations = courseDTO.Annotations;

            _context.CampusCourse.Update(course);
            await _context.SaveChangesAsync();

            CampusCourseDetailsModel response = new CampusCourseDetailsModel()
            {
                Id = course.Id,
                Name = course.Name,
                StartYear = course.StartYear,
                MaximumStudentsCount = course.MaximumStudentsCount,
                StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                Requirements = course.Requirements,
                Annotations = course.Annotations,
                Status = course.Status,
                Semester = course.Semester,
                Students = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Student).Select(p => new CampusCourseStudentModel()
                {
                    Id = p.User.Id,
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    Status = p.Status,
                    MidtermResult = p.MidtermResult,
                    FinalResult = p.FinalResult
                }).ToList(),
                Teachers = course.CampusCourseUsers.Where(p => p.Role == UserCampusCourseRole.Teacher).Select(p => new CampusCourseTeacherModel()
                {
                    Name = p.User.FullName,
                    Email = p.User.Email,
                    isMain = p.isMain
                }).ToList(),
                Notifications = course.CampusCourseNotifications.Select(p => new CampusCourseNotificationModel()
                {
                    Text = p.Text,
                    IsImportant = p.IsImportant
                }).ToList()
            };

            return Ok(response);
        }
        [HttpPost("courses/{id}/teachers")]
        public async Task<IActionResult> AddTeacherToCampusCourse(Guid id, AddTeacherToCourseModel teacher)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseUser? campusCourseUser = await _context.CampusCourseUser
                                                    .Where(p => p.CampusCourseId == id && p.isMain == true)
                                                    .FirstOrDefaultAsync();

            if (campusCourseUser == null)
            {
                Response responseNotFound = new Response()
                {
                    message = $"Campus course with Id = {id} does not exist."
                };

                return NotFound(responseNotFound);
            }

            bool isUserExists = await _context.User.Where(p => p.Id == teacher.UserId).AnyAsync();

            if (!isUserExists)
            {
                Response responseExists = new Response()
                {
                    message = $"User with Id = {teacher.UserId} does not exist."
                };

                return NotFound(responseExists);
            }

            bool isAlreadyTeacher = await _context.CampusCourseUser
                                        .Where(p => p.UserId == teacher.UserId && p.Role == UserCampusCourseRole.Teacher && p.CampusCourseId == id)
                                        .AnyAsync();

            if (isAlreadyTeacher)
            {
                Response responseExists = new Response()
                {
                    message = $"User with Id = {teacher.UserId} is a teacher of this course already."
                };

                return BadRequest(responseExists);
            }

            bool isAlreadyStudent = await _context.CampusCourseUser
                                            .Where(p => p.UserId == teacher.UserId && p.Role == UserCampusCourseRole.Student && p.CampusCourseId == id)
                                            .AnyAsync();

            if (isAlreadyStudent)
            {
                Response responseExists = new Response()
                {
                    message = $"User with Id = {teacher.UserId} is a student of this course already."
                };

                return BadRequest(responseExists);
            }


            bool isAdmin = User.FindFirstValue(ClaimTypes.Role) == "Admin";
            bool isMainTeacher = campusCourseUser.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication);

            if (!isAdmin && !isMainTeacher)
            {
                return Forbid();
            } 
            else
            {
                CampusCourseModel course = await _context.CampusCourse.Where(p => p.Id == id)
                                                .Include(u => u.CampusCourseUsers)
                                                    .ThenInclude(u => u.User)
                                                .Include(u => u.CampusCourseNotifications)
                                                .FirstAsync();

                CampusCourseUser newTeacher = new CampusCourseUser()
                {
                    Id = Guid.NewGuid(),
                    UserId = teacher.UserId,
                    User = await _context.User.Where(p => p.Id == teacher.UserId).FirstAsync(),
                    CampusCourseId = id,
                    CampusCourse = course,
                    Role = UserCampusCourseRole.Teacher,
                    isMain = false
                };

                await _context.CampusCourseUser.AddAsync(newTeacher);
                await _context.SaveChangesAsync();

                CampusCourseDetailsModel response = new CampusCourseDetailsModel()
                {
                    Id = course.Id,
                    Name = course.Name,
                    StartYear = course.StartYear,
                    MaximumStudentsCount = course.MaximumStudentsCount,
                    StudentsEnrolledCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.Accepted).Count(),
                    StudentsInQueueCount = course.CampusCourseUsers.Where(p => p.Status == StudentStatuses.InQueue).Count(),
                    Requirements = course.Requirements,
                    Annotations = course.Annotations,
                    Status = course.Status,
                    Semester = course.Semester,
                    Students = course.CampusCourseUsers
                                        .Where(p => p.Role == UserCampusCourseRole.Student)
                                        .Select(student => new CampusCourseStudentModel()
                    {
                        Id = student.User.Id,
                        Name = student.User.FullName,
                        Email = student.User.Email,
                        Status = student.Status,
                        MidtermResult = student.MidtermResult,
                        FinalResult = student.FinalResult,
                    }).ToList(),
                    Teachers = course.CampusCourseUsers
                    .Where(p => p.Role == UserCampusCourseRole.Teacher)
                    .Select(teach => new CampusCourseTeacherModel()
                    {
                        Name = teach.User.FullName,
                        Email = teach.User.Email,
                        isMain = teach.isMain
                    }).ToList(),
                    Notifications = course.CampusCourseNotifications
                                            .Select(notification => new CampusCourseNotificationModel()
                    {
                        Text = notification.Text,
                        IsImportant = notification.IsImportant
                    }).ToList()
                };

                return Ok(response);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCampusCourse(Guid id)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            CampusCourseModel? course = await _context.CampusCourse.FirstOrDefaultAsync(p => p.Id == id);

            if (course == null)
            {
                return NotFound($"Course with Id = {id} does not exist.");
            }

            Response response = new Response()
            {
                status = null,
                message = "Course was successfully deleted."
            };

            _context.CampusCourse.Remove(course);
            await _context.SaveChangesAsync();

            return Ok(response);
        }
        [HttpGet("courses/my")]
        public async Task<IActionResult> GetCampusCourseMy()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            List<CampusCoursePreviewModel> courses = _context.CampusCourseUser
                .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && p.Role == UserCampusCourseRole.Student)
                .Select(p => new CampusCoursePreviewModel()
            {
                Id = p.CampusCourseId,
                Name = p.CampusCourse.Name,
                StartYear = p.CampusCourse.StartYear,
                MaximumStudentsCount = p.CampusCourse.MaximumStudentsCount,
                RemainingSlotsCount = p.CampusCourse.RemainingSlotsCount,
                Status = p.CampusCourse.Status,
                Semester = p.CampusCourse.Semester
            }).ToList();

            return Ok(courses);
        }
        [HttpGet("courses/teaching")]
        public async Task<IActionResult> GetCampusCourseTeaching()
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            List<CampusCoursePreviewModel> courses = _context.CampusCourseUser
                .Where(p => p.UserId.ToString() == User.FindFirstValue(ClaimTypes.Authentication) && p.Role == UserCampusCourseRole.Teacher)
                .Select(p => new CampusCoursePreviewModel()
                {
                    Id = p.CampusCourseId,
                    Name = p.CampusCourse.Name,
                    StartYear = p.CampusCourse.StartYear,
                    MaximumStudentsCount = p.CampusCourse.MaximumStudentsCount,
                    RemainingSlotsCount = p.CampusCourse.RemainingSlotsCount,
                    Status = p.CampusCourse.Status,
                    Semester = p.CampusCourse.Semester
                }).ToList();

            return Ok(courses);
        }
        [AllowAnonymous]
        [HttpGet("courses/list")]
        public async Task<IActionResult> GetCampusCourseList(SortList? sort, string? search, bool? hasPlacesAndOpen, Semesters? semester, int page = 1, int pageSize = 10)
        {
            LogoutToken logout = new LogoutToken { Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "") };

            if (await TokenService.CheckToken(logout, _context))
            {
                return Unauthorized();
            }

            if (page < 1)
            {
                Response responseBad = new Response()
                {
                    message = $"An invalid value for page = {page}"
                };

                return BadRequest(responseBad);
            }

            if (pageSize < 1)
            {
                Response responseBad = new Response()
                {
                    message = $"An invalid value for page size = {pageSize}"
                };

                return BadRequest(responseBad);
            }

            IQueryable<CampusCourseModel> courses = _context.CampusCourse;

            if (search != null)
            {
                courses = from course in courses
                          where course.Name.Contains(search)
                          select course;
            }

            switch (hasPlacesAndOpen)
            {
                case true:
                    courses = from course in courses
                              where course.Status == CourseStatuses.OpenForAssigning && course.RemainingSlotsCount > 0
                              select course;
                    break;
                case false:
                    courses = from course in courses
                              where course.Status != CourseStatuses.OpenForAssigning || course.RemainingSlotsCount <= 0
                              select course;
                    break;
            }

            if (semester != null)
            {
                courses = from course in courses
                          where course.Semester == semester
                          select course;
            }

            if (sort != null)
            {
                switch (sort)
                {
                    case SortList.CreatedAsc:
                        courses = from course in courses
                                  orderby course.CreationTime ascending
                                  select course;
                        break;
                    case SortList.CreatedDesc:
                        courses = from course in courses
                                  orderby course.CreationTime descending
                                  select course;
                        break;

                }
            }

            List<CampusCoursePreviewModel> response = await courses
                                                    .Skip((page - 1) * pageSize)
                                                    .Take(pageSize)
                                                    .Select(p => new CampusCoursePreviewModel()
            {
                Id = p.Id,
                Name = p.Name,
                StartYear = p.StartYear,
                MaximumStudentsCount = p.MaximumStudentsCount,
                RemainingSlotsCount = p.RemainingSlotsCount,
                Status = p.Status,
                Semester = p.Semester
            }).ToListAsync();

            return Ok(response);
        }
    }
}
