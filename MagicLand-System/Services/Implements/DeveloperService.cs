using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class DeveloperService : BaseService<DeveloperService>, IDeveloperService
    {
        public DeveloperService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<DeveloperService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor, configuration)
        {
        }

        public async Task<List<AccountResponse>> GetStudentAccount(List<Guid> studentIdList)
        {
            var responses = new List<AccountResponse>();
            foreach (var id in studentIdList)
            {
                var account = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.StudentIdAccount == id);
                responses.Add(_mapper.Map<AccountResponse>(account));
            }

            return responses;
        }

        public async Task<List<StudentLearningInfor>> TakeFullAttendanceAndEvaluateAsync(Guid classId, int percentageAbsent, List<EvaluateDataRequest> noteEvaluate)
        {
            try
            {
                var responses = new List<StudentLearningInfor>();

                var studentClasses = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(selector: x => x.Student, predicate: x => x.ClassId == classId);
                var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                    orderBy: x => x.OrderBy(x => x.Date),
                    predicate: x => x.ClassId == classId,
                    include: x => x.Include(x => x.Attendances).Include(x => x.Evaluates));

                var allAttendances = schedules.SelectMany(sc => sc.Attendances);
                var allEvaluates = schedules.SelectMany(sc => sc.Evaluates);

                var numberAbsentDay = percentageAbsent == 0 ? 0 : percentageAbsent / 5;

                Random random = new Random();

                var updateAttendances = new List<Attendance>();
                var updateEvaluates = new List<Evaluate>();
                if (studentClasses.Any())
                {
                    foreach (var stu in studentClasses)
                    {
                        var attendanceAndEvaluateInfors = new List<AttendanceAndEvaluateInfor>();

                        var absentDay = new List<int>();

                        if (numberAbsentDay != 0)
                        {
                            while (absentDay.Count < numberAbsentDay)
                            {
                                int index;
                                do
                                {
                                    index = random.Next(1, schedules.Count);
                                } while (absentDay.Contains(index));
                                absentDay.Add(index);
                            }
                        }

                        int order = 0;
                        foreach (var schedule in schedules)
                        {
                            order++;
                            var currentAttendance = allAttendances.Single(x => x.ScheduleId == schedule.Id && x.StudentId == stu!.Id);
                            var currentEvaluate = allEvaluates.Single(x => x.ScheduleId == schedule.Id && x.StudentId == stu!.Id);

                            var evaluateIndex = random.Next(0, noteEvaluate.Count);

                            if (currentAttendance.IsPresent == null)
                            {
                                currentAttendance.IsPresent = absentDay.Contains(order) ? false : true;
                                updateAttendances.Add(currentAttendance);
                            };

                            if (string.IsNullOrEmpty(currentEvaluate.Status))
                            {

                                currentEvaluate.Status = absentDay.Contains(order) ? null : noteEvaluate[evaluateIndex].Level;
                                currentEvaluate.Note = absentDay.Contains(order) ? string.Empty : noteEvaluate[evaluateIndex].Note;
                                updateEvaluates.Add(currentEvaluate);
                            }

                            attendanceAndEvaluateInfors.Add(new AttendanceAndEvaluateInfor
                            {
                                AttendanceStatus = currentAttendance.IsPresent == true ? "Có Mặt" : "Vắng Mặt",
                                EvaluateStatus = currentEvaluate.Status == null ? "Không Có Đánh Giá" : currentEvaluate.Status,
                                EvaluateNote = currentEvaluate.Note == null ? "" : currentEvaluate.Note,
                                Date = schedule.Date.ToString("MM/dd/yyyy"),
                            });

                        }
                        responses.Add(new StudentLearningInfor
                        {
                            StudentName = stu!.FullName!,
                            LearningInfors = attendanceAndEvaluateInfors,
                        });
                    }
                }
                else
                {
                    throw new BadHttpRequestException("Dữ Liệu Không Hợp Lệ", StatusCodes.Status500InternalServerError);
                }

                _unitOfWork.GetRepository<Attendance>().UpdateRange(updateAttendances);
                _unitOfWork.GetRepository<Evaluate>().UpdateRange(updateEvaluates);
                _unitOfWork.Commit();

                return responses;

            }
            catch (Exception e)
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh " + e.Message, StatusCodes.Status500InternalServerError);
            }

        }
    }
}
