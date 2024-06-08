using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class DashBoardService : BaseService<DashBoardService>, IDashboardService
    {
        public DashBoardService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<DashBoardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor, configuration)
        {
        }

        public async Task<List<DashboardRegisterResponse>> GetDashboardRegisterResponses(string quarter, string? courseId)
        {
            string[] parts = quarter.Split('-');
            var qua = parts[0];
            var year = parts[1];
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            var yearNumber = int.Parse(year);
            if (int.Parse(qua) == 1)
            {
                startDate = new DateTime(yearNumber, 1, 1);
                endDate = new DateTime(yearNumber, 3, 31);
            }
            if (int.Parse(qua) == 2)
            {
                startDate = new DateTime(yearNumber, 4, 1);
                endDate = new DateTime(yearNumber, 6, 30);
            }
            if (int.Parse(qua) == 3)
            {
                startDate = new DateTime(yearNumber, 7, 1);
                endDate = new DateTime(yearNumber, 9, 30);
            }
            if (int.Parse(qua) == 4)
            {
                startDate = new DateTime(yearNumber, 10, 1);
                endDate = new DateTime(yearNumber, 12, 31);
            }
            var startAnchor = startDate;
            List<DashboardRegisterResponse> responses = new List<DashboardRegisterResponse>();

            while (startAnchor <= endDate)
            {
                var endAnchor = startAnchor.AddDays(7);
                if (endAnchor.Date > endDate.Date)
                {
                    endAnchor = endDate;
                }
                var listStudentClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.AddedTime.Value.Date >= startAnchor.Date && x.AddedTime.Value.Date <= endDate.Date);
                if (courseId != null)
                {
                    var classIds = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId.ToString()), selector: x => x.Id);
                    listStudentClass = listStudentClass.Where(x => classIds.Any(p => p == x.ClassId)).ToList();
                }
                var response = new DashboardRegisterResponse
                {
                    NumberOfRegisters = listStudentClass.Count,
                    Date = startAnchor.Date.Day + "/" + startAnchor.Date.Month + "-" + endAnchor.Date.Day + "/" + endAnchor.Date.Month,
                    StartDateIn = startAnchor,
                };
                startAnchor = endAnchor.AddDays(1);
                responses.Add(response);
            }
            return responses.OrderBy(x => x.StartDateIn).ToList();
        }

        public async Task<List<FavoriteCourseResponse>> GetFavoriteCourseResponse(DateTime? startDate, DateTime? endDate)
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
            List<FavoriteCourseResponse> responses = new List<FavoriteCourseResponse>();
            foreach (var course in courses)
            {
                var numberOfclass = 0;
                var numberOfStudent = 0;
                var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Status.Equals("UPCOMING") && x.CourseId == course.Id);
                if (classes == null || classes.Count == 0)
                {
                    numberOfclass = 0;
                }
                else
                {
                    numberOfclass = classes.Count;
                    foreach (var c in classes)
                    {
                        var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id && x.AddedTime >= startDate && x.AddedTime <= endDate);
                        if (students != null)
                        {
                            numberOfStudent = numberOfStudent + students.Count;
                        }
                    }
                }
                responses.Add(new FavoriteCourseResponse
                {
                    Id = course.Id,
                    CourseName = course.Name,
                    NumberClassUpComing = numberOfclass,
                    NumberStudentsRegister = numberOfStudent,
                    Subject = course.Syllabus.SyllabusCategory.Name,
                    SubjectName = course.Syllabus.SubjectCode,
                }
                );
            }
            responses = responses.Where(x => x.NumberStudentsRegister > 0).OrderByDescending(x => x.NumberStudentsRegister).ToList();
            return responses;
        }

        public async Task<NumberOfMemberResponse> GetOfMemberResponse()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role));
            var parents = users.Where(x => x.Role.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()));
            var staff = users.Where(x => x.Role.Name.Equals(RoleEnum.STAFF.GetDescriptionFromEnum<RoleEnum>()) || x.Role.Name.Equals(RoleEnum.ADMIN.GetDescriptionFromEnum<RoleEnum>()) || x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync();
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Status.ToLower().Equals("upcoming"));
            return new NumberOfMemberResponse
            {
                NumOfChildrens = students.Count,
                NumOfCurrentClasses = classes.Count,
                NumOfParents = parents.Count(),
                NumOfStaffs = staff.Count(),
            };
        }

        public async Task<List<RevenueDashBoardResponse>> GetRevenueDashBoardResponse(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate.Value.AddHours(7);
            var wallettransaction = await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync(predicate: x => x.CreateTime >= startDate && x.CreateTime <= endDate);
            var groupwallet = wallettransaction.GroupBy(x => new { x.CreateTime, x.Method }).Select(g => new
            {
                CreateTime = g.Key.CreateTime,
                Method = g.Key.Method,
                Revenue = g.Sum(x => x.Money),
                DateIn = g.Key.CreateTime,
            });
            List<RevenueDashBoardResponse> revenueDashBoardResponses = new List<RevenueDashBoardResponse>();
            foreach (var group in groupwallet)
            {
                var date = group.CreateTime;
                var day = date.Day;
                var month = date.Month;
                var revenueRes = new RevenueDashBoardResponse
                {
                    Method = group.Method,
                    Date = day + "/" + month,
                    Revenue = group.Revenue,
                    DateIn = date,
                };
                if (group.Method.Equals("SystemWallet"))
                {
                    revenueRes.Method = "Ví";
                    revenueDashBoardResponses.Add(revenueRes);
                }
                if (group.Method.Equals("DirectionTransaction"))
                {
                    revenueRes.Method = "Trực Tiếp";
                    revenueDashBoardResponses.Add(revenueRes);
                }
            }
            var groupedTransactions = revenueDashBoardResponses
            .GroupBy(t => new { t.Date, t.Method })
            .Select(g => new RevenueDashBoardResponse
            {
                Date = g.Key.Date,
                Method = g.Key.Method,
                Revenue = g.Sum(t => t.Revenue),
                DateIn = g.First(t => t.Date == t.Date).DateIn,
            }).ToList();
            var resArray = groupedTransactions.OrderBy(x => x.DateIn).ToArray();
            for (int i = 1; i < resArray.Length; i++)
            {
                var anchor = resArray[i - 1].DateIn;
                if (i - 1 == 0)
                {
                    anchor = startDate.Value;
                }
                var begin = anchor.AddDays(1);
                var nextDate = resArray[i].DateIn;
                while (begin.Date < nextDate.Date)
                {
                    groupedTransactions.Add(new RevenueDashBoardResponse
                    {
                        Date = begin.Day + "/" + begin.Month,
                        Revenue = 0,
                        DateIn = begin,
                        Method = "Ví"
                    });
                    groupedTransactions.Add(new RevenueDashBoardResponse
                    {
                        Date = begin.Day + "/" + begin.Month,
                        Revenue = 0,
                        DateIn = begin,
                        Method = "Trực Tiếp"
                    });
                    begin = begin.AddDays(1);
                }
            }
            var endx = resArray[resArray.Length - 1].DateIn.AddDays(1);
            while (endx.Date <= endDate.Value.Date)
            {
                groupedTransactions.Add(new RevenueDashBoardResponse
                {
                    Date = endx.Day + "/" + endx.Month,
                    Revenue = 0,
                    DateIn = endx,
                    Method = "Ví",
                });
                groupedTransactions.Add(new RevenueDashBoardResponse
                {
                    Date = endx.Day + "/" + endx.Month,
                    Revenue = 0,
                    DateIn = endx,
                    Method = "Trực Tiếp",
                });
                endx = endx.AddDays(1);
            }
            return groupedTransactions.OrderBy(x => x.DateIn).ToList();
        }

        public DateTime GetTime()
        {
            return GetCurrentTime();
        }
    }
}
