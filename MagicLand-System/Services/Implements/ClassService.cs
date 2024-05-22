using AutoMapper;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Topics;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        private IRoomService _roomService;
        private IUserService _userService;
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IRoomService roomService, IUserService userService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _roomService = roomService;
            _userService = userService;
        }

        #region gia_thuong_code
        public async Task<string> AutoCreateClassCode(string courseId)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId), include: x => x.Include(x => x.Syllabus));
            if (course == null)
            {
                return null;
            }
            //var name = course.Name;
            //var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //string abbreviation = string.Join("", words.Select(word => word[0]));
            if (course.Syllabus == null)
            {
                throw new BadHttpRequestException("course chưa gắn syllabus", StatusCodes.Status400BadRequest);
            }
            var name = course.Syllabus.SubjectCode;
            var classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId)));
            int numberOfClass = 0;
            if (classes == null)
            {
                numberOfClass = 1;
            }
            numberOfClass = classes.Count + 1;
            var abbreviation = name + "-" + numberOfClass;
            return abbreviation;
        }

        public async Task<CreateSingleClassResponse> CreateNewClass(CreateClassRequest request)
        {
            if (request.StartDate < DateTime.Now)
            {
                throw new BadHttpRequestException("Ngày bắt đầu không hợp lệ", StatusCodes.Status400BadRequest);
            }
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()));
            if (course == null)
            {
                throw new BadHttpRequestException("không thấy lớp hợp lệ", StatusCodes.Status400BadRequest);
            }
            Class createdClass = new Class
            {
                Id = Guid.NewGuid(),
                ClassCode = request.ClassCode,
                CourseId = request.CourseId,
                StartDate = request.StartDate,
                EndDate = DateTime.UtcNow,
                LeastNumberStudent = request.LeastNumberStudent,
                LimitNumberStudent = request.LimitNumberStudent,
                LecturerId = request.LecturerId,
                Status = "UPCOMING",
                Method = request.Method,
                District = "Tân Bình",
                City = "Hồ Chí Minh",
                Street = "138 Lương Định Của",
                AddedDate = DateTime.Now,
                Image = course.Image,
            };
            await _unitOfWork.GetRepository<Class>().InsertAsync(createdClass);
            var isSuccessAtClass = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccessAtClass)
            {
                throw new BadHttpRequestException("Thêm lớp thất bại", StatusCodes.Status400BadRequest);
            }
            List<ScheduleRequest> scheduleRequests = request.ScheduleRequests;
            List<string> daysOfWeek = new List<string>();
            foreach (ScheduleRequest scheduleRequest in scheduleRequests)
            {
                daysOfWeek.Add(scheduleRequest.DateOfWeek);
            }
            List<DayOfWeek> convertedDateOfWeek = new List<DayOfWeek>();
            foreach (var dayOfWeek in daysOfWeek)
            {
                if (dayOfWeek.ToLower().Equals("sunday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Sunday);
                }
                if (dayOfWeek.ToLower().Equals("monday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Monday);
                }
                if (dayOfWeek.ToLower().Equals("tuesday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Tuesday);
                }
                if (dayOfWeek.ToLower().Equals("wednesday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Wednesday);
                }
                if (dayOfWeek.ToLower().Equals("thursday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Thursday);
                }
                if (dayOfWeek.ToLower().Equals("friday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Friday);
                }
                if (dayOfWeek.ToLower().Equals("saturday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Saturday);
                }
            }

            int numberOfSessions = course.NumberOfSession;
            int scheduleAdded = 0;
            DateTime startDate = request.StartDate;
            List<Schedule> schedules = new List<Schedule>();
            List<ScheduleRequest> sc = request.ScheduleRequests;
            while (scheduleAdded < numberOfSessions)
            {
                if (convertedDateOfWeek.Contains(startDate.DayOfWeek))
                {
                    string dateString = startDate.DayOfWeek.ToString().ToLower();
                    Guid slotId = Guid.NewGuid();
                    foreach (var sq in sc)
                    {
                        if (sq.DateOfWeek.ToLower().Equals(dateString))
                        {
                            slotId = sq.SlotId;
                        }
                    }

                    schedules.Add(new Schedule
                    {
                        Id = Guid.NewGuid(),
                        ClassId = createdClass.Id,
                        Date = startDate,
                        RoomId = request.RoomId,
                        SlotId = slotId,
                        DayOfWeek = (int)Math.Pow(2, (int)startDate.DayOfWeek),
                    });
                    scheduleAdded++;
                }
                startDate = startDate.AddDays(1);
            }
            try
            {
                await _unitOfWork.GetRepository<Schedule>().InsertRangeAsync(schedules);
                var isSchedule = await _unitOfWork.CommitAsync() > 0;
                if (!isSchedule)
                {
                    throw new BadHttpRequestException("thêm lịch thất bại", StatusCodes.Status400BadRequest);
                }
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException == null ? ex.Message : ex.InnerException.ToString(), StatusCodes.Status400BadRequest);
            }
            var endDate = schedules.OrderByDescending(x => x.Date).FirstOrDefault().Date;
            createdClass.EndDate = endDate;

            _unitOfWork.GetRepository<Class>().UpdateAsync(createdClass);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                throw new BadHttpRequestException("Cập nhật lớp thất bại");
            }
            var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.LecturerId.ToString()), selector: x => x.FullName);
            var roomName = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.RoomId.ToString()), selector: x => x.Name);
            var classCode = request.ClassCode;
            List<ScheduleRequestV2> Times = new List<ScheduleRequestV2>();
            foreach (var schedule in request.ScheduleRequests)
            {
                var dow = "";
                if (schedule.DateOfWeek.ToLower().Equals("sunday"))
                {
                    dow = "Chủ Nhật";
                }
                if (schedule.DateOfWeek.ToLower().Equals("monday"))
                {
                    dow = "Thứ 2";
                }
                if (schedule.DateOfWeek.ToLower().Equals("tuesday"))
                {
                    dow = "Thứ 3";

                }
                if (schedule.DateOfWeek.ToLower().Equals("wednesday"))
                {
                    dow = "Thứ 4";
                }
                if (schedule.DateOfWeek.ToLower().Equals("thursday"))
                {
                    dow = "Thứ 5";
                }
                if (schedule.DateOfWeek.ToLower().Equals("friday"))
                {
                    dow = "Thứ 6";
                }
                if (schedule.DateOfWeek.ToLower().Equals("saturday"))
                {
                    dow = "Thứ 7";
                }
                var slot = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.SlotId.ToString()));
                var startTime = slot.StartTime;
                var endTime = slot.EndTime;
                var time = dow + ": " + startTime + "-" + endTime;
                var slotx = startTime + " - " + endTime;
                Times.Add(new ScheduleRequestV2
                {
                    Schedule = dow,
                    Slot = slotx,
                });
            }
            return new CreateSingleClassResponse
            {
                ClassCode = classCode,
                Times = Times,
                LecturerName = lecturerName,
                RoomName = roomName,
                Success = isSuccess,
            };

        }


        public async Task<ClassResultResponse> GetAllClass(string searchString = null, string status = null)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(include: x => x.Include(x => x.Schedules));
            List<MyClassResponse> result = new List<MyClassResponse>();
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            foreach (var c in classes)
            {
                var schedulex = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == c.Id)).FirstOrDefault();
                if (schedulex == null) { continue; }
                var room = (await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == schedulex.RoomId));
                if (room == null) { continue; }
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.LecturerId.ToString()));
                if (lecturer == null) { continue; }
                RoomResponse roomResponse = new RoomResponse
                {
                    Floor = room.Floor.Value,
                    Capacity = room.Capacity,
                    RoomId = room.Id,
                    Name = room.Name,
                    Status = room.Status,
                    LinkUrl = room.LinkURL,

                };
                LecturerResponse lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                };
                List<DailySchedule> schedules = new List<DailySchedule>();
                var DaysOfWeek = c.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
                foreach (var day in DaysOfWeek)
                {
                    var slot = slots.Where(x => x.Id.ToString().ToLower().Equals(day.SlotId.ToString().ToLower())).FirstOrDefault();
                    if (day.DayOfWeek == 1)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Sunday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 2)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Monday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 4)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Tuesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 8)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Wednesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 16)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Thursday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 32)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Friday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 64)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Saturday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                }
                Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                var studentList = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id);
                var studentClassCount = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id && x.Status.Equals("Saved"));
                var scheduleCompleted = c.Schedules.Where(x => x.Date.Date <= DateTime.Now.Date);
                MyClassResponse myClassResponse = new MyClassResponse
                {
                    ClassId = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,
                    ClassCode = c.ClassCode,
                    LecturerName = lecturer.FullName,
                    CoursePrice = await GetDynamicPrice(course.Id, false),
                    EndDate = c.EndDate,
                    CourseId = c.CourseId,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = studentList.Count - studentClassCount.Count,
                    Schedules = schedules,
                    CourseName = course.Name,
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,
                    CreatedDate = c.AddedDate.Value,
                    CurrentSession = scheduleCompleted.Count(),
                };
                var syllabusCode = "undefined";
                var syllabusName = "undefined";
                var syllabusType = "undefined";
                if (course.Syllabus != null)
                {
                    syllabusCode = course.Syllabus.SubjectCode;
                    syllabusName = course.Syllabus.Name;
                    syllabusType = course.Syllabus.SyllabusCategory.Name;
                }
                CustomCourseResponse customCourseResponse = new CustomCourseResponse
                {
                    Image = course.Image,
                    MainDescription = course.MainDescription,
                    MaxYearOldsStudent = course.MaxYearOldsStudent,
                    MinYearOldsStudent = course.MinYearOldsStudent,
                    Name = course.Name,
                    Price = await GetDynamicPrice(course.Id, false),
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = string.Empty,
                };
                myClassResponse.CourseResponse = customCourseResponse;
                result.Add(myClassResponse);
            }
            var numberOfClasses = 0;
            if (status == null)
            {
                numberOfClasses = (await _unitOfWork.GetRepository<Class>().GetListAsync()).Count;
            }
            else
            {
                numberOfClasses = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Status.ToLower().Equals(status.ToLower()))).Count;
            }
            //
            if (result.Count == 0)
            {
                return null;
            }
            if (result != null)
            {
                result = result.OrderByDescending(x => x.CreatedDate).ToList();
            }
            if (searchString == null && status == null)
            {
                return new ClassResultResponse
                {
                    MyClassResponses = result.OrderByDescending(x => x.CreatedDate).ToList(),
                    NumberOfClasses = numberOfClasses,
                };
            }
            if (searchString == null)
            {
                return new ClassResultResponse
                {
                    MyClassResponses = (result.Where(x => x.Status.ToLower().Equals(status.ToLower())).OrderByDescending(x => x.CreatedDate).ToList()),
                    NumberOfClasses = numberOfClasses,
                };
            }
            if (status == null)
            {
                List<MyClassResponse> res = new List<MyClassResponse>();
                var filter1 = result.Where(x => x.ClassCode.ToLower().Contains(searchString.ToLower()));
                if (filter1 != null)
                {
                    res.AddRange(filter1);
                }
                var filter2 = result.Where(x => x.CourseName.ToLower().Contains((searchString.ToLower())));
                if (filter2 != null)
                {
                    res.AddRange(filter2);
                }
                return new ClassResultResponse
                {
                    MyClassResponses = res.OrderByDescending(x => x.CreatedDate).ToList(),
                    NumberOfClasses = numberOfClasses,
                };
            }
            return new ClassResultResponse
            {
                MyClassResponses = (result.Where(x => ((x.ClassCode.ToLower().Contains(searchString.ToLower()) || x.CourseName.ToLower().Contains(searchString.ToLower())) && x.Status.ToLower().Equals(status.ToLower())))).OrderByDescending(x => x.CreatedDate).ToList(),
                NumberOfClasses = numberOfClasses,
            };

        }

        public async Task<MyClassResponse> GetClassDetail(string id)
        {
            //var listClass = await GetAllClass();
            //if (listClass == null)
            //{
            //    return null;
            //}
            //var matchClass = listClass.SingleOrDefault(x => x.ClassId.ToString().Equals(id.ToString()));
            //return matchClass;
            var c = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id), include: x => x.Include(x => x.Schedules));
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            var schedulex = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == c.Id)).FirstOrDefault();
            if (schedulex == null) { return null; }
            var room = (await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == schedulex.RoomId));
            if (room == null) { return null; }
            var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.LecturerId.ToString()));
            if (lecturer == null) { return null; }
            RoomResponse roomResponse = new RoomResponse
            {
                Floor = room.Floor.Value,
                Capacity = room.Capacity,
                RoomId = room.Id,
                Name = room.Name,
                Status = room.Status,
                LinkUrl = room.LinkURL,

            };
            LecturerResponse lecturerResponse = new LecturerResponse
            {
                AvatarImage = lecturer.AvatarImage,
                DateOfBirth = lecturer.DateOfBirth,
                Email = lecturer.Email,
                FullName = lecturer.FullName,
                Gender = lecturer.Gender,
                LectureId = lecturer.Id,
                Phone = lecturer.Phone,
            };
            List<DailySchedule> schedules = new List<DailySchedule>();
            var DaysOfWeek = c.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
            foreach (var day in DaysOfWeek)
            {
                var slot = slots.Where(x => x.Id.ToString().ToLower().Equals(day.SlotId.ToString().ToLower())).FirstOrDefault();
                if (day.DayOfWeek == 1)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Sunday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 2)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Monday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 4)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Tuesday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 8)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Wednesday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 16)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Thursday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 32)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Friday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
                if (day.DayOfWeek == 64)
                {
                    schedules.Add(new DailySchedule
                    {
                        DayOfWeek = "Saturday",
                        EndTime = slot.EndTime,
                        StartTime = slot.StartTime,
                    });
                }
            }
            Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
            var studentList = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id);
            var studentClassCount = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id && x.Status.Equals("Saved"));
            MyClassResponse myClassResponse = new MyClassResponse
            {
                ClassId = c.Id,
                LimitNumberStudent = c.LimitNumberStudent,
                ClassCode = c.ClassCode,
                LecturerName = lecturer.FullName,
                //CoursePrice = c.Course.Price,
                EndDate = c.EndDate,
                CourseId = c.CourseId,
                Image = c.Image,
                LeastNumberStudent = c.LeastNumberStudent,
                Method = c.Method,
                StartDate = c.StartDate,
                Status = c.Status,
                Video = c.Video,
                NumberStudentRegistered = studentList.Count - studentClassCount.Count,
                Schedules = schedules,
                CourseName = course.Name,
                LecturerResponse = lecturerResponse,
                RoomResponse = roomResponse,
                CreatedDate = c.AddedDate.Value,
            };
            var syllabusCode = "undefined";
            var syllabusName = "undefined";
            var syllabusType = "undefined";
            if (course.Syllabus != null)
            {
                syllabusCode = course.Syllabus.SubjectCode;
                syllabusName = course.Syllabus.Name;
                syllabusType = course.Syllabus.SyllabusCategory.Name;
            }
            CustomCourseResponse customCourseResponse = new CustomCourseResponse
            {
                Image = course.Image,
                MainDescription = course.MainDescription,
                MaxYearOldsStudent = course.MaxYearOldsStudent,
                MinYearOldsStudent = course.MinYearOldsStudent,
                Name = course.Name,
                //Price = course.Price,
                SyllabusCode = syllabusCode,
                SyllabusName = syllabusName,
                SyllabusType = syllabusType,
                Status = string.Empty,
            };
            myClassResponse.CourseResponse = customCourseResponse;
            return myClassResponse;

        }

        public async Task<bool> UpdateClass(string classId, UpdateClassRequest request)
        {
            var classFound = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()));
            if (classFound == null)
            {
                return false;
            }
            if (request.LeastNumberStudent != null)
            {
                classFound.LeastNumberStudent = request.LeastNumberStudent.Value;
            }
            if (request.LimitNumberStudent != null)
            {
                classFound.LimitNumberStudent = request.LimitNumberStudent.Value;
            }
            if (request.Method != null)
            {
                classFound.Method = request.Method;
            }
            if (request.CourseId != null)
            {
                classFound.CourseId = request.CourseId.Value;
            }
            if (request.LeastNumberStudent >= request.LimitNumberStudent)
            {
                throw new BadHttpRequestException("số lượng tối thiểu phải nhỏ hơn số lượng tối đa", StatusCodes.Status400BadRequest);
            }
            classFound.Status = "UPCOMING";
            classFound.AddedDate = DateTime.Now;
            _unitOfWork.GetRepository<Class>().UpdateAsync(classFound);
            bool isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                return false;
            }
            if (request.RoomId != null || request.LecturerId != null)
            {
                var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(classFound.Id.ToString()));
                var filterSchedule = schedules.Where(x => x.Date >= DateTime.Now.AddHours(-23));
                if (filterSchedule != null)
                {
                    foreach (var schedule in filterSchedule)
                    {
                        if (request.RoomId != null)
                        {
                            schedule.RoomId = request.RoomId.Value;
                        }
                        if (request.LecturerId != null)
                        {
                            schedule.SubLecturerId = request.LecturerId.Value;
                        }
                        _unitOfWork.GetRepository<Schedule>().UpdateAsync(schedule);
                        var isSuccessful = await _unitOfWork.CommitAsync() > 0;
                        if (!isSuccess)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public async Task<List<ClassProgressResponse>> GetClassProgressResponsesAsync(string classId)
        {
            var scheduleFounds = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(classId), include: x => x.Include(x => x.Room).Include(x => x.Slot).Include(x => x.Class));
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId));
            if (scheduleFounds == null)
            {
                return new List<ClassProgressResponse>();
            }
            List<ClassProgressResponse> responses = new List<ClassProgressResponse>();
            foreach (var schedule in scheduleFounds)
            {
                User lecturer = null;
                if (schedule.SubLecturerId != null)
                {
                    lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.SubLecturerId.ToString()));
                }
                else
                {
                    lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.Class.LecturerId.ToString()));
                }
                UserResponse userResponse = new UserResponse
                {
                    Email = lecturer.Email,
                    Address = lecturer.Address,
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth.Value,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    Id = lecturer.Id,
                    Phone = lecturer.Phone,
                };
                RoomResponse roomResponse = new RoomResponse
                {
                    Capacity = schedule.Room.Capacity,
                    Floor = schedule.Room.Floor.Value,
                    LinkUrl = schedule.Room.LinkURL,
                    Name = schedule.Room.Name,
                    RoomId = schedule.RoomId,
                    Status = schedule.Room.Status,
                };
                SlotResponse slotResponse = new SlotResponse
                {
                    EndTime = TimeOnly.Parse(schedule.Slot.EndTime),
                    StartTime = TimeOnly.Parse(schedule.Slot.StartTime),
                    SlotId = schedule.SlotId,
                    StartTimeString = TimeOnly.Parse(schedule.Slot.StartTime).ToString("HH:mm"),
                    EndTimeString = TimeOnly.Parse(schedule.Slot.EndTime).ToString("HH:mm"),
                };
                var status = "completed";
                if (schedule.Date > DateTime.Now.AddHours(-23))
                {
                    status = "future";
                }
                if (classx.Status.Equals(ClassStatusEnum.CANCELED.ToString()))
                {
                    status = "canceled";
                }
                var dateOfWeek = "monday";
                if (schedule.DayOfWeek == 1)
                {
                    dateOfWeek = "sunday";
                }
                if (schedule.DayOfWeek == 2)
                {
                    dateOfWeek = "monday";
                }
                if (schedule.DayOfWeek == 4)
                {
                    dateOfWeek = "tuesday";
                }
                if (schedule.DayOfWeek == 8)
                {
                    dateOfWeek = "wednesday";
                }
                if (schedule.DayOfWeek == 16)
                {
                    dateOfWeek = "thursday";
                }
                if (schedule.DayOfWeek == 32)
                {
                    dateOfWeek = "friday";
                }
                if (schedule.DayOfWeek == 64)
                {
                    dateOfWeek = "saturday";
                }
                ClassProgressResponse progressResponse = new ClassProgressResponse
                {
                    Date = schedule.Date,
                    DayOfWeeks = dateOfWeek,
                    Id = schedule.Id,
                    Room = roomResponse,
                    Slot = slotResponse,
                    Status = status,
                    Lecturer = userResponse,

                };
                responses.Add(progressResponse);
            }
            var responsesx = responses.OrderBy(x => x.Date).ToArray();
            List<ClassProgressResponse> result = new List<ClassProgressResponse>();
            for (int i = 0; i < responsesx.Length; i++)
            {
                responsesx[i].Index = i + 1;
                result.Add(responsesx[i]);
            }
            result = result.OrderBy(x => x.Index).ToList();
            return result;
        }

        public async Task<List<ClassForAttendance>> GetAllClassForAttandance(string? searchString, DateTime dateTime, string? attendanceStatusInput = null)
        {
            var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                predicate: x => (x.Date.Date.Year == dateTime.Date.Year) && (x.Date.Month == dateTime.Date.Month) && (x.Date.Date == dateTime.Date.Date),
                include: x => x.Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class).ThenInclude(cls => cls!.Course)!);

            if (schedules.Count == 0 || schedules == null)
            {
                return new List<ClassForAttendance>();
            }
            List<ClassForAttendance> classForAttendances = new List<ClassForAttendance>();
            foreach (var schedule in schedules)
            {
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.Class.LecturerId.ToString()));
                var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(
                    predicate: x => x.ScheduleId.ToString().Equals(schedule.Id.ToString()) && x.Student!.StudentClasses.Any(sc => sc.ClassId == schedule.ClassId && sc.SavedTime == null));

                if (attendances != null && attendances.Count > 0)
                {
                    var attendanceStatus = "NTA";
                    foreach (var attendance in attendances)
                    {
                        if (attendance == null)
                        {
                            attendanceStatus = "Not found";
                        }
                        if (attendance.IsPresent != null)
                        {
                            attendanceStatus = "TA";
                        }
                    }
                    LecturerResponse response = new LecturerResponse
                    {
                        AvatarImage = lecturer.AvatarImage,
                        DateOfBirth = lecturer.DateOfBirth,
                        Email = lecturer.Email,
                        FullName = lecturer.FullName,
                        Gender = lecturer.Gender,
                        LectureId = lecturer.Id,
                        Phone = lecturer.Phone,
                    };
                    ClassForAttendance classForAttendance = new ClassForAttendance
                    {
                        ClassCode = schedule.Class.ClassCode,
                        ClassId = schedule.Class.Id,
                        //ClassSubject = schedule.Class.Course.CourseCategory.Name,
                        Method = schedule.Class.Method,
                        Image = schedule.Class.Image,
                        EndDate = schedule.Class.EndDate,
                        //CoursePrice = schedule.Class.Course.Price,
                        CourseId = schedule.Class.Course.Id,
                        CourseName = schedule.Class.Course.Name,
                        CourseRate = schedule.Class.Course.TotalRate / 5,
                        LeastNumberStudent = schedule.Class.LeastNumberStudent,
                        LimitNumberStudent = schedule.Class.LimitNumberStudent,
                        StartDate = schedule.Class.StartDate,
                        Status = schedule.Class.Status,
                        Video = schedule.Class.Video,
                        Schedule = schedule,
                        Lecturer = response,
                        AttandanceStatus = attendanceStatus
                    };
                    classForAttendances.Add(classForAttendance);
                }

            }
            if (searchString != null)
            {
                classForAttendances = classForAttendances.Where(x => (x.ClassCode.ToLower().Equals(searchString.Trim().ToLower()) || x.CourseName.Trim().ToLower().Contains(searchString.Trim().ToLower()) || x.Lecturer.FullName.Trim().ToLower().Contains(searchString.Trim().ToLower()))).ToList();
            }
            if (attendanceStatusInput != null)
            {
                classForAttendances = classForAttendances.Where(x => x.AttandanceStatus.ToLower().Equals(attendanceStatusInput.ToLower())).ToList();
            }
            return classForAttendances;
        }

        public async Task<List<ClassWithDailyScheduleRes>> GetSuitableClassAsync(Guid classId, List<Guid> studentIdList)
        {
            var currentClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId,
            include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules));

            if (currentClass == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            if (!studentIdList.All(stu => currentClass.StudentClasses.Any(sc => sc.StudentId == stu)))
            {
                throw new BadHttpRequestException($"Id [{string.Join(", ", studentIdList)}] Của Các Học Sinh Không Thuộc Về Lớp [{currentClass.ClassCode}]", StatusCodes.Status400BadRequest);
            }

            if (currentClass.Status != ClassStatusEnum.UPCOMING.ToString() && currentClass.Status != ClassStatusEnum.CANCELED.ToString())
            {
                string errorMessage = currentClass.Status == ClassStatusEnum.PROGRESSING.ToString()
                    ? "Đã Bắt Đầu"
                    : "Đã Hoàn Thành";

                throw new BadHttpRequestException($"Chỉ Có Thể Chuyển Học Sinh Thuộc Lớp Sắp Bắt Đầu Hoặc Đã Hủy, Lớp [{currentClass.ClassCode}] [{errorMessage}], Không Thể Chuyển Lớp",
                      StatusCodes.Status400BadRequest);
            }

            var classes = new List<Class>();

            foreach (Guid id in studentIdList)
            {
                var currentStudentClasses = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Id != classId && x.StudentClasses.Any(sc => sc.StudentId == id) && (x.Status != ClassStatusEnum.COMPLETED.ToString() || x.Status != ClassStatusEnum.CANCELED.ToString()),
                include: x => x.Include(x => x.Schedules).ThenInclude(sc => sc.Slot)!);

                if (currentStudentClasses != null)
                {
                    classes.AddRange(currentStudentClasses);
                }
            }

            var allCourseClass = await _unitOfWork.GetRepository<Class>()
               .GetListAsync(predicate: x => x.CourseId == currentClass.CourseId && x.Status == ClassStatusEnum.UPCOMING.ToString(),
               include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
               .Include(x => x.Lecture)
               .Include(x => x.StudentClasses));

            var suitableClasses = allCourseClass.Where(courCls =>
                !courCls.Schedules.Any(courSchedule =>
                    classes.Any(currCls =>
                        currCls.Schedules.Any(schedule =>
                            schedule.Slot?.StartTime == courSchedule.Slot?.StartTime))))
                .ToList();
            #region
            //var suitableClasses = new List<Class>();

            //foreach (var courCls in allCourseClass)
            //{
            //    bool hasOverlap = false;

            //    foreach (var courSchedule in courCls.Schedules)
            //    {
            //        foreach (var currCls in classes)
            //        {
            //            foreach (var schedule in currCls.Schedules)
            //            {
            //                if (schedule.Slot?.StartTime == courSchedule.Slot?.StartTime)
            //                {
            //                    hasOverlap = true;
            //                    break; // No need to check further, as we found an overlap
            //                }
            //            }

            //            if (hasOverlap)
            //                break; // No need to check further, as we found an overlap
            //        }

            //        if (hasOverlap)
            //            break; // No need to check further, as we found an overlap
            //    }

            //    if (!hasOverlap)
            //        suitableClasses.Add(courCls);
            //}
            #endregion

            suitableClasses = suitableClasses
           .Where(cls => cls.StudentClasses.Count + studentIdList.Count <= cls.LimitNumberStudent)
           .Where(cls => cls.Id != currentClass.Id).ToList();

            var suitableClassesx = suitableClasses.Select(cls => _mapper.Map<ClassWithDailyScheduleRes>(cls)).ToList();
            List<ClassWithDailyScheduleRes> res = new List<ClassWithDailyScheduleRes>();
            foreach (var suitableClass in suitableClassesx)
            {
                var groupBy = from schedule in suitableClass.Schedules
                              group schedule by new { schedule.DayOfWeek, schedule.StartTime, schedule.EndTime } into grouped
                              select new DailySchedule
                              {
                                  DayOfWeek = grouped.Key.DayOfWeek,
                                  StartTime = grouped.Key.StartTime,
                                  EndTime = grouped.Key.EndTime,
                              };
                foreach (var sch in groupBy)
                {
                    if (sch.DayOfWeek.Equals("Sunday"))
                    {
                        sch.DayOfWeek = "Chủ Nhật";
                    }
                    if (sch.DayOfWeek.Equals("Monday"))
                    {
                        sch.DayOfWeek = "Thứ Hai";
                    }
                    if (sch.DayOfWeek.Equals("Tuesday"))
                    {
                        sch.DayOfWeek = "Thứ Ba";
                    }
                    if (sch.DayOfWeek.Equals("Wednesday"))
                    {
                        sch.DayOfWeek = "Thứ Tư";
                    }
                    if (sch.DayOfWeek.Equals("Thursday"))
                    {
                        sch.DayOfWeek = "Thứ Năm";
                    }
                    if (sch.DayOfWeek.Equals("Friday"))
                    {
                        sch.DayOfWeek = "Thứ Sáu";
                    }
                    if (sch.DayOfWeek.Equals("Saturday"))
                    {
                        sch.DayOfWeek = "Thứ Bảy";
                    }
                }
                suitableClass.Schedules = groupBy.ToList();

            }
            return suitableClassesx;

        }

        public async Task<bool> CancelClass(string classId)
        {
            var classFound = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()));
            if (classFound == null)
            {
                throw new BadHttpRequestException("không thể tìm ra lớp có id như vậy", StatusCodes.Status400BadRequest);
            }
            if (!classFound.Status.Equals(ClassStatusEnum.UPCOMING.ToString()))
            {
                throw new BadHttpRequestException("chỉ có thể hủy class với status là upcoming", StatusCodes.Status400BadRequest);
            }
            classFound.Status = ClassStatusEnum.CANCELED.ToString();
            _unitOfWork.GetRepository<Class>().UpdateAsync(classFound);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<bool> UpdateSession(string sessionId, UpdateSessionRequest request)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(sessionId));
            var myScheudule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => (!x.Id.ToString().Equals(schedule.Id.ToString()) && x.ClassId.ToString().Equals(schedule.ClassId.ToString())));
            var isExist = myScheudule.Where(x => (x.Date.Date.Day == request.DateTime.Value.Day && x.Date.Month == request.DateTime.Value.Month && x.Date.Year == request.DateTime.Value.Year && x.SlotId.ToString().Equals(request.SlotId.ToString())));
            if (isExist.Count() > 0)
            {
                throw new BadHttpRequestException("Ngày này đã tồn tại , không thể update", StatusCodes.Status400BadRequest);
            }
            if (schedule == null)
            {
                return false;
            }
            if (request.LecturerId != null)
            {
                schedule.SubLecturerId = request.LecturerId;
            }
            if (request.RoomId != null)
            {
                schedule.RoomId = request.RoomId.Value;
            }
            if (request.SlotId != null)
            {
                schedule.SlotId = request.SlotId.Value;
            }
            if (request.DateTime != null)
            {
                schedule.Date = request.DateTime.Value;
                var date = request.DateTime.Value.DayOfWeek;
                if (date == DayOfWeek.Sunday)
                {
                    schedule.DayOfWeek = 1;
                }
                if (date == DayOfWeek.Monday)
                {
                    schedule.DayOfWeek = 2;
                }
                if (date == DayOfWeek.Tuesday)
                {
                    schedule.DayOfWeek = 4;
                }
                if (date == DayOfWeek.Wednesday)
                {
                    schedule.DayOfWeek = 8;
                }
                if (date == DayOfWeek.Tuesday)
                {
                    schedule.DayOfWeek = 16;
                }
                if (date == DayOfWeek.Friday)
                {
                    schedule.DayOfWeek = 32;
                }
                if (date == DayOfWeek.Saturday)
                {
                    schedule.DayOfWeek = 64;
                }
            }
            _unitOfWork.GetRepository<Schedule>().UpdateAsync(schedule);
            bool isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<bool> MakeUpClass(string StudentId, string ScheduleId, MakeupClassRequest request)
        {
            try
            {
                var currentClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Schedules.Any(sch => sch.Id == Guid.Parse(ScheduleId)));
                var makeUpClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Schedules.Any(sch => sch.Id == Guid.Parse(request.ScheduleId)));

                if (currentClass == null || makeUpClass == null) { return false; }

                if (currentClass.CourseId != makeUpClass.CourseId)
                {
                    throw new BadHttpRequestException($"Không Thể Chuyển Lớp Học Bù Cho 2 Lớp Khác Khóa Học", StatusCodes.Status400BadRequest);
                }

                var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => (x.StudentId.ToString().Equals(StudentId) && x.ScheduleId.ToString().Equals(ScheduleId)));
                var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => (x.StudentId.ToString().Equals(StudentId) && x.ScheduleId.ToString().Equals(ScheduleId)));

                attendance.IsPublic = false;
                evaluate.IsPublic = false;

                var makeUpAttendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    IsPresent = null,
                    IsPublic = true,
                    Note = null,
                    MakeUpFromScheduleId = Guid.Parse(ScheduleId),
                    StudentId = Guid.Parse(StudentId),
                    ScheduleId = Guid.Parse(request.ScheduleId)
                };
                var makeUpEvaluate = new Evaluate
                {
                    Id = Guid.NewGuid(),
                    IsPublic = true,
                    Note = null,
                    Status = null,
                    MakeUpFromScheduleId = Guid.Parse(ScheduleId),
                    StudentId = Guid.Parse(StudentId),
                    ScheduleId = Guid.Parse(request.ScheduleId),
                };

                _unitOfWork.GetRepository<Evaluate>().UpdateAsync(evaluate);
                _unitOfWork.GetRepository<Attendance>().UpdateAsync(attendance);
                await _unitOfWork.GetRepository<Evaluate>().InsertAsync(makeUpEvaluate);
                await _unitOfWork.GetRepository<Attendance>().InsertAsync(makeUpAttendance);

                var isSuccess = _unitOfWork.Commit() > 0;
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh: [{ex}]", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<List<ScheduleResponse>> GetScheduleCanMakeUp(string scheduleId, string studentId, DateTime? date = null, string? keyword = null, string? slotId = null)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(scheduleId), include: x => x.Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class).ThenInclude(x => x.Course));
            if (schedule == null)
            {
                return new List<ScheduleResponse>();
            }
            var allSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(include: x => x.Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class).ThenInclude(x => x.Course));
            var scheduleInCourse = allSchedule.Where(x => x.Class.Course.Id.ToString().Equals(schedule.Class.Course.Id.ToString()));
            var groupbyId = scheduleInCourse.GroupBy(x => x.Class.Id);
            List<Guid> Ids = new List<Guid>();
            foreach (var group in groupbyId)
            {
                Ids.Add(group.Key);
            }
            var scheduleClassIndex = allSchedule.Where(x => x.Class.Id.ToString().Equals(schedule.Class.Id.ToString()) && x.Class.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString())).ToList();
            var scheduleClassSort = scheduleClassIndex.OrderBy(x => x.Date).ToArray();
            int index = -1;
            for (int i = 0; i < scheduleClassSort.Length; i++)
            {
                if (scheduleClassSort[i].Date == schedule.Date)
                {
                    index = i; break;
                }
            }
            List<Schedule> schedules = new List<Schedule>();
            foreach (var Id in Ids)
            {
                if (Id != schedule.Class.Id)
                {
                    var ClassIndex = allSchedule.Where(x => x.ClassId.ToString().Equals(Id.ToString())).ToList();
                    var ClassSort = ClassIndex.OrderBy(x => x.Date).ToArray();
                    if (ClassSort.Length - 1 < index)
                    {
                        schedules.Add(ClassSort[ClassSort.Length - 1]);
                    }
                    else
                    {
                        schedules.Add(ClassSort[index]);
                    }
                }
            }
            List<ScheduleResponse> responses = new List<ScheduleResponse>();
            foreach (var schedulex in schedules)
            {
                User lecturer = new User();
                if (schedulex.SubLecturerId != null)
                {
                    lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedulex.SubLecturerId.ToString()));
                }
                else
                {
                    lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedulex.Class.LecturerId.ToString()));
                }
                var dateOfWeek = "monday";
                if (schedulex.DayOfWeek == 1)
                {
                    dateOfWeek = "sunday";
                }
                if (schedulex.DayOfWeek == 2)
                {
                    dateOfWeek = "monday";
                }
                if (schedulex.DayOfWeek == 4)
                {
                    dateOfWeek = "tuesday";
                }
                if (schedulex.DayOfWeek == 8)
                {
                    dateOfWeek = "wednesday";
                }
                if (schedulex.DayOfWeek == 16)
                {
                    dateOfWeek = "thursday";
                }
                if (schedulex.DayOfWeek == 32)
                {
                    dateOfWeek = "friday";
                }
                if (schedulex.DayOfWeek == 64)
                {
                    dateOfWeek = "saturday";
                }

                ScheduleResponse scheduleResponse = new ScheduleResponse
                {
                    ScheduleId = schedulex.Id,
                    Date = schedulex.Date,
                    Room = new RoomResponse
                    {
                        Capacity = schedulex.Room.Capacity,
                        Floor = schedulex.Room.Floor.Value,
                        LinkUrl = schedulex.Room.LinkURL,
                        Name = schedulex.Room.Name,
                        RoomId = schedulex.RoomId,
                        Status = schedulex.Room.Status,
                    },
                    Slot = new SlotResponse
                    {
                        SlotId = schedulex.SlotId,
                        EndTime = TimeOnly.Parse(schedulex.Slot.EndTime),
                        StartTime = TimeOnly.Parse(schedulex.Slot.StartTime),
                        StartTimeString = schedulex.Slot.StartTime,
                        EndTimeString = schedulex.Slot.EndTime,
                    },
                    DayOfWeeks = dateOfWeek,
                    Lecturer = new LecturerResponse
                    {
                        AvatarImage = lecturer.AvatarImage,
                        DateOfBirth = lecturer.DateOfBirth,
                        Email = lecturer.Email,
                        FullName = lecturer.FullName,
                        Gender = lecturer.Gender,
                        LectureId = lecturer.Id,
                        Phone = lecturer.Phone,
                        Role = "Lecturer",
                    },
                    ClassCode = schedulex.Class.ClassCode,
                    Method = schedulex.Class.Method,
                };
                responses.Add(scheduleResponse);
            }
            var attendance = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId));
            if (attendance == null)
            {
                responses = responses;
            }
            List<ScheduleResponse> resultList = new List<ScheduleResponse>();
            foreach (var response in responses)
            {
                var isExist = attendance.SingleOrDefault(x => x.ScheduleId.ToString().Equals(response.ScheduleId.ToString()));
                if (isExist == null)
                {
                    resultList.Add(response);
                }
            }
            if (date != null && responses != null)
            {
                resultList = (responses.Where(x => (x.Date.Day == date.Value.Day && x.Date.Month == date.Value.Month && x.Date.Year == date.Value.Year))).ToList();
            }
            if (slotId != null && responses != null)
            {
                resultList = (responses.Where(x => x.Slot.SlotId.ToString().Equals(slotId.ToString()))).ToList();
            }
            if (keyword != null && responses != null)
            {
                resultList = (responses.Where(x => (x.ClassCode.Trim().ToLower().Contains(keyword.Trim().ToLower()) || x.Lecturer.FullName.ToLower().Trim().Contains(keyword.ToLower().Trim())))).ToList();
            }
            if (resultList != null)
            {
                resultList = resultList.Where(x => x.Date > DateTime.Now).OrderBy(x => x.Date).ToList();
            }
            return resultList;
        }
        public async Task<InsertClassesResponse> InsertClasses(List<CreateClassesRequest> request)
        {
            if (request == null)
            {
                throw new BadHttpRequestException("Request không có", StatusCodes.Status400BadRequest);
            }
            InsertClassesResponse response = new InsertClassesResponse
            {
                SuccessRow = 0,
                FailureRow = 0,

            };
            List<RowInsertResponse> rows = new List<RowInsertResponse>();
            foreach (var rq in request)
            {
                Guid? syllabusId = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.SubjectCode.Trim().ToLower().Equals(rq.CourseCode.Trim().ToLower()), selector: x => x.Id);
                Guid? courseId = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syllabusId, selector: x => x.Id);
                if (syllabusId == Guid.Empty || courseId == Guid.Empty)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Không tìm thấy khóa học có mã như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LeastNumberStudent >= rq.LimitNumberStudent)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa không thể nhỏ hơn số lượng tối thiểu",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LimitNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LeastNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối thiểu phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LimitNumberStudent > 30)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải nhỏ hơn 30",
                        IsSucess = false
                    });
                    continue;
                }
                var schedules = rq.ScheduleRequests;
                List<ScheduleRequest> scheduleRequests = new List<ScheduleRequest>();
                foreach (var schedule in schedules)
                {
                    try
                    {
                        //string[] parts = schedule.ScheduleTime.Split(':');
                        string day = schedule.Schedule; // Thứ 2
                        string timeRange = schedule.Slot.Trim(); // 15:00 - 20:00
                        string startTime = timeRange.Substring(0, timeRange.IndexOf("-"));
                        var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Trim().Equals(startTime.Trim()), selector: x => x.Id);
                        if (slotId == null || slotId == Guid.Empty)
                        {
                            rows.Add(new RowInsertResponse
                            {
                                Index = rq.Index,
                                Messsage = $" index {rq.Index} Không tìm thấy slot như vậy , slot có thời gian {timeRange} không tồn tại",
                                IsSucess = false
                            });
                            break;
                        }
                        string dateofweek = "";
                        if (day.ToLower().Equals("thứ 2"))
                        {
                            dateofweek = "monday";
                        }
                        if (day.ToLower().Equals("thứ 3"))
                        {
                            dateofweek = "tuesday";
                        }
                        if (day.ToLower().Equals("thứ 4"))
                        {
                            dateofweek = "wednesday";
                        }
                        if (day.ToLower().Equals("thứ 5"))
                        {
                            dateofweek = "thursday";
                        }
                        if (day.ToLower().Equals("thứ 6"))
                        {
                            dateofweek = "friday";
                        }
                        if (day.ToLower().Equals("thứ 7"))
                        {
                            dateofweek = "saturday";
                        }
                        if (day.ToLower().Equals("chủ nhật"))
                        {
                            dateofweek = "sunday";
                        }
                        scheduleRequests.Add(new ScheduleRequest
                        {
                            DateOfWeek = dateofweek,
                            SlotId = slotId,
                        });
                    }
                    catch (Exception ex)
                    {
                        rows.Add(new RowInsertResponse
                        {
                            Index = rq.Index,
                            Messsage = $" index {rq.Index} Không tìm thấy slot như vậy , slot có thời gian như vậy không tồn tại",
                            IsSucess = false
                        });
                        break;
                    }

                }
                var check = scheduleRequests.Select(x => x.DateOfWeek).Any(x => x.Equals(""));
                if (check)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"không tồn tại thứ trong tuần có tên như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                string format = "dd/MM/yyyy";

                var date = DateTime.TryParseExact(rq.StartDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                    ? (DateTime?)parsedDate
                    : DateTime.Parse(rq.StartDate);
                var doW = date.Value.DayOfWeek.ToString();
                var isExist = scheduleRequests.Select(x => x.DateOfWeek).Any(x => x.ToLower().Trim().Equals(doW.ToLower().Trim()));
                if (!isExist)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Ngày bắt đầu không trùng vào các thứ trong lịch học",
                        IsSucess = false
                    });
                    continue;
                }
                if (date.Value < DateTime.Now)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Ngày bắt đầu không thể là ngày trong quá khứ",
                        IsSucess = false
                    });
                    continue;
                }
                var roomLec = await GetRoomAndLecturer(new FilterLecturerRequest
                {
                    CourseId = courseId.ToString(),
                    Schedules = scheduleRequests,
                    StartDate = date,
                });
                if (roomLec.Lecturer == null && roomLec.Room == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy giáo viên phù hợp , phòng phù hợp với lịch như như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (roomLec.Lecturer == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy giáo viên phù hợp với lịch như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (roomLec.Room == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy phòng phù hợp với lịch như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var resultCheck = await CheckValidateSchedule(new FilterLecturerRequest
                {
                    CourseId = courseId.ToString(),
                    Schedules = scheduleRequests,
                    StartDate = date,
                }, roomLec.Lecturer.LectureId.ToString(), roomLec.Room.Id.ToString());
                if (resultCheck.Equals("FBRL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng cả giáo viên và phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBR"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng giảng viên nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var myRequest = new CreateClassRequest
                {
                    ClassCode = await AutoCreateClassCode(courseId.ToString()),
                    CourseId = courseId.Value,
                    LecturerId = roomLec.Lecturer.LectureId,
                    LeastNumberStudent = rq.LeastNumberStudent,
                    LimitNumberStudent = rq.LimitNumberStudent,
                    Method = rq.Method,
                    ScheduleRequests = scheduleRequests,
                    StartDate = date.Value,
                    RoomId = roomLec.Room.Id,
                };
                var isSuccess = await CreateNewClass(myRequest);
                if (!isSuccess.Success)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Có lỗi trong quá trình insert {myRequest.ClassCode}",
                        IsSucess = false
                    });
                    continue;
                }
                rows.Add(new RowInsertResponse
                {
                    Index = rq.Index,
                    IsSucess = true,
                    Messsage = $"ClassCode : {myRequest.ClassCode} , LecturerName : {roomLec.Lecturer.FullName}, Room : {roomLec.Room.Name}",
                    SuccessfulInformation = new SuccessfulInformation
                    {
                        ClassCode = myRequest.ClassCode,
                        LecturerName = roomLec.Lecturer.FullName,
                        RoomName = roomLec.Room.Name,
                        Times = rq.ScheduleRequests.Select(x => x.Schedule + ":" + x.Slot).ToList(),
                        StartDate = date.Value,
                    }
                });
            }
            response.RowInsertResponse = rows;
            foreach (var row in rows)
            {
                if (!row.IsSucess)
                {
                    var requestx = request.Single(x => x.Index == row.Index);
                    row.CreateClass = (await GenerateCreateClassFail(requestx));
                }
            }
            var succ = response.RowInsertResponse.Where(x => x.IsSucess).Count();
            var fail = response.RowInsertResponse.Where(x => !x.IsSucess).Count();
            response.SuccessRow = succ;
            response.FailureRow = fail;
            return response;
        }
        private async Task<string> CheckValidateSchedule(FilterLecturerRequest request, string LecturerId, string RoomId)
        {
            List<ScheduleRequest> scheduleRequests = request.Schedules;
            List<string> daysOfWeek = new List<string>();
            foreach (ScheduleRequest scheduleRequest in scheduleRequests)
            {
                daysOfWeek.Add(scheduleRequest.DateOfWeek);
            }
            List<DayOfWeek> convertedDateOfWeek = new List<DayOfWeek>();
            foreach (var dayOfWeek in daysOfWeek)
            {
                if (dayOfWeek.ToLower().Equals("sunday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Sunday);
                }
                if (dayOfWeek.ToLower().Equals("monday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Monday);
                }
                if (dayOfWeek.ToLower().Equals("tuesday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Tuesday);
                }
                if (dayOfWeek.ToLower().Equals("wednesday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Wednesday);
                }
                if (dayOfWeek.ToLower().Equals("thursday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Thursday);
                }
                if (dayOfWeek.ToLower().Equals("friday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Friday);
                }
                if (dayOfWeek.ToLower().Equals("saturday"))
                {
                    convertedDateOfWeek.Add(DayOfWeek.Saturday);
                }
            }
            var coursex = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()));
            if (coursex == null)
            {
                throw new BadHttpRequestException("không thấy lớp hợp lệ", StatusCodes.Status400BadRequest);
            }
            int numberOfSessions = coursex.NumberOfSession;
            int scheduleAdded = 0;
            DateTime startDatex = request.StartDate.Value;
            while (scheduleAdded < numberOfSessions)
            {
                if (convertedDateOfWeek.Contains(startDatex.DayOfWeek))
                {

                    scheduleAdded++;
                }
                startDatex = startDatex.AddDays(1);
            }
            var endDate = startDatex;
            List<ScheduleRequest> schedules = request.Schedules;
            List<ConvertScheduleRequest> convertSchedule = new List<ConvertScheduleRequest>();
            foreach (var schedule in schedules)
            {
                var doW = 1;
                if (schedule.DateOfWeek.ToLower().Equals("sunday"))
                {
                    doW = 1;
                }
                if (schedule.DateOfWeek.ToLower().Equals("monday"))
                {
                    doW = 2;
                }
                if (schedule.DateOfWeek.ToLower().Equals("tuesday"))
                {
                    doW = 4;
                }
                if (schedule.DateOfWeek.ToLower().Equals("wednesday"))
                {
                    doW = 8;
                }
                if (schedule.DateOfWeek.ToLower().Equals("thursday"))
                {
                    doW = 16;
                }
                if (schedule.DateOfWeek.ToLower().Equals("friday"))
                {
                    doW = 32;
                }
                if (schedule.DateOfWeek.ToLower().Equals("saturday"))
                {
                    doW = 64;
                }
                convertSchedule.Add(new ConvertScheduleRequest
                {
                    DateOfWeek = doW,
                    SlotId = schedule.SlotId,
                });
            }
            var allSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(include: x => x.Include(x => x.Class));
            allSchedule = allSchedule.Where(x => (x.Date < endDate && x.Date >= request.StartDate)).ToList();
            List<Schedule> result = new List<Schedule>();
            foreach (var convert in convertSchedule)
            {
                var newFilter = allSchedule.Where(x => (x.DayOfWeek == convert.DateOfWeek && x.SlotId.ToString().Equals(convert.SlotId.ToString()))).ToList();
                if (newFilter != null)
                {
                    result.AddRange(newFilter);
                }
            }
            var rx1 = result.Where(x => (x.RoomId.ToString().Equals(RoomId) && x.Class.LecturerId.ToString().Equals(LecturerId))).ToList();
            var rx2 = result.Where(x => x.SubLecturerId != null).Where(x => x.SubLecturerId.ToString().Equals(LecturerId)).Where(x => x.RoomId.ToString().Equals(RoomId)).ToList();
            if ((rx1 != null && rx1.Count > 0) || (rx2 != null && rx2.Count > 0))
            {
                return "FBRL";
            }
            var rs = result.Where(x => x.RoomId.ToString().Equals(RoomId)).ToList();
            if (rs != null && rs.Count > 0)
            {
                return "FBR";
            }
            var rs1 = result.Where(x => (x.Class.LecturerId.ToString().Equals(LecturerId))).ToList();
            var rs2 = result.Where(x => x.SubLecturerId != null).Where(x => x.SubLecturerId.ToString().Equals(LecturerId)).ToList();
            if ((rs1 != null && rs1.Count > 0) || (rs2 != null && rs2.Count > 0))
            {
                return "FBL";
            }
            return "S";
        }
        public async Task<ClassFromClassCode> GetClassFromClassCode(string classCode)
        {
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.ClassCode.Equals(classCode), include: x => x.Include(x => x.Course));
            if (classCode == null)
            {
                return new ClassFromClassCode();
            }
            var course = classx.Course;
            StaffCourseResponse staffCourseResponse = new StaffCourseResponse
            {
                AddedDate = course.AddedDate,
                Id = course.Id,
                Image = course.Image,
                MainDescription = course.MainDescription,
                MaxYearOldsStudent = course.MaxYearOldsStudent,
                MinYearOldsStudent = course.MinYearOldsStudent,
                Name = course.Name,
                NumberOfSession = course.NumberOfSession,
                //Price = course.Price,
                UpdateDate = course.UpdateDate,
                Status = string.Empty,
            };
            var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == classx.LecturerId, include: x => x.Include(x => x.LecturerField));
            var lecturerResponse = new LecturerResponse
            {
                Email = lecturer.Email,
                FullName = lecturer.FullName,
                AvatarImage = lecturer.AvatarImage,
                DateOfBirth = lecturer.DateOfBirth,
                Gender = lecturer.Gender,
                LecturerField = lecturer.LecturerField.Name,
                Phone = lecturer.Phone,
                Role = "Lecturer",
                LectureId = lecturer.Id,
            };
            ClassFromClassCode classFromClassCode = new ClassFromClassCode
            {
                AddedDate = classx.AddedDate,
                EndDate = classx.EndDate,
                Image = classx.Image,
                LecturerResponse = lecturerResponse,
                LeastNumberStudent = classx.LeastNumberStudent,
                LimitNumberStudent = classx.LimitNumberStudent,
                Method = classx.Method,
                StaffCourseResponse = staffCourseResponse,
                StartDate = classx.StartDate,
                Status = classx.Status,
                Video = classx.Video,
            };
            return classFromClassCode;
        }
        private async Task<CreateClassResponse> GenerateCreateClassFail(CreateClassesRequest rq)
        {
            Guid? syllabusId = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.SubjectCode.Trim().ToLower().Equals(rq.CourseCode.Trim().ToLower()), selector: x => x.Id);
            Guid? courseId = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syllabusId, selector: x => x.Id);
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syllabusId);
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.SubjectCode.Trim().ToLower().Equals(rq.CourseCode.Trim().ToLower()));
            MyCourseResponse myCourseResponse = null;
            if (courseId != null && courseId != Guid.Empty)
            {
                myCourseResponse = new MyCourseResponse { CourseId = courseId.Value, CourseName = course.Name, SubjectCode = syllabus.SubjectCode };
            }
            var schedules = rq.ScheduleRequests;
            List<ScheduleRequest> scheduleRequests = new List<ScheduleRequest>();
            foreach (var schedule in schedules)
            {
                try
                {
                    string day = schedule.Schedule; // Thứ 2
                    string timeRange = schedule.Slot.Trim(); // 15:00 - 20:00
                    string startTime = timeRange.Substring(0, timeRange.IndexOf("-"));
                    var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Trim().Equals(startTime.Trim()), selector: x => x.Id);
                    if (slotId == null || slotId == Guid.Empty)
                    {
                        break;
                    }
                    string dateofweek = "";
                    if (day.ToLower().Equals("thứ 2"))
                    {
                        dateofweek = "monday";
                    }
                    if (day.ToLower().Equals("thứ 3"))
                    {
                        dateofweek = "tuesday";
                    }
                    if (day.ToLower().Equals("thứ 4"))
                    {
                        dateofweek = "wednesday";
                    }
                    if (day.ToLower().Equals("thứ 5"))
                    {
                        dateofweek = "thursday";
                    }
                    if (day.ToLower().Equals("thứ 6"))
                    {
                        dateofweek = "friday";
                    }
                    if (day.ToLower().Equals("thứ 7"))
                    {
                        dateofweek = "saturday";
                    }
                    if (day.ToLower().Equals("chủ nhật"))
                    {
                        dateofweek = "sunday";
                    }
                    var slot = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(slotId.ToString()));
                    scheduleRequests.Add(new ScheduleRequest
                    {
                        DateOfWeek = dateofweek,
                        SlotId = slotId,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                    });
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            string format = "dd/MM/yyyy";

            var date = DateTime.TryParseExact(rq.StartDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                ? (DateTime?)parsedDate
                : DateTime.Parse(rq.StartDate);
            CreateClassResponse createClassResponse = new CreateClassResponse
            {
                ClassCode = null,
                CourseId = courseId,
                LecturerId = null,
                LeastNumberStudent = rq.LeastNumberStudent,
                LimitNumberStudent = rq.LimitNumberStudent,
                Method = rq.Method,
                RoomId = null,
                ScheduleRequests = scheduleRequests,
                StartDate = date.Value.AddHours(7),
                MyCourseResponse = myCourseResponse,
            };
            return createClassResponse;
        }
        public async Task<InsertClassesResponse> InsertClassesV2(List<CreateClassesRequest> request)
        {
            if (request == null)
            {
                throw new BadHttpRequestException("Request không có", StatusCodes.Status400BadRequest);
            }
            InsertClassesResponse response = new InsertClassesResponse
            {
                SuccessRow = 0,
                FailureRow = 0,

            };
            List<RowInsertResponse> rows = new List<RowInsertResponse>();
            foreach (var rq in request)
            {
                Guid? syllabusId = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.SubjectCode.Trim().ToLower().Equals(rq.CourseCode.Trim().ToLower()), selector: x => x.Id);
                Guid? courseId = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syllabusId, selector: x => x.Id);
                if (syllabusId == Guid.Empty || courseId == Guid.Empty)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Không tìm thấy khóa học có mã như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LeastNumberStudent >= rq.LimitNumberStudent)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa không thể nhỏ hơn số lượng tối thiểu",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LimitNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LeastNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối thiểu phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (rq.LimitNumberStudent > 30)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải nhỏ hơn 30",
                        IsSucess = false
                    });
                    continue;
                }
                var schedules = rq.ScheduleRequests;
                List<ScheduleRequest> scheduleRequests = new List<ScheduleRequest>();
                foreach (var schedule in schedules)
                {
                    try
                    {
                        string day = schedule.Schedule; // Thứ 2
                        string timeRange = schedule.Slot.Trim(); // 15:00 - 20:00
                        string startTime = timeRange.Substring(0, timeRange.IndexOf("-"));
                        var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Trim().Equals(startTime.Trim()), selector: x => x.Id);
                        if (slotId == null || slotId == Guid.Empty)
                        {
                            rows.Add(new RowInsertResponse
                            {
                                Index = rq.Index,
                                Messsage = $" index {rq.Index} Không tìm thấy slot như vậy , slot có thời gian {timeRange} không tồn tại",
                                IsSucess = false
                            });
                            break;
                        }
                        string dateofweek = "";
                        if (day.ToLower().Equals("thứ 2"))
                        {
                            dateofweek = "monday";
                        }
                        if (day.ToLower().Equals("thứ 3"))
                        {
                            dateofweek = "tuesday";
                        }
                        if (day.ToLower().Equals("thứ 4"))
                        {
                            dateofweek = "wednesday";
                        }
                        if (day.ToLower().Equals("thứ 5"))
                        {
                            dateofweek = "thursday";
                        }
                        if (day.ToLower().Equals("thứ 6"))
                        {
                            dateofweek = "friday";
                        }
                        if (day.ToLower().Equals("thứ 7"))
                        {
                            dateofweek = "saturday";
                        }
                        if (day.ToLower().Equals("chủ nhật"))
                        {
                            dateofweek = "sunday";
                        }
                        var slot = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(slotId.ToString()));
                        scheduleRequests.Add(new ScheduleRequest
                        {
                            DateOfWeek = dateofweek,
                            SlotId = slotId,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,
                        });
                    }
                    catch (Exception ex)
                    {
                        rows.Add(new RowInsertResponse
                        {
                            Index = rq.Index,
                            Messsage = $" index {rq.Index} Không tìm thấy slot như vậy , slot có thời gian như vậy không tồn tại",
                            IsSucess = false
                        });
                        break;
                    }

                }
                var check = scheduleRequests.Select(x => x.DateOfWeek).Any(x => x.Equals(""));
                if (check)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"không tồn tại thứ trong tuần có tên như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                string format = "dd/MM/yyyy";

                var date = DateTime.TryParseExact(rq.StartDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                    ? (DateTime?)parsedDate
                    : DateTime.Parse(rq.StartDate);
                var doW = date.Value.DayOfWeek.ToString();
                var isExist = scheduleRequests.Select(x => x.DateOfWeek).Any(x => x.ToLower().Trim().Equals(doW.ToLower().Trim()));
                if (!isExist)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Ngày bắt đầu không trùng vào các thứ trong lịch học",
                        IsSucess = false
                    });
                    continue;
                }
                if (date.Value < DateTime.Now)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Ngày bắt đầu không thể là ngày trong quá khứ",
                        IsSucess = false
                    });
                    continue;
                }
                var roomLec = await GetRoomAndLecturer(new FilterLecturerRequest
                {
                    CourseId = courseId.ToString(),
                    Schedules = scheduleRequests,
                    StartDate = date.Value.AddHours(7),
                });
                if (roomLec.Lecturer == null && roomLec.Room == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy giáo viên phù hợp , phòng phù hợp với lịch như như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (roomLec.Lecturer == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy giáo viên phù hợp với lịch như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (roomLec.Room == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $" index {rq.Index} Không tìm thấy phòng phù hợp với lịch như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var resultCheck = await CheckValidateSchedule(new FilterLecturerRequest
                {
                    CourseId = courseId.ToString(),
                    Schedules = scheduleRequests,
                    StartDate = date,
                }, roomLec.Lecturer.LectureId.ToString(), roomLec.Room.Id.ToString());
                if (resultCheck.Equals("FBRL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng cả giáo viên và phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBR"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng giảng viên nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var myRequest = new CreateClassRequest
                {
                    ClassCode = await AutoCreateClassCode(courseId.ToString()),
                    CourseId = courseId.Value,
                    LecturerId = roomLec.Lecturer.LectureId,
                    LeastNumberStudent = rq.LeastNumberStudent,
                    LimitNumberStudent = rq.LimitNumberStudent,
                    Method = rq.Method,
                    ScheduleRequests = scheduleRequests,
                    StartDate = date.Value.AddHours(7),
                    RoomId = roomLec.Room.Id,
                };

                //    rows.Add(new RowInsertResponse
                //    {
                //        Index = rq.Index,
                //        IsSucess = true,
                //        Messsage = $"ClassCode : {myRequest.ClassCode} , LecturerName : {roomLec.Lecturer.FullName}, Room : {roomLec.Room.Name}",
                //        SuccessfulInformation = new SuccessfulInformation
                //        {
                //            ClassCode = myRequest.ClassCode,
                //            LecturerName = roomLec.Lecturer.FullName,
                //            RoomName = roomLec.Room.Name,
                //            Times = rq.ScheduleRequests.Select(x => x.ScheduleTime).ToList(),
                //            StartDate = date.Value,
                //        },
                //        //CreateClass = await GenerateCreateClassFail(rq),
                //    });
                var newRIR = new RowInsertResponse
                {
                    Index = rq.Index,
                    IsSucess = true,
                    Messsage = $"LecturerName : {roomLec.Lecturer.FullName}, Room : {roomLec.Room.Name}",
                    SuccessfulInformation = new SuccessfulInformation
                    {
                        ClassCode = myRequest.ClassCode,
                        LecturerName = roomLec.Lecturer.FullName,
                        RoomName = roomLec.Room.Name,
                        Times = rq.ScheduleRequests.Select(x => x.Schedule + ":" + x.Slot).ToList(),
                        StartDate = date,
                    },
                };
                var createClass = await GenerateCreateClassFail(rq);
                createClass.ClassCode = myRequest.ClassCode;
                createClass.LecturerId = myRequest.LecturerId;
                createClass.RoomId = myRequest.RoomId;
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == myRequest.LecturerId);
                var room = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == myRequest.RoomId);
                var courseName = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == myRequest.CourseId);
                var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == courseName.SyllabusId);
                var lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                    Role = "Lecuterer",
                };
                var roomResponse = new RoomResponse
                {
                    Capacity = room.Capacity,
                    Floor = room.Floor.Value,
                    LinkUrl = room.LinkURL,
                    Name = room.Name,
                    RoomId = room.Id,
                    Status = room.Status,
                };
                createClass.RoomResponse = roomResponse;
                createClass.LecturerResponse = lecturerResponse;
                createClass.MyCourseResponse = new MyCourseResponse
                {
                    CourseId = courseName.Id,
                    CourseName = courseName.Name,
                    SubjectCode = syllabus.SubjectCode,

                };
                createClass.ScheduleRequests = scheduleRequests;
                newRIR.CreateClass = createClass;
                rows.Add(newRIR);
            }
            response.RowInsertResponse = rows;
            foreach (var row in rows)
            {
                if (!row.IsSucess)
                {
                    var requestx = request.Single(x => x.Index == row.Index);
                    row.CreateClass = (await GenerateCreateClassFail(requestx));
                }
            }
            var succ = response.RowInsertResponse.Where(x => x.IsSucess).Count();
            var fail = response.RowInsertResponse.Where(x => !x.IsSucess).Count();
            response.SuccessRow = succ;
            response.FailureRow = fail;
            return response;
        }
        private async Task<CreateClassesRequest> FromCToC(CreateClassResponse request)
        {
            var courseCode = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                selector: x => x.Syllabus!.SubjectCode,
                predicate: x => x.Id == request.CourseId);

            List<ScheduleRequestV2> requestV2s = new List<ScheduleRequestV2>();
            var schedules = request.ScheduleRequests;
            foreach (var sch in schedules)
            {
                var dateOfWeek = sch.DateOfWeek;
                var dow = "";
                if (dateOfWeek.ToLower().EndsWith("monday"))
                {
                    dow = "Thứ 2";
                }
                if (dateOfWeek.ToLower().EndsWith("tuesday"))
                {
                    dow = "Thứ 3";
                }
                if (dateOfWeek.ToLower().EndsWith("wednesday"))
                {
                    dow = "Thứ 4";
                }
                if (dateOfWeek.ToLower().EndsWith("thursday"))
                {
                    dow = "Thứ 5";
                }
                if (dateOfWeek.ToLower().EndsWith("friday"))
                {
                    dow = "Thứ 6";
                }
                if (dateOfWeek.ToLower().EndsWith("saturday"))
                {
                    dow = "Thứ 7";
                }
                if (dateOfWeek.ToLower().EndsWith("sunday"))
                {
                    dow = "Chủ Nhật";
                }
                var slot = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id == sch.SlotId);
                requestV2s.Add(new ScheduleRequestV2
                {
                    Schedule = dow,
                    Slot = slot.StartTime + "-" + slot.EndTime,
                });
            }
            CreateClassesRequest createClassesRequest = new CreateClassesRequest
            {
                CourseCode = courseCode,
                Index = 0,
                LeastNumberStudent = request.LeastNumberStudent.Value,
                LimitNumberStudent = request.LimitNumberStudent.Value,
                Method = request.Method,
                StartDate = request.StartDate.ToString(),
                ScheduleRequests = requestV2s
            };
            return createClassesRequest;
        }
        public async Task<InsertClassesResponse> InsertClassesSave(InsertClassesResponse request)
        {
            var listRq = request.RowInsertResponse;
            InsertClassesResponse response = new InsertClassesResponse
            {
                SuccessRow = 0,
                FailureRow = 0,

            };
            List<RowInsertResponse> rows = new List<RowInsertResponse>();
            List<CreateClassesRequest> createClassesRequest = new List<CreateClassesRequest>();
            var changeList = request.RowInsertResponse;
            foreach (var c in changeList)
            {
                CreateClassesRequest changed = await FromCToC(c.CreateClass);
                changed.Index = c.Index;
                createClassesRequest.Add(changed);
            }
            foreach (var rq in listRq)
            {
                var createClass = rq.CreateClass;
                CreateClassRequest createClassRequest = new CreateClassRequest
                {
                    ClassCode = await AutoCreateClassCode(createClass.CourseId.ToString()),
                    CourseId = createClass.CourseId.Value,
                    LeastNumberStudent = createClass.LeastNumberStudent.Value,
                    LecturerId = createClass.LecturerId.Value,
                    LimitNumberStudent = createClass.LimitNumberStudent.Value,
                    Method = createClass.Method,
                    RoomId = createClass.RoomId.Value,
                    ScheduleRequests = createClass.ScheduleRequests,
                    StartDate = createClass.StartDate.Value,
                };
                if (createClass.LeastNumberStudent >= createClass.LimitNumberStudent.Value)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa không thể nhỏ hơn số lượng tối thiểu",
                        IsSucess = false
                    });
                    continue;
                }
                if (createClass.LimitNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (createClass.LeastNumberStudent < 0)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối thiểu phải lớn hơn 0",
                        IsSucess = false
                    });
                    continue;
                }
                if (createClass.LimitNumberStudent > 30)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Số lượng tối đa phải nhỏ hơn 30",
                        IsSucess = false
                    });
                    continue;
                }
                var resultCheck = await CheckValidateSchedule(new FilterLecturerRequest
                {
                    CourseId = createClass.CourseId.ToString(),
                    Schedules = createClass.ScheduleRequests,
                    StartDate = createClass.StartDate,
                }, createClassRequest.LecturerId.ToString(), createClassRequest.RoomId.ToString());
                if (resultCheck.Equals("FBRL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng cả giáo viên và phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBR"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = "Xuất hiện sự trùng giảng viên nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var isSuccess = await CreateNewClass(createClassRequest);
                if (!isSuccess.Success)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Messsage = $"Có lỗi trong quá trình insert {createClassRequest.ClassCode}",
                        IsSucess = false
                    });
                    continue;
                }
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == createClassRequest.LecturerId);
                var room = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == createClassRequest.RoomId);
                var cr = createClassesRequest.Single(x => x.Index == rq.Index);
                var newRIR = new RowInsertResponse
                {
                    Index = rq.Index,
                    IsSucess = true,
                    Messsage = $"ClassCode : {createClassRequest.ClassCode} , LecturerName : {lecturer.FullName}, Room : {room.Name}",
                    SuccessfulInformation = new SuccessfulInformation
                    {
                        ClassCode = createClass.ClassCode,
                        LecturerName = lecturer.FullName,
                        RoomName = room.Name,
                        Times = cr.ScheduleRequests.Select(x => x.Schedule + ":" + x.Slot).ToList(),
                        StartDate = createClass.StartDate.Value,
                    },
                };
                var createClassx = await GenerateCreateClassFail(cr);
                createClass.ClassCode = createClass.ClassCode;
                createClass.LecturerId = createClass.LecturerId;
                createClass.RoomId = createClass.RoomId;
                var lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                    Role = "Lecuterer",
                };
                var roomResponse = new RoomResponse
                {
                    Capacity = room.Capacity,
                    Floor = room.Floor.Value,
                    LinkUrl = room.LinkURL,
                    Name = room.Name,
                    RoomId = room.Id,
                    Status = room.Status,
                };
                createClassx.RoomResponse = roomResponse;
                createClassx.LecturerResponse = lecturerResponse;
                newRIR.CreateClass = createClassx;
                rows.Add(newRIR);
            }
            response.RowInsertResponse = rows;
            foreach (var row in rows)
            {
                if (!row.IsSucess)
                {
                    var requestx = createClassesRequest.Single(x => x.Index == row.Index);
                    row.CreateClass = (await GenerateCreateClassFail(requestx));
                }
            }
            var succ = response.RowInsertResponse.Where(x => x.IsSucess).Count();
            var fail = response.RowInsertResponse.Where(x => !x.IsSucess).Count();
            response.SuccessRow = succ;
            response.FailureRow = fail;
            return response;
        }
        #endregion
        #region thanh_lee_code
        public async Task<List<ClassWithSlotShorten>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent, PeriodTimeEnum time)
        {
            var classes = await FetchClasses(time);

            #region
            //For satisfy all key word

            //classes = keyWords == null || keyWords.Count() == 0
            //        ? classes
            //        : classes.Where(c => keyWords.All(k =>
            //        k == null ||
            //       c.Name.ToLower().Contains(k.ToLower()) ||
            //       c.StartDate.ToString().ToLower().Contains(k.ToLower()) ||
            //       c.EndDate.ToString().ToLower().Contains(k.ToLower()) ||
            //       c.Method.ToString().ToLower().Contains(k.ToLower()) || 
            //       c.Status.ToString().ToLower().Contains(k.ToLower()) ||
            //       (c.Address != null && (c.Address.City!.ToLower().Contains(k.ToLower()) || 
            //       c.Address.Street!.ToLower().Contains(k.ToLower()) || 
            //       c.Address.District!.ToLower().Contains(k.ToLower()))))).ToList();
            #endregion

            //For satisfy just one key word
            classes = keyWords == null || keyWords.Count() == 0
                    ? classes
                    : classes.Where(c => keyWords.Any(k =>
                    (k != null) &&
                    (c.ClassCode!.ToLower().Contains(k.ToLower()) ||
                    c.StartDate.ToString().ToLower().Contains(k.ToLower()) ||
                    c.EndDate.ToString().ToLower().Contains(k.ToLower()) ||
                    c.Method!.ToString().ToLower().Contains(k.ToLower()) ||
                    c.Status!.ToString().ToLower().Contains(k.ToLower()) ||
                    (c.City + c.District + c.Street).ToLower().Contains(k.ToLower())))).ToList();

            leastNumberStudent ??= 1;
            limitStudent ??= int.MaxValue;

            classes = classes.Where(c => c.LeastNumberStudent >= leastNumberStudent || c.LimitNumberStudent == limitStudent).ToList();
            var responses = classes.Select(c => _mapper.Map<ClassWithSlotShorten>(c)).ToList();

            foreach (var response in responses)
            {
                response.CoursePrice = await GetDynamicPrice(response.ClassId, true);
            }

            return responses;
        }

        public async Task<ClassResExtraInfor> GetClassByIdAsync(Guid id)
        {

            var cls = await _unitOfWork.GetRepository<Class>()
               .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
               .Include(x => x.Lecture!)
               .Include(x => x.StudentClasses));

            if (cls == null)
            {
                return default!;
            }

            cls.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == cls.CourseId,
                include: x => x.Include(x => x.Syllabus)
                .ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
                .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)));

            cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == id,
                include: x => x.Include(x => x.Slot).Include(x => x.Room)!);

            var response = _mapper.Map<ClassResExtraInfor>(cls);

            response.CoursePrice = await GetDynamicPrice(response.ClassId, true);

            return response;
        }

        public async Task<List<ClassWithSlotShorten>> GetClassesAsync(PeriodTimeEnum time)
        {
            #region

            //try
            //{
            //var deleteClasses = new List<Class>();
            //var deleteSyllabuses = new List<Syllabus>();
            //var deleteCourses = new List<Course>();            

            //var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync();

            //foreach (var syll in syllabuses)
            //{
            //    var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syll.Id);
            //    if (course == null)
            //    {
            //        deleteSyllabuses.Add(syll);
            //        continue;
            //    }
            //    var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId == course.Id);
            //    if (classes == null && !classes.Any())
            //    {
            //        deleteCourses.Add(course);
            //        deleteSyllabuses.Add(syll);
            //        continue;
            //    }
            //    int classCount = 0;
            //    foreach (var cls in classes)
            //    {
            //        var studentClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == cls.Id);
            //        if (studentClass == null && (cls.Status != ClassStatusEnum.LOCKED.ToString() || cls.Status != ClassStatusEnum.CANCELED.ToString()))
            //        {
            //            classCount++;
            //            deleteClasses.Add(cls);
            //        }
            //    }
            //    if (classCount == classes.Count())
            //    {
            //        deleteCourses.Add(course);
            //        deleteSyllabuses.Add(syll);
            //    }
            //}

            //var temps = new List<TempItemPrice>();
            //var course = await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.Classes));
            //foreach (var cou in course)
            //{
            //    var coursePrices = (await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
            //        selector: x => x.CoursePrices,
            //        predicate: x => x.Id == cou.Id))!.ToArray();

            //    foreach (var cls in cou.Classes)
            //    {
            //        double result = 0;

            //        coursePrices = coursePrices.OrderBy(x => x.EffectiveDate).ToArray();

            //        result = coursePrices.First().Price;
            //        for (int i = 1; i < coursePrices.Length; i++)
            //        {
            //            if (cls.AddedDate >= coursePrices[i].EffectiveDate)
            //            {
            //                result = coursePrices[i].Price;
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }

            //        temps.Add(new TempItemPrice
            //        {
            //            Id = Guid.NewGuid(),
            //            ClassId = cls.Id,
            //            CourseId = default,
            //            Price = result,
            //        });
            //    }

            //    coursePrices = coursePrices!.OrderByDescending(x => x.EffectiveDate).ToArray();

            //    var Couresult = coursePrices.Last().Price;

            //    var currentValidPrice = coursePrices.FirstOrDefault(x => x.EffectiveDate <= DateTime.Now)?.Price;

            //    if (currentValidPrice is not null)
            //    {
            //        Couresult = currentValidPrice.Value;
            //    }

            //    temps.Add(new TempItemPrice
            //    {
            //        Id = Guid.NewGuid(),
            //        ClassId = default,
            //        CourseId = cou.Id,
            //        Price = Couresult,
            //    });
            //}
            //var newEva = new List<Evaluate>();
            //var oldEva = new List<Evaluate>();
            //var newAtten = new List<Attendance>();
            //var oldAtten = new List<Attendance>();

            //var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse("C7EAB299-1511-4F61-B6D5-11BCBDCA650D"), include: x => x.Include(x => x.StudentClasses).Include(x => x.Schedules.OrderBy(sc => sc.Date)));
            //foreach (var stu in cls.StudentClasses)
            //{
            //    foreach (var sch in cls.Schedules)
            //    {
            //var oa = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == sch.Id && x.StudentId == stu.StudentId);
            //if (oa != null)
            //{

            //    oldAtten.Add(oa);
            //}
            //var oe = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == sch.Id && x.StudentId == stu.StudentId);
            //if (oe != null)
            //{

            //    oldEva.Add(oe);
            //}

            //newEva.Add(new Evaluate
            //{
            //    Id = Guid.NewGuid(),
            //    Status = null,
            //    Note = null,
            //    IsValid = true,
            //    StudentId = stu.StudentId,
            //    ScheduleId = sch.Id,
            //});

            //        newAtten.Add(new Attendance
            //        {
            //            Id = Guid.NewGuid(),
            //            IsPresent = null,
            //            IsPublic = true,
            //            IsValid = true,
            //            Note = null,
            //            StudentId = stu.StudentId,
            //            ScheduleId = sch.Id,
            //        });
            //    }
            //}

            //await _unitOfWork.GetRepository<Evaluate>().InsertRangeAsync(newEva);
            //await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(newAtten);
            //_unitOfWork.GetRepository<Evaluate>().DeleteRangeAsync(oldEva);
            //_unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(oldAtten);
            //_unitOfWork.Commit();


            //}
            //catch (Exception ex)
            //{
            //    var s = ex;
            //}

            //return default!;
            #endregion
            var classes = await FetchClasses(time);
            var responses = classes.Select(c => _mapper.Map<ClassWithSlotShorten>(c)).ToList();

            foreach (var res in responses)
            {
                res.CoursePrice = await GetDynamicPrice(res.ClassId, true);
            }

            return responses;
        }


        public async Task<List<ClassWithSlotShorten>> GetClassesByCourseIdAsync(Guid id, ClassStatusEnum status)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                include: x => x.Include(x => x.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
                .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)));
            if (course is null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == id,
                include: x => x.Include(x => x.Lecture!).Include(x => x.StudentClasses));

            foreach (var cls in classes)
            {
                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == cls.Id,
                include: x => x.Include(x => x.Slot).Include(x => x.Room)!);

                cls.Course = course;
            }
            status = status == ClassStatusEnum.DEFAULT ? ClassStatusEnum.UPCOMING : status;
            classes = classes.Where(cls => cls.Status == status.ToString()).ToList();

            var responses = classes.Select(cls => _mapper.Map<ClassWithSlotShorten>(cls)).ToList();

            foreach (var res in responses)
            {
                res.CoursePrice = await GetDynamicPrice(res.ClassId, true);
            }

            return responses;
        }

        private async Task<ICollection<Class>> FetchClasses(PeriodTimeEnum time)
        {
            var classes = new List<Class>();

            if (time != default)
            {
                classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => GetCurrentTime() <= x.StartDate && x.StartDate <= GetCurrentTime().AddDays((int)time),
                include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses))).ToList();
            }
            else
            {
                classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses))).ToList();
            }

            foreach (var cls in classes)
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                    predicate: x => x.Id == cls.CourseId);

                var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    predicate: x => x.Id == course.SyllabusId,
                    include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession))
                   .ThenInclude(ses => ses.SessionDescriptions!.OrderBy(ses => ses.Order)));

                var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                    orderBy: x => x.OrderBy(x => x.Date),
                    predicate: x => x.ClassId == cls.Id,
                    include: x => x.Include(x => x.Slot!).Include(x => x.Room!));

                cls.Course = course;
                cls.Schedules = schedules;
                cls.Course.Syllabus = syllabus;

            }

            return classes;
            #region
            //return await _unitOfWork.GetRepository<Class>()
            //   .GetListAsync(predicate: x => DateTime.Now <= x.StartDate && x.StartDate <= DateTime.Now.AddDays((int)time), include: x => x
            //   .Include(x => x.Lecture)
            //   .Include(x => x.Course!).ThenInclude(c => c.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
            //   .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)).ThenInclude(ses => ses.SessionDescriptions)
            //   .Include(x => x.StudentClasses)
            //   .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Slot)!
            //   .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Room)!);

            //return await _unitOfWork.GetRepository<Class>()
            //    .GetListAsync(include: x => x
            //    .Include(x => x.Lecture)
            //    .Include(x => x.Course!).ThenInclude(c => c.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
            //    .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)).ThenInclude(ses => ses.SessionDescriptions)
            //    .Include(x => x.StudentClasses)
            //    .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Slot)!
            //    .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Room)!);
            #endregion
        }

        public async Task<List<StudentInClass>> GetAllStudentInClass(string id)
        {
            var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(id) && !x.Status.Equals("Saved"),
               include: x => x.Include(x => x.Student)!.ThenInclude(x => x.Parent));


            if (students == null)
            {
                return default!;
            }

            return students.Select(x => _mapper.Map<StudentInClass>(x)).ToList();

        }

        public async Task ValidateScheduleOfClassesAsync(List<Guid> classIdList)
        {
            var classes = new List<Class>();

            foreach (var id in classIdList)
            {
                var cls = await _unitOfWork.GetRepository<Class>()
                   .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
                   .Include(x => x.Schedules)
                   .ThenInclude(s => s.Slot)
                   .Include(x => x.Schedules)
                   .ThenInclude(s => s.Room)!);

                classes.Add(cls);
            }

            for (int i = 0; i < classes.Count - 1; i++)
            {
                for (int j = i + 1; j < classes.Count; j++)
                {
                    CheckConflictSchedule(classes, i, j);
                }
            }
        }

        private void CheckConflictSchedule(List<Class> classes, int defaultIndex, int browserIndex)
        {
            var defaultSchedules = classes[defaultIndex].Schedules;
            var browserSchedules = classes[browserIndex].Schedules;

            foreach (var ds in defaultSchedules)
            {
                foreach (var bs in browserSchedules)
                {
                    if (ds.Date == bs.Date && ds.Slot!.StartTime == bs.Slot!.StartTime)
                    {
                        if (classes[defaultIndex].Id == classes[browserIndex].Id)
                        {
                            throw new BadHttpRequestException($"Bạn Đang Đăng Ký Lớp [{classes[defaultIndex].ClassCode}] Nhiều Hơn 2 Lần", StatusCodes.Status400BadRequest);
                        }

                        throw new BadHttpRequestException($"Lịch Của Lớp [{classes[defaultIndex].ClassCode}] Bị Trùng Thời Gian Bắt Đầu [{ds.Slot.StartTime}]" +
                        $" Với Lớp [{classes[browserIndex].ClassCode}] Hãy Chọn Lớp Bạn Mong Muốn Đăng Ký Nhất", StatusCodes.Status400BadRequest);
                    }
                }
            }
        }

        public async Task<List<ClassWithSlotOutSideResponse>> GetCurrentLectureClassesScheduleAsync()
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId == GetUserIdFromJwt() && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sche => sche.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sche => sche.Room).Include(x => x.Course!));

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Giáo Viên Chưa Được Phân Dạy Ở Bất Kỳ Lớp Nào", StatusCodes.Status400BadRequest);
            }
            var nearestClasses = classes.Where(cls => cls.Schedules.Any(sc => sc.Date.Date == GetCurrentTime().Date || sc.Date.Date == GetCurrentTime().AddDays(1).Date)).ToList();

            if (!nearestClasses.Any())
            {
                return new List<ClassWithSlotOutSideResponse>();
            }

            return await GenerateClassSchedule(nearestClasses);
        }

        private async Task<List<ClassWithSlotOutSideResponse>> GenerateClassSchedule(List<Class> currentClasses)
        {
            var responses = new List<ClassWithSlotOutSideResponse>();

            TimeOnly currentTime = TimeOnly.FromDateTime(GetCurrentTime());

            foreach (var cls in currentClasses)
            {
                var currentSchedule = new Schedule();

                if (currentTime.Hour < 21 && currentTime.Hour >= 0)
                {
                    var checkingSchedule = cls.Schedules.SingleOrDefault(sc => sc.Date.Date == GetCurrentTime().Date);
                    if (checkingSchedule == null)
                    {
                        continue;
                    }

                    if (currentTime > TimeOnly.Parse(checkingSchedule!.Slot!.EndTime))
                    {
                        continue;
                    }
                    currentSchedule = checkingSchedule;
                }
                else if (currentTime.Hour >= 21 || currentTime.Hour <= 24)
                {
                    var checkingSchedule = cls.Schedules.SingleOrDefault(sc => sc.Date.Date == GetCurrentTime().AddDays(1).Date);
                    if (checkingSchedule == null)
                    {
                        continue;
                    }
                    currentSchedule = checkingSchedule;

                }

                var response = _mapper.Map<ClassWithSlotOutSideResponse>(cls);
                response.ScheduleId = currentSchedule!.Id;
                response.DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(currentSchedule.DayOfWeek)[0].ToString();
                response.Date = currentSchedule.Date;
                response.NoSession = cls.Schedules.ToList().IndexOf(currentSchedule) + 1;
                response.Slot = SlotCustomMapper.fromSlotToSlotForLecturerResponse(currentSchedule.Slot!);
                response.Room = RoomCustomMapper.fromRoomToRoomResponse(currentSchedule.Room!);
                response.SlotOrder = StringHelper.GetSlotNumber(currentSchedule.Slot!.StartTime);

                response.CoursePrice = await GetDynamicPrice(cls.Id, true);

                responses.Add(response);
            }
            responses = responses.OrderBy(res => res.Date).ThenBy(res => res.Slot.StartTime).ToList();
            return responses;
        }

        public async Task<ScheduleWithAttendanceResponse> GetAttendanceOfClassesInDateAsync(Guid classId, DateTime date)
        {

            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.LecturerId == GetUserIdFromJwt() && x.Id == classId,
             include: x => x.Include(x => x.Lecture).Include(x => x.Course)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại Hoặc Lớp Học Không Thuộc Phân Công Của Giáo Viên Hiện Tại", StatusCodes.Status400BadRequest);
            }

            if (cls.Status!.Trim() != ClassStatusEnum.PROGRESSING.ToString())
            {

                throw new BadHttpRequestException("Chỉ Có Thể Truy Suất Danh Sách Điểm Danh Của Các Lớp Học Đang Diễn Ra," +
                    $" Lớp [{cls.ClassCode}] [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status)}]", StatusCodes.Status400BadRequest);
            }

            return await GenerateSchedultAttendance(cls, date);
        }

        private async Task<ScheduleWithAttendanceResponse> GenerateSchedultAttendance(Class cls, DateTime date)
        {
            var schedule = cls.Schedules.SingleOrDefault(sc => sc.Date.Date == date.Date);

            if (schedule == null)
            {
                return new ScheduleWithAttendanceResponse();
            }
            var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(
                predicate: x => x.ScheduleId == schedule.Id && x.IsPublic == true,
                include: x => x.Include(x => x.Student)!);

            var savedStudentClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == cls.Id && x.SavedTime != null);
            if (savedStudentClass != null && savedStudentClass.Any())
            {
                attendances = attendances.Where(att => !savedStudentClass.Select(x => x.StudentId).Contains(att.StudentId)).ToList();
            }

            schedule.Attendances = attendances;


            var response = _mapper.Map<ScheduleWithAttendanceResponse>(schedule);
            response.ClassSubject = cls.Course!.SubjectName;
            response.ClassName = cls.Course!.Name;
            response.Method = cls.Method;
            response.ClassCode = cls.ClassCode;

            return response;
        }

        #region
        private async Task<List<Class>> FindSuitableClasses(Guid classId, List<Guid> studentIdList, Class currentClass)
        {
            var allStudentRegisteredClass = new List<Class>();

            foreach (Guid id in studentIdList)
            {
                var studentRegisteredClass = await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.Id != classId && x.StudentClasses.Any(sc => sc.StudentId == id) && (x.Status != ClassStatusEnum.COMPLETED.ToString() || x.Status != ClassStatusEnum.CANCELED.ToString()),
                include: x => x.Include(x => x.Schedules).ThenInclude(sc => sc.Slot)!);

                if (studentRegisteredClass != null)
                {
                    allStudentRegisteredClass.AddRange(studentRegisteredClass);
                }
            }

            var allCourseClass = await _unitOfWork.GetRepository<Class>()
               .GetListAsync(predicate: x => x.CourseId == currentClass.CourseId && x.Status == ClassStatusEnum.UPCOMING.ToString(),
               include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
               .Include(x => x.Lecture)
               .Include(x => x.StudentClasses));

            var suitableClasses = allCourseClass.Where(acc =>
                !acc.Schedules.Any(accsc =>
                    allStudentRegisteredClass.Any(asrc =>
                        asrc.Schedules.Any(asrcsc =>
                            asrcsc.Slot?.StartTime == accsc.Slot?.StartTime))))
                .ToList();
            #region
            //var suitableClasses = new List<Class>();

            //foreach (var courCls in allCourseClass)
            //{
            //    bool hasOverlap = false;

            //    foreach (var courSchedule in courCls.Schedules)
            //    {
            //        foreach (var currCls in classes)
            //        {
            //            foreach (var schedule in currCls.Schedules)
            //            {
            //                if (schedule.Slot?.StartTime == courSchedule.Slot?.StartTime)
            //                {
            //                    hasOverlap = true;
            //                    break; // No need to check further, as we found an overlap
            //                }
            //            }

            //            if (hasOverlap)
            //                break; // No need to check further, as we found an overlap
            //        }

            //        if (hasOverlap)
            //            break; // No need to check further, as we found an overlap
            //    }

            //    if (!hasOverlap)
            //        suitableClasses.Add(courCls);
            //}
            #endregion

            suitableClasses = suitableClasses
           .Where(cls => cls.StudentClasses.Count + studentIdList.Count <= cls.LimitNumberStudent)
           .Where(cls => cls.Id != currentClass.Id).ToList();
            return suitableClasses;
        }

        private void ValidateFindSuitableRequestClass(Guid classId, List<Guid> studentIdList, Class currentClass)
        {
            if (currentClass == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            if (!studentIdList.All(stu => currentClass.StudentClasses.Any(sc => sc.StudentId == stu)))
            {
                throw new BadHttpRequestException($"Id [{string.Join(", ", studentIdList)}] Của Các Học Sinh Không Thuộc Về Lớp [{currentClass.ClassCode}]", StatusCodes.Status400BadRequest);
            }

            if (currentClass.Status != ClassStatusEnum.UPCOMING.ToString() && currentClass.Status != ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException("Chỉ Có Thể Chuyển Học Sinh Thuộc Lớp Sắp Bắt Đầu Hoặc Đã Hủy," +
                    $" Lớp [{currentClass.ClassCode}] [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(currentClass.Status!)}], Không Thể Chuyển Lớp",
                      StatusCodes.Status400BadRequest);
            }

            var changedId = studentIdList.FirstOrDefault(id => currentClass.StudentClasses.FirstOrDefault(stu => stu.Id == id)!.CanChangeClass == false);

            if (changedId != default)
            {
                throw new BadHttpRequestException($"Id [{changedId}] Của Học Sinh Đã Được Chuyển Lớp", StatusCodes.Status400BadRequest);
            }
        }
        #endregion
        public async Task<string> ChangeStudentClassAsync(Guid fromClassId, Guid toClassId, List<Guid> studentIdList)
        {
            try
            {
                var fromClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == fromClassId,
                include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

                var toClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == toClassId,
               include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

                ValidateChangeClassRequest(fromClassId, studentIdList, fromClass, true);
                ValidateChangeClassRequest(toClassId, studentIdList, toClass, false);

                foreach (Guid id in studentIdList)
                {
                    var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                    include: x => x.Include(x => x.StudentClasses.Where(sc => sc.ClassId != fromClassId)).ThenInclude(sc => sc.Class!).ThenInclude(cls => cls.Schedules)!
                    .ThenInclude(x => x.Slot!).Include(x => x.Parent));

                    ValidateStudentChangeClassRequest(toClassId, studentIdList, fromClass, toClass, id, student);

                    await ChangeClassProgress(fromClass, toClass, student);

                }

                _unitOfWork.Commit();
                return "Đổi Lớp Thành Công";
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
        }

        private async Task ChangeClassProgress(Class fromClass, Class toClass, Student student)
        {
            var oldStudentClass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.StudentId == student.Id && x.ClassId == fromClass.Id);
            var newStudentClass = new StudentClass
            {
                Id = new Guid(),
                ClassId = toClass.Id,
                StudentId = student.Id,
            };

            var oldStudentAttendance = new List<Attendance>();
            var oldStudentEvaluate = new List<Evaluate>();

            foreach (var schedule in fromClass.Schedules)
            {
                var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.StudentId == student.Id && x.ScheduleId == schedule.Id);
                var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.StudentId == student.Id && x.ScheduleId == schedule.Id);
                if (attendance != null)
                {
                    oldStudentAttendance.Add(attendance);
                }
                if (evaluate != null)
                {
                    oldStudentEvaluate.Add(evaluate);
                }
            }


            var newStudentAttendance = new List<Attendance>();
            var newStudentEvaluate = new List<Evaluate>();
            foreach (var schedule in toClass.Schedules)
            {
                newStudentAttendance.Add(new Attendance
                {
                    Id = Guid.NewGuid(),
                    IsPublic = true,
                    IsPresent = default,
                    StudentId = student.Id,
                    ScheduleId = schedule.Id,
                });

                newStudentEvaluate.Add(new Evaluate
                {
                    Id = Guid.NewGuid(),
                    Status = null,
                    Note = null,
                    IsPublic = true,
                    StudentId = student.Id,
                    ScheduleId = schedule.Id,
                });
            }


            var newNotification = new Notification
            {
                Id = new Guid(),
                Title = NotificationMessageContant.ChangeClassTitle,
                Body = NotificationMessageContant.ChangeClassBody(fromClass.ClassCode!, toClass.ClassCode!,
                      student.FullName!, fromClass.Status == ClassStatusEnum.UPCOMING.ToString() ? ChangeClassReasoneEnum.REQUEST : ChangeClassReasoneEnum.CANCELED),
                Type = NotificationTypeEnum.TransferClass.ToString(),
                Image = ImageUrlConstant.SystemImageUrl,
                CreatedAt = GetCurrentTime(),
                IsRead = false,
                ActionData = StringHelper.GenerateJsonString(new List<(string, string)>
                      {
                        ($"{AttachValueEnum.ClassId}", $"{fromClass.Id} , {toClass.Id}"),
                        ($"{AttachValueEnum.StudentId}", $"{student.Id}"),
                      }),
                UserId = student.Parent.Id,
            };

            await SaveChangeProgress(fromClass, oldStudentClass, newStudentClass, oldStudentAttendance, newStudentAttendance, oldStudentEvaluate, newStudentEvaluate, newNotification);
        }

        private async Task SaveChangeProgress(Class fromClass, StudentClass oldStudentClass, StudentClass newStudentClass,
            List<Attendance> oldStudentAttendance, List<Attendance> newStudentAttendance, List<Evaluate> oldStudentEvaluate, List<Evaluate> newStudentEvaluate, Notification newNotification)
        {
            var listItemIdentify = new List<string>
            {
                StringHelper.TrimStringAndNoSpace(newNotification.UserId is null ? "" : newNotification.UserId.Value.ToString()),
                StringHelper.TrimStringAndNoSpace(newNotification.Title!),
                StringHelper.TrimStringAndNoSpace(newNotification.Body!),
                StringHelper.TrimStringAndNoSpace(newNotification.Image!),
                StringHelper.TrimStringAndNoSpace(newNotification.ActionData!),
            };

            string identify = StringHelper.ComputeSHA256Hash(string.Join("", listItemIdentify));
            newNotification.Identify = identify;

            await _unitOfWork.GetRepository<StudentClass>().InsertAsync(newStudentClass);
            await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(newStudentAttendance);
            await _unitOfWork.GetRepository<Evaluate>().InsertRangeAsync(newStudentEvaluate);
            await _unitOfWork.GetRepository<Notification>().InsertAsync(newNotification);

            if (oldStudentAttendance.Any())
            {
                _unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(oldStudentAttendance);
            }
            if (oldStudentEvaluate.Any())
            {
                _unitOfWork.GetRepository<Evaluate>().DeleteRangeAsync(oldStudentEvaluate);
            }

            if (fromClass.Status == ClassStatusEnum.UPCOMING.ToString() || fromClass.Status == ClassStatusEnum.PROGRESSING.ToString())
            {
                _unitOfWork.GetRepository<StudentClass>().DeleteAsync(oldStudentClass);
            }

            if (fromClass.Status == ClassStatusEnum.CANCELED.ToString())
            {
                oldStudentClass.CanChangeClass = false;
                _unitOfWork.GetRepository<StudentClass>().UpdateAsync(oldStudentClass);
            }

        }

        private void ValidateStudentChangeClassRequest(Guid toClassId, List<Guid> studentIdList, Class fromClass, Class toClass, Guid id, Student student)
        {
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            if (toClass.Course!.Id != fromClass.Course!.Id || toClass.Status != ClassStatusEnum.UPCOMING.ToString())
            {
                throw new BadHttpRequestException($"Id [{toClassId}] Lớp Sẽ Chuyển Không Thuộc Cùng 1 Khóa Với Lớp Cần Chuyển Hoặc Không Phải Là Lớp Sắp Diễn Ra",
                    StatusCodes.Status400BadRequest);
            }

            if (toClass.StudentClasses.Count + studentIdList.Count > toClass.LimitNumberStudent)
            {
                throw new BadHttpRequestException($"Số Lượng Học Sinh Của Lớp [{toClass.ClassCode}] Đã Đủ Số Lượng Tối Đa Không Thể Chuyển ", StatusCodes.Status400BadRequest);
            }

            var currentStudentClasses = student.StudentClasses.Select(sc => sc.Class).ToList();

            foreach (var currCls in currentStudentClasses)
            {
                foreach (var schedule in currCls!.Schedules)
                {

                    if (toClass.Schedules.Any(sc => sc.Slot!.StartTime == schedule.Slot!.StartTime))
                    {
                        throw new BadHttpRequestException($"Id [{toClassId}] Lớp Học Sẽ Chuyển Trùng Lịch Học Với Các Lớp Hiện Đang Đăng Ký Khác Của Học Sinh Có Id [{id}]", StatusCodes.Status400BadRequest);
                    }
                }
            }
        }

        private void ValidateChangeClassRequest(Guid classId, List<Guid> studentIdList, Class cls, bool isFromClass)
        {
            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            if (cls.Status != ClassStatusEnum.UPCOMING.ToString() && cls.Status != ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException("Chỉ Có Thể Chuyển Học Sinh Từ Lớp Sắp Diễn Ra Hoặc Đã Hủy," +
                    $" Id [{classId}] Lớp Học Sẽ Chuyển [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status!)}]", StatusCodes.Status400BadRequest);
            }

            if (isFromClass)
            {
                foreach (Guid id in studentIdList)
                {
                    var student = cls.StudentClasses.Where(stu => stu.StudentId == id).FirstOrDefault();
                    if (student != null && student.CanChangeClass == false)
                    {
                        throw new BadHttpRequestException($"Id [{student.Id}] Của Học Sinh Đã Được Chuyển Lớp", StatusCodes.Status400BadRequest);
                    }
                }
            }
            else
            {
                if (studentIdList.Any(id => cls.StudentClasses.Any(sc => sc.StudentId == id)))
                {
                    throw new BadHttpRequestException($"Yêu Cầu Chuyển Lớp Không Hợp Lệ Một Số Id Của Học Sinh Đã Có Trong Id [{classId}] Lớp Sẽ Chuyển", StatusCodes.Status400BadRequest);
                }
            }
        }

        public async Task<List<ClassWithSlotShorten>> GetValidClassForStudentAsync(Guid courseId, Guid studentId)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == courseId,
                include: x => x.Include(x => x.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
               .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)).ThenInclude(ses => ses.SessionDescriptions!));

            if (course == null)
            {
                throw new BadHttpRequestException($"Id [{courseId}] Khóa Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(
             predicate: x => x.CourseId == courseId && x.Status == ClassStatusEnum.UPCOMING.ToString(),
             include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses))).ToList();

            if (!classes.Any())
            {
                throw new BadHttpRequestException($"Id [{courseId}] Khóa Học Chưa Có Lớp Học", StatusCodes.Status400BadRequest);
            }

            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId);
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{courseId}] Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var classRegistered = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId) &&
                x.Status != ClassStatusEnum.COMPLETED.ToString() && x.Status != ClassStatusEnum.CANCELED.ToString(),
                include: x => x.Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

            int age = DateTime.Now.Year - student.DateOfBirth.Year;
            if (age < course.MinYearOldsStudent || age > course.MaxYearOldsStudent)
            {
                return new List<ClassWithSlotShorten>();
            }

            var responses = new List<ClassWithSlotShorten>();

            foreach (var cls in classes)
            {
                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == cls.Id,
                include: x => x.Include(x => x.Slot!));

                cls.Course = course;

                if (cls.StudentClasses.Any(sc => sc.StudentId == studentId))
                {
                    continue;
                }

                if (classRegistered != null)
                {
                    var scheduleChecking = cls.Schedules.ToList();
                    var scheduleRegistered = classRegistered.SelectMany(cr => cr.Schedules).ToList();

                    if (scheduleChecking.Any(scc => scheduleRegistered.Any(scr =>
                        scc.Date == scr.Date && scc.Slot!.StartTime == scr.Slot!.StartTime)))
                    {
                        continue;
                    }
                }

                var syllabusRequired = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    selector: x => x.PrequisiteSyllabusId,
                    predicate: x => x.Id == course.SyllabusId);

                if (syllabusRequired != null && syllabusRequired != default)
                {
                    var syllabusCompleted = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        selector: x => x.Course!.Syllabus!.Id,
                        predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId) && x.Status == ClassStatusEnum.COMPLETED.ToString());

                    var result = syllabusCompleted.Contains(syllabusRequired.Value);
                    if (!result)
                    {
                        continue;
                    }
                }
                var response = _mapper.Map<ClassWithSlotShorten>(cls);
                response.CoursePrice = await GetDynamicPrice(response.ClassId, true);
                responses.Add(response);
            }
            return responses;

        }

        public async Task<List<StudentResponse>> GetValidStudentForClassAsync(Guid classId, List<Guid> studentIds)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
             predicate: x => x.Id == classId,
             include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses)
             .Include(x => x.Course)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
               orderBy: x => x.OrderBy(x => x.Date),
               predicate: x => x.ClassId == cls.Id,
               include: x => x.Include(x => x.Slot!));

            var syllabusRequired = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.PrequisiteSyllabusId,
                predicate: x => x.Id == cls.Course!.SyllabusId));

            var responses = new List<StudentResponse>();

            foreach (var id in studentIds)
            {
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == id);

                int age = DateTime.Now.Year - student.DateOfBirth.Year;
                if (age < cls.Course!.MinYearOldsStudent || age > cls.Course!.MaxYearOldsStudent)
                {
                    continue;
                }

                if (cls.StudentClasses.Any(sc => sc.StudentId == student.Id))
                {
                    continue;
                }

                var classRegistered = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id) &&
                x.Status != ClassStatusEnum.COMPLETED.ToString() && x.Status != ClassStatusEnum.CANCELED.ToString(),
                include: x => x.Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

                if (classRegistered != null)
                {
                    var scheduleChecking = cls.Schedules.ToList();
                    var scheduleRegistered = classRegistered.SelectMany(cr => cr.Schedules).ToList();

                    if (scheduleChecking.Any(scc => scheduleRegistered.Any(scr =>
                        scc.Date == scr.Date && scc.Slot!.StartTime == scr.Slot!.StartTime)))
                    {
                        continue;
                    }
                }

                if (syllabusRequired != null && syllabusRequired != default)
                {
                    var syllabusCompleted = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        selector: x => x.Course!.Syllabus!.Id,
                        predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id) && x.Status == ClassStatusEnum.COMPLETED.ToString());

                    var result = syllabusCompleted.Contains(syllabusRequired.Value);
                    if (!result)
                    {
                        continue;
                    }
                }
                responses.Add(_mapper.Map<StudentResponse>(student));
            }

            return responses;
        }

        public async Task<List<ClassWithSlotShorten>> GetClassesNotInCartAsync(Guid? courseId)
        {
            var classes = await FetchClasses(default);
            if (courseId != null)
            {
                classes = classes.Where(cls => cls.CourseId == courseId && cls.Status == ClassStatusEnum.UPCOMING.ToString()).ToList();
            }

            var classesInCartId = await _unitOfWork.GetRepository<CartItem>().GetListAsync(
                        selector: x => x.ClassId,
                        predicate: x => x.Cart!.UserId == GetUserIdFromJwt() && x.ClassId != default);

            classes = classes.Where(cls => !classesInCartId.Contains(cls.Id)).ToList();
            var responses = classes.Select(c => _mapper.Map<ClassWithSlotShorten>(c)).ToList();
            foreach (var res in responses)
            {
                res.CoursePrice = await GetDynamicPrice(res.ClassId, true);
            }

            return responses;
        }

        public async Task<TopicResponse> GetTopicLearningAsync(Guid classId, int topicOrder)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                         predicate: x => x.Id == classId,
                         include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            var tempQuizTimes = new List<TempQuizTime>();

            var topic = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.Topics!.SingleOrDefault(tp => tp.OrderNumber == topicOrder),
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions!.OrderBy(sd => sd.Order)));


            var questionPackages = new List<QuestionPackage>();
            foreach (var session in topic!.Sessions!)
            {
                var pakage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
                if (pakage != null)
                {
                    questionPackages.Add(pakage);
                }
            }

            var response = new TopicResponse
            {
                OrderTopic = topic!.OrderNumber,
                TopicName = topic.Name,
                Sessions = SessionCustomMapper.fromSessionsAndScheduleToSessionForSyllabusResponses(topic.Sessions!, cls.Schedules),
            };

            foreach (var session in response.Sessions)
            {
                var quiz = questionPackages.SingleOrDefault(qp => qp.NoSession == session.OrderSession);

                if (quiz != null)
                {
                    await SettingSesstionExamTime(classId, tempQuizTimes, session, quiz, session);
                }
            }
            try
            {
                if (tempQuizTimes.Count > 0)
                {
                    await _unitOfWork.GetRepository<TempQuizTime>().InsertRangeAsync(tempQuizTimes);
                    _unitOfWork.Commit();
                }

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status500InternalServerError);
            }

            return response;
        }

        private async Task SettingSesstionExamTime(Guid classId, List<TempQuizTime> tempQuizTimes, SessionSyllabusResponse session, QuestionPackage quiz, SessionSyllabusResponse ssr)
        {
            var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(predicate: x => x.ExamId == quiz.Id && x.ClassId == classId);

            var startTime = DateTime.Parse(session.Date!).Date.Add(session.StartTime!.Value.ToTimeSpan());
            var endTime = DateTime.Parse(session.Date!).Date.Add(session.EndTime!.Value.ToTimeSpan());
            int attempt = 1, duration = 600;
            bool isNonRequireTime = quiz.PackageType != PackageTypeEnum.Review.ToString() && quiz.PackageType != PackageTypeEnum.ProgressTest.ToString() ? true : false;

            if (quizTime != null)
            {
                startTime = quizTime.ExamStartTime != default ? startTime.Date.Add(quizTime.ExamStartTime) : startTime;
                endTime = quizTime.ExamEndTime != default ? endTime.Date.Add(quizTime.ExamEndTime) : endTime;
                attempt = quizTime.AttemptAllowed != 0 ? quizTime.AttemptAllowed : attempt;
                duration = quizTime.Duration != 0 ? quizTime.Duration : duration;
            }
            else if (!isNonRequireTime)
            {
                tempQuizTimes.Add(new TempQuizTime
                {
                    Id = Guid.NewGuid(),
                    ExamStartTime = session.StartTime!.Value.ToTimeSpan(),
                    ExamEndTime = session.EndTime!.Value.ToTimeSpan(),
                    AttemptAllowed = attempt,
                    Duration = duration,
                    ClassId = classId,
                    ExamId = quiz.Id,
                });
            }

            int part = quiz.QuizType.ToLower() == QuizTypeEnum.flashcard.ToString() ? 2 : 1;

            var extenstionName = quiz.PackageType == PackageTypeEnum.FinalExam.ToString() ? "" : " " + quiz.OrderPackage;
            ssr.Quiz = new QuizInforResponse
            {
                ExamId = quiz.Id,
                ExamName = "Bài " + quiz.ContentName.ToLower() + extenstionName,
                ExamPart = part,
                QuizName = quiz.Score == 0 ? "Làm Tại Lớp" : quiz.Title!,
                //QuizDuration = isNonRequireTime ? null : duration,
                QuizDuration = duration,
                Attempts = isNonRequireTime ? null : attempt,
                //QuizStartTime = isNonRequireTime ? null : startTime,
                QuizStartTime = startTime,
                //QuizStartTime = isNonRequireTime ? startTime.Date.Add(new TimeSpan(6, 0, 0)) : startTime,
                //QuizEndTime = isNonRequireTime ? endTime.Date.Add(new TimeSpan(6, 0, 0)) : endTime,
                //QuizEndTime = isNonRequireTime ? null : endTime,
                QuizEndTime = endTime,
            };
        }

        private async Task<FilterRoomAndLecturer> GetRoomAndLecturer(FilterLecturerRequest request)
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role));
            if (users == null)
            {
                return null;
            }
            var lecturers = users.Where(x => x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
            List<LecturerResponse> lecturerResponses = new List<LecturerResponse>();
            foreach (var user in lecturers)
            {
                var lecturerField = await _unitOfWork.GetRepository<LecturerField>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(user.LecturerFieldId.ToString()), selector: x => x.Name);
                var cls = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId.ToString().Equals(user.Id.ToString()));
                var count = 0;
                if (cls != null)
                {
                    count = cls.Count;
                }
                var schedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.SubLecturerId.ToString().Equals(user.Id.ToString()));
                if (schedule != null)
                {
                    var sc = schedule.GroupBy(x => x.ClassId).ToList();
                    count = count + sc.Count;
                }
                LecturerResponse responsex = new LecturerResponse
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    AvatarImage = user.AvatarImage,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    LectureId = user.Id,
                    Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>(),
                    LecturerField = lecturerField,
                    NumberOfClassesTeaching = count,
                };
                lecturerResponses.Add(responsex);
            }
            FilterRoomAndLecturer res = new FilterRoomAndLecturer();
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync();
            if (rooms == null)
            {
                return null;
            }
            if (request != null)
            {
                var type = "all";
                var coursex = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                if (coursex != null)
                {
                    if (coursex.Syllabus != null)
                    {
                        type = coursex.Syllabus.SyllabusCategory.Name;
                    }
                }
                if (type.Equals("all"))
                {
                    lecturerResponses = lecturerResponses;
                }
                else
                {
                    lecturerResponses = lecturerResponses.Where(x => x.LecturerField.Equals(type)).ToList();
                }
                if (request.Schedules != null && request.StartDate != null && request.CourseId != null)
                {
                    List<ScheduleRequest> scheduleRequests = request.Schedules;
                    List<string> daysOfWeek = new List<string>();
                    foreach (ScheduleRequest scheduleRequest in scheduleRequests)
                    {
                        daysOfWeek.Add(scheduleRequest.DateOfWeek);
                    }
                    List<DayOfWeek> convertedDateOfWeek = new List<DayOfWeek>();
                    foreach (var dayOfWeek in daysOfWeek)
                    {
                        if (dayOfWeek.ToLower().Equals("sunday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Sunday);
                        }
                        if (dayOfWeek.ToLower().Equals("monday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Monday);
                        }
                        if (dayOfWeek.ToLower().Equals("tuesday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Tuesday);
                        }
                        if (dayOfWeek.ToLower().Equals("wednesday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Wednesday);
                        }
                        if (dayOfWeek.ToLower().Equals("thursday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Thursday);
                        }
                        if (dayOfWeek.ToLower().Equals("friday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Friday);
                        }
                        if (dayOfWeek.ToLower().Equals("saturday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Saturday);
                        }
                    }
                    var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()));
                    if (course == null)
                    {
                        throw new BadHttpRequestException("không thấy lớp hợp lệ", StatusCodes.Status400BadRequest);
                    }
                    int numberOfSessions = course.NumberOfSession;
                    int scheduleAdded = 0;
                    DateTime startDatex = request.StartDate.Value;
                    while (scheduleAdded < numberOfSessions)
                    {
                        if (convertedDateOfWeek.Contains(startDatex.DayOfWeek))
                        {

                            scheduleAdded++;
                        }
                        startDatex = startDatex.AddDays(1);
                    }
                    var endDate = startDatex;
                    List<ScheduleRequest> schedules = request.Schedules;
                    List<ConvertScheduleRequest> convertSchedule = new List<ConvertScheduleRequest>();
                    foreach (var schedule in schedules)
                    {
                        var doW = 1;
                        if (schedule.DateOfWeek.ToLower().Equals("sunday"))
                        {
                            doW = 1;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("monday"))
                        {
                            doW = 2;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("tuesday"))
                        {
                            doW = 4;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("wednesday"))
                        {
                            doW = 8;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("thursday"))
                        {
                            doW = 16;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("friday"))
                        {
                            doW = 32;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("saturday"))
                        {
                            doW = 64;
                        }
                        convertSchedule.Add(new ConvertScheduleRequest
                        {
                            DateOfWeek = doW,
                            SlotId = schedule.SlotId,
                        });
                    }
                    var allSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync();
                    allSchedule = allSchedule.Where(x => (x.Date < endDate && x.Date >= request.StartDate)).ToList();
                    List<Schedule> result = new List<Schedule>();
                    foreach (var convert in convertSchedule)
                    {
                        var newFilter = allSchedule.Where(x => (x.DayOfWeek == convert.DateOfWeek && x.SlotId.ToString().Equals(convert.SlotId.ToString()))).ToList();
                        if (newFilter != null)
                        {
                            result.AddRange(newFilter);
                        }
                    }
                    List<Guid> roomIds = new List<Guid>();
                    List<Room> exceptRooms = new List<Room>();
                    List<Guid> classIds = new List<Guid>();
                    List<Guid> subLecturerIds = new List<Guid>();
                    if (result.Count > 0)
                    {
                        var groupByRoom = result.GroupBy(x => x.RoomId);
                        roomIds = groupByRoom.Select(x => x.Key).ToList();
                        var groupByClass = result.GroupBy(x => x.ClassId);
                        classIds = groupByClass.Select(x => x.Key).ToList();
                        var groupBySubLecturer = result.Where(x => (x.SubLecturerId != null)).GroupBy(x => x.SubLecturerId.Value);
                        subLecturerIds = groupBySubLecturer.Select(x => x.Key).ToList();

                    }
                    List<Guid> LecturerIds = new List<Guid>();
                    foreach (var classId in classIds)
                    {
                        LecturerIds.Add(await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()), selector: x => x.LecturerId));
                    }
                    LecturerIds.AddRange(subLecturerIds);
                    List<LecturerResponse> final = new List<LecturerResponse>();
                    foreach (var resx in lecturerResponses)
                    {
                        if (!LecturerIds.Contains(resx.LectureId))
                        {
                            final.Add(resx);
                        }
                    }
                    if (final == null || final.Count == 0)
                    {
                        res.Lecturer = null;
                    }
                    else
                    {
                        res.Lecturer = final.OrderBy(x => x.NumberOfClassesTeaching).ToArray()[0];
                    }

                    if (roomIds.Count == 0)
                    {
                        res.Room = rooms.OrderBy(x => x.Name).ToArray()[0];
                    }
                    List<Room> finalResult = new List<Room>();
                    foreach (var room in rooms)
                    {
                        if (!roomIds.Contains(room.Id))
                        {
                            finalResult.Add(room);
                        }
                    }
                    if (final == null || final.Count == 0)
                    {
                        res.Room = null;
                    }
                    else
                    {
                        res.Room = finalResult.OrderBy(x => x.Name).ToArray()[0];
                    }

                    return res;
                }

            }
            res.Room = rooms.OrderBy(x => x.Name).ToArray()[0];
            res.Lecturer = lecturerResponses.OrderBy(x => x.NumberOfClassesTeaching).ToArray()[0];
            return res;
        }

        public async Task<List<Room>> GetRoomsForUpdate(string classId)
        {

            var myResponse = await GetClassDetail(classId);
            var schedule = myResponse.Schedules;
            List<ScheduleRequest> requests = new List<ScheduleRequest>();
            foreach (var sch in schedule)
            {
                var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Equals(sch.StartTime), selector: x => x.Id);
                requests.Add(new ScheduleRequest
                {
                    DateOfWeek = sch.DayOfWeek.ToLower(),
                    SlotId = slotId,
                    StartTime = sch.StartTime,
                    EndTime = sch.EndTime,
                });
            }
            FilterRoomRequest filterRoomRequest = new FilterRoomRequest
            {
                CourseId = myResponse.CourseId.ToString(),
                Method = myResponse.Method,
                StartDate = myResponse.StartDate,
                Schedules = requests,
            };
            var rooms = await _roomService.GetRoomList(filterRoomRequest);
            var oldroom = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == myResponse.RoomResponse.RoomId);
            var exist = rooms.Any(x => x.Id == oldroom.Id);
            if (!exist)
            {
                rooms.Add(oldroom);
            }
            return rooms.OrderBy(x => x.Name).ToList();
        }
        public async Task<List<LecturerResponse>> GetLecturerForUpdate(string classId)
        {
            var myResponse = await GetClassDetail(classId);
            var schedule = myResponse.Schedules;
            List<ScheduleRequest> requests = new List<ScheduleRequest>();
            foreach (var sch in schedule)
            {
                var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Equals(sch.StartTime), selector: x => x.Id);
                requests.Add(new ScheduleRequest
                {
                    DateOfWeek = sch.DayOfWeek.ToLower(),
                    SlotId = slotId,
                    StartTime = sch.StartTime,
                    EndTime = sch.EndTime,
                });
            }
            FilterLecturerRequest filterRoomRequest = new FilterLecturerRequest
            {
                CourseId = myResponse.CourseId.ToString(),
                StartDate = myResponse.StartDate,
                Schedules = requests,
            };
            var lecturers = await _userService.GetLecturers(filterRoomRequest);
            var oldLecutrer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == myResponse.LecturerResponse.LectureId, include: x => x.Include(x => x.LecturerField).Include(x => x.Classes));
            var exist = lecturers.Any(x => x.LectureId == oldLecutrer.Id);
            if (!exist)
            {
                LecturerResponse response = new LecturerResponse
                {
                    FullName = oldLecutrer.FullName,
                    LectureId = oldLecutrer.Id,
                    AvatarImage = oldLecutrer.AvatarImage,
                    DateOfBirth = oldLecutrer.DateOfBirth,
                    Email = oldLecutrer.Email,
                    Gender = oldLecutrer.Gender,
                    LecturerField = oldLecutrer.LecturerField.Name,
                    NumberOfClassesTeaching = oldLecutrer.Classes.Count,
                    Phone = oldLecutrer.Phone,
                    Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>(),
                };
                lecturers.Add(response);
            }
            return lecturers.OrderBy(x => x.NumberOfClassesTeaching).ToList();
        }
        public async Task<List<Room>> GetRoomForUpdateSession(string classId, string slotId, DateTime date)
        {
            var myResponse = await GetClassDetail(classId);
            var listSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Date.Date == date.Date && x.SlotId == Guid.Parse(slotId));
            if (listSchedule == null || listSchedule.Count == 0)
            {
                var rm = await _unitOfWork.GetRepository<Room>().GetListAsync();
                return rm.Where(x => x.Type.ToLower().Equals(myResponse.Method.ToLower())).ToList();
            }
            List<Room> list = new List<Room>();
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync();
            foreach (var room in rooms)
            {
                var isExít = listSchedule.Any(x => x.RoomId == room.Id);
                if (!isExít)
                {
                    list.Add(room);
                }
            }
            list = list.Where(x => x.Type.ToLower().Equals(myResponse.Method.ToLower())).ToList();
            return list;
        }

        public async Task<List<LecturerResponse>> GetLecturerResponseForUpdateSession(string classId, string slotId, DateTime date)
        {
            var myResponse = await GetClassDetail(classId);
            var listSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Date.Date == date.Date && x.SlotId.ToString().Equals(slotId), include: x => x.Include(x => x.Class));
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(myResponse.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
            var cate = course.Syllabus.SyllabusCategory.Name;
            if (listSchedule == null || listSchedule.Count == 0)
            {
                var userList = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role).Include(x => x.Classes));
                userList = userList.Where(x => x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>())).ToList();
                List<LecturerResponse> lecturers = new List<LecturerResponse>();
                foreach (var oldLecutrer in userList)
                {
                    LecturerResponse response = new LecturerResponse
                    {
                        FullName = oldLecutrer.FullName,
                        LectureId = oldLecutrer.Id,
                        AvatarImage = oldLecutrer.AvatarImage,
                        DateOfBirth = oldLecutrer.DateOfBirth,
                        Email = oldLecutrer.Email,
                        Gender = oldLecutrer.Gender,
                        LecturerField = oldLecutrer.LecturerField.Name,
                        NumberOfClassesTeaching = oldLecutrer.Classes.Count,
                        Phone = oldLecutrer.Phone,
                        Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>(),
                    };
                    lecturers.Add(response);
                }
                lecturers = lecturers.Where(x => x.LecturerField.ToLower().Equals(cate.ToLower())).ToList();
                return lecturers.OrderBy(x => x.NumberOfClassesTeaching).ToList();
            }
            List<LecturerResponse> list = new List<LecturerResponse>();
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role).Include(x => x.Classes).Include(x => x.LecturerField));
            users = users.Where(x => x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>())).ToList();
            foreach (var userx in users)
            {
                var isExít = listSchedule.Any(x => x.Class.LecturerId == userx.Id || x.SubLecturerId == userx.Id);
                if (!isExít)
                {
                    LecturerResponse response = new LecturerResponse
                    {
                        FullName = userx.FullName,
                        LectureId = userx.Id,
                        AvatarImage = userx.AvatarImage,
                        DateOfBirth = userx.DateOfBirth,
                        Email = userx.Email,
                        Gender = userx.Gender,
                        LecturerField = userx.LecturerField.Name,
                        NumberOfClassesTeaching = userx.Classes.Count,
                        Phone = userx.Phone,
                        Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>(),
                    };
                    list.Add(response);
                }
            }
            list = list.Where(x => x.LecturerField.ToLower().Equals(cate.ToLower())).ToList();
            return list.OrderBy(x => x.NumberOfClassesTeaching).ToList();
        }

        public async Task<List<StudentGradeResponse>> GetStudentGradeResponses(string classId)
        {
            var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(classId));
            List<StudentGradeResponse> response = new List<StudentGradeResponse>();
            foreach (var st in students)
            {
                var studentId = st.StudentId;
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId, include: x => x.Include(x => x.Parent));
                var res = new StudentGradeResponse
                {
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender,
                    Id = student.Id,
                    ImgAvatar = student.AvatarImage,
                    Name = student.FullName,
                    ParentPhoneNumber = student.Parent.Phone,
                };
                var syllabusId = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId), selector: x => x.Course!.SyllabusId);
                //var syllabusId = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == syllabusId, selector: x => x.Id);
                var examSyll = await _unitOfWork.GetRepository<ExamSyllabus>().GetListAsync(predicate: x => x.SyllabusId == syllabusId);

            }
            return response;
        }

        public async Task<List<ClassWithDailyScheduleRes>> GetClassWithDailyScheduleRes(string classId, string studentId)
        {
            var currentClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId),
         include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules));

            if (currentClass == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var classes = new List<Class>();

            var allCourseClass = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == currentClass.CourseId,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
               .Include(x => x.Lecture)!
               .Include(x => x.StudentClasses).Include(x => x.Course)!);

            var suitableClasses = allCourseClass.Where(courCls =>
               !courCls.Schedules.Any(courSchedule =>
                classes.Any(currCls =>
                currCls.Schedules.Any(schedule =>
                schedule.Slot?.StartTime == courSchedule.Slot?.StartTime)))).ToList();


            suitableClasses = suitableClasses
           .Where(cls => cls.StudentClasses.Count + 1 <= cls.LimitNumberStudent)
           .Where(cls => cls.Id != currentClass.Id).ToList();

            if (currentClass.Status.ToLower().Equals("upcoming"))
            {
                suitableClasses = suitableClasses.Where(x => x.Status.ToLower().Equals("upcoming")).ToList();
            }
            else
            {
                var schedule = currentClass.Schedules.OrderBy(x => x.Date).ToArray();
                var index = 0;
                for (int i = 0; i < schedule.Length; i++)
                {
                    if (schedule[i].Date.Date >= DateTime.Now.Date)
                    {
                        index = i; break;
                    }
                }
                List<Class> result = new List<Class>();
                foreach (var sc in suitableClasses)
                {
                    var schedules = sc.Schedules.OrderBy(x => x.Date).ToArray();
                    if (schedules[index].Date.Date > DateTime.Now.Date)
                    {
                        result.Add(sc);
                    }
                }
                suitableClasses = result;
            }

            var suitableClassesx = suitableClasses.Select(cls => _mapper.Map<ClassWithDailyScheduleRes>(cls)).ToList();
            List<ClassWithDailyScheduleRes> res = new List<ClassWithDailyScheduleRes>();
            foreach (var suitableClass in suitableClassesx)
            {
                var groupBy = from schedule in suitableClass.Schedules
                              group schedule by new { schedule.DayOfWeek, schedule.StartTime, schedule.EndTime } into grouped
                              select new DailySchedule
                              {
                                  DayOfWeek = grouped.Key.DayOfWeek,
                                  StartTime = grouped.Key.StartTime,
                                  EndTime = grouped.Key.EndTime,
                              };
                foreach (var sch in groupBy)
                {
                    if (sch.DayOfWeek.Equals("Sunday"))
                    {
                        sch.DayOfWeek = "Chủ Nhật";
                    }
                    if (sch.DayOfWeek.Equals("Monday"))
                    {
                        sch.DayOfWeek = "Thứ Hai";
                    }
                    if (sch.DayOfWeek.Equals("Tuesday"))
                    {
                        sch.DayOfWeek = "Thứ Ba";
                    }
                    if (sch.DayOfWeek.Equals("Wednesday"))
                    {
                        sch.DayOfWeek = "Thứ Tư";
                    }
                    if (sch.DayOfWeek.Equals("Thursday"))
                    {
                        sch.DayOfWeek = "Thứ Năm";
                    }
                    if (sch.DayOfWeek.Equals("Friday"))
                    {
                        sch.DayOfWeek = "Thứ Sáu";
                    }
                    if (sch.DayOfWeek.Equals("Saturday"))
                    {
                        sch.DayOfWeek = "Thứ Bảy";
                    }
                }
                suitableClass.Schedules = groupBy.ToList();
                var schedules = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == suitableClass.ClassId, include: x => x.Include(x => x.Schedules));
                var sc = schedules.Schedules.Where(x => x.Date.Date <= DateTime.Now.Date);
                if (sc == null)
                {
                    suitableClass.CurrentSession = 0;
                }
                else
                {
                    suitableClass.CurrentSession = sc.Count();
                }

            }
            return suitableClassesx;
        }

        public async Task<ChangeClassResponse> ChangeStaffStudentClassAsync(string fromClassId, string toClassId, string studentId)
        {
            try
            {
                var fromClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(fromClassId),
                include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

                var toClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(toClassId),
                include: x => x.Include(x => x.Course!).Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Slot!));

                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId),
                    include: x => x.Include(x => x.Parent));

                if (toClass.StudentClasses.Any(sc => sc.StudentId.ToString() == studentId))
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Đã Có Trong Lớp Cần Chuyển", StatusCodes.Status400BadRequest);
                }

                await ChangeClassProgress(fromClass, toClass, student);
                _unitOfWork.Commit();
                var schedules = toClass.Schedules.Where(x => x.Date.Date <= DateTime.Now.Date).OrderByDescending(x => x.Date).ToArray();
                if (schedules == null || schedules.Length == 0)
                {
                    return new ChangeClassResponse
                    {
                        CurrentSession = 0,
                        Message = "Đổi lớp thành công",
                    };
                }
                return new ChangeClassResponse
                {
                    CurrentSession = schedules.Length,
                    Message = "Đổi lớp thành công",
                };
                //return "Đổi Lớp Thành Công";
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
        }

        public async Task<bool> SetNotCanMakeUp(string scheduleId, string studentId)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(scheduleId));
            if (schedule.Date.Date < DateTime.Now.Date)
            {
                throw new BadHttpRequestException("Lịch học bạn đưa ra đã nằm ở trong quá khứ", StatusCodes.Status400BadRequest);
            }
            var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId.ToString().Equals(scheduleId) && x.StudentId.ToString().Equals(studentId));
            if (attendance == null)
            {
                throw new BadHttpRequestException("Không tìm ra kết quả", StatusCodes.Status400BadRequest);
            }
            attendance.Note = "CanNotMakeUp";
            _unitOfWork.GetRepository<Attendance>().UpdateAsync(attendance);
            bool isSuc = await _unitOfWork.CommitAsync() > 0;
            return isSuc;
        }

        public async Task<List<CanNotMakeUpResponse>> GetCanNotMakeUpResponses(string? search, DateTime? dateOfBirth)
        {
            var listFound = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.Note.Equals("CanNotMakeUp"));
            if (listFound == null)
            {
                return new List<CanNotMakeUpResponse>();
            }
            List<CanNotMakeUpResponse> canNotMakeUpResponses = new List<CanNotMakeUpResponse>();
            foreach (var found in listFound)
            {
                var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id == found.ScheduleId);
                var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == schedule.ClassId);
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == classx.CourseId);
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == found.StudentId);
                var parent = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == student.ParentId);
                var listSc = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == classx.Id, include: x => x.Include(x => x.Slot)!);
                var dateCheck = schedule.Date.Date;
                var schArray = listSc.OrderBy(x => x.Date).ToArray();
                var index = 0;
                for (var i = 0; i < schArray.Length; i++)
                {
                    if (schArray[i].Date.Date == dateCheck)
                    {
                        index = i;
                        break;
                    }
                }
                var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == course.SyllabusId);
                var listTopic = await _unitOfWork.GetRepository<Topic>().GetListAsync(predicate: x => x.SyllabusId == syllabus.Id);
                Session session = new Session();
                Topic topic1 = new Topic();
                foreach (var topic in listTopic)
                {
                    var sessionx = await _unitOfWork.GetRepository<Session>().SingleOrDefaultAsync(predicate: x => x.TopicId == topic.Id && x.NoSession == index + 1, include: x => x.Include(x => x.SessionDescriptions));
                    if (sessionx != null)
                    {
                        session = sessionx;
                        topic1 = topic;
                        break;
                    }
                }
                var ssd = session.SessionDescriptions;
                List<StaffSessionDescriptionResponse> staffssd = new List<StaffSessionDescriptionResponse>();
                foreach (var ssdx in ssd)
                {
                    staffssd.Add(new StaffSessionDescriptionResponse
                    {
                        ScheduleId = schArray[index].Id,
                        Content = ssdx.Content,
                        Details = ssdx.Detail.Split("/r/n").ToList()
                    });
                }
                var studentRes = new StudentResponse
                {
                    Age = (DateTime.Now.Year - student.DateOfBirth.Year),
                    FullName = student.FullName,
                    StudentId = student.Id,
                    AvatarImage = student.AvatarImage,
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender,
                };
                var parentRes = new UserResponse
                {
                    Email = parent.Email,
                    AvatarImage = parent.AvatarImage,
                    Address = parent.Address,
                    Gender = parent.Gender,
                    DateOfBirth = parent.DateOfBirth.Value,
                    FullName = parent.FullName,
                    Id = parent.Id,
                    Phone = parent.Phone,
                };
                var lastSc = listSc.OrderByDescending(x => x.Date).First();
                var status = "";
                if (lastSc.Date.Date >= DateTime.Now.Date)
                {
                    status = "Waiting Arranging";
                }
                else
                {
                    status = "Invalid";
                }
                var endTime = schArray[schArray.Length - 1].Slot!.EndTime;
                var hour = int.Parse(endTime.Split(':')[0]);
                var minute = int.Parse(endTime.Split(':')[1]);
                var newresponse = new CanNotMakeUpResponse
                {
                    Id = found.Id,
                    ClassCode = classx.ClassCode,
                    CourseName = course.Name,
                    ParentResponse = parentRes,
                    Status = status,
                    StudentResponse = studentRes,
                    NoOfSession = index + 1,
                    Topic = topic1.Name,
                    SessionDescription = staffssd,
                    ValidDate = schArray[schArray.Length - 1].Date.AddHours(hour).AddMinutes(minute),
                };
                canNotMakeUpResponses.Add(newresponse);
            }
            var canNotMakeUpResponse1 = new List<CanNotMakeUpResponse>();
            var canNotMakeUpResponse2 = new List<CanNotMakeUpResponse>();
            if (search != null)
            {
                canNotMakeUpResponse1 = canNotMakeUpResponses.Where(x => x.StudentResponse.FullName.ToLower().Trim().Contains(search.ToLower().Trim()) || x.CourseName.ToLower().Trim().Contains(search.ToLower().Trim())).ToList();
            }
            if (dateOfBirth != null)
            {
                canNotMakeUpResponse2 = canNotMakeUpResponses.Where(x => x.StudentResponse.DateOfBirth.Date == dateOfBirth.Value.Date).ToList();
            }
            if (search != null || dateOfBirth != null)
            {
                canNotMakeUpResponses = canNotMakeUpResponse1.UnionBy(canNotMakeUpResponse2, x => x.Id).ToList();
            }

            return canNotMakeUpResponses;
        }

        public async Task<bool> SaveCourse(string classId, string studentId)
        {
            var studentClass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.StudentId.ToString().Equals(studentId) && x.ClassId.ToString().Equals(classId));
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == studentClass.ClassId);
            if (!(classx.Status.Equals(ClassStatusEnum.UPCOMING.ToString()) || classx.Status.Equals(ClassStatusEnum.CANCELED.ToString())))
            {
                throw new BadHttpRequestException("Lớp đã bắt đầu không thể bảo lưu", StatusCodes.Status400BadRequest);
            }
            studentClass.Status = "Saved";
            studentClass.SavedTime = DateTime.Now;
            _unitOfWork.GetRepository<StudentClass>().UpdateAsync(studentClass);
            var isSuc = await _unitOfWork.CommitAsync() > 0;
            return isSuc;
        }

        public async Task<List<SaveCourseResponse>> GetSaveCourseResponse(string? search, DateTime? dateOfBirth)
        {
            var listFound = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.Status.Equals("Saved"));
            if (listFound == null)
            {
                return new List<SaveCourseResponse>();
            }
            List<SaveCourseResponse> canNotMakeUpResponses = new List<SaveCourseResponse>();
            foreach (var found in listFound)
            {
                var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == found.ClassId);
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == classx.CourseId);
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == found.StudentId);
                var parent = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == student.ParentId);
                var listSc = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == classx.Id);
                var studentRes = new StudentResponse
                {
                    Age = (DateTime.Now.Year - student.DateOfBirth.Year),
                    FullName = student.FullName,
                    StudentId = student.Id,
                    AvatarImage = student.AvatarImage,
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender,
                };
                var parentRes = new UserResponse
                {
                    Email = parent.Email,
                    AvatarImage = parent.AvatarImage,
                    Address = parent.Address,
                    Gender = parent.Gender,
                    DateOfBirth = parent.DateOfBirth.Value,
                    FullName = parent.FullName,
                    Id = parent.Id,
                    Phone = parent.Phone,
                };
                var lastSc = listSc.OrderByDescending(x => x.Date).First();
                var status = "";
                var newresponse = new SaveCourseResponse
                {
                    CourseName = course.Name,
                    SavedTime = found.SavedTime,
                    ParentResponse = parentRes,
                    Status = "Saved",
                    StudentResponse = studentRes,
                    Id = found.Id,
                    CourseId = course.Id,
                    ValidDate = found.SavedTime.Value.AddMonths(3),
                };
                canNotMakeUpResponses.Add(newresponse);
            }
            var canNotMakeUpResponse1 = new List<SaveCourseResponse>();
            var canNotMakeUpResponse2 = new List<SaveCourseResponse>();
            if (search != null)
            {
                canNotMakeUpResponse1 = canNotMakeUpResponses.Where(x => x.StudentResponse.FullName.ToLower().Trim().Contains(search.ToLower().Trim()) || x.CourseName.ToLower().Trim().Contains(search.ToLower().Trim())).ToList();
            }
            if (dateOfBirth != null)
            {
                canNotMakeUpResponse2 = canNotMakeUpResponses.Where(x => x.StudentResponse.DateOfBirth.Date == dateOfBirth.Value.Date).ToList();
            }
            if (search != null || dateOfBirth != null)
            {
                canNotMakeUpResponses = canNotMakeUpResponse1.UnionBy(canNotMakeUpResponse2, x => x.Id).ToList();
            }
            return canNotMakeUpResponses;
        }

        public async Task<List<MyClassResponse>> GetClassToRegister(string studentId, string courseId)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId) && x.Status.ToLower().Equals("upcoming"), include: x => x.Include(x => x.StudentClasses).Include(x => x.Schedules));
            List<Class> filterCls = new List<Class>();
            foreach (var cls in classes)
            {
                var count = cls.StudentClasses.Where(x => x.StudentId.ToString().Equals(studentId));
                if (count.Count() == 0)
                {
                    filterCls.Add(cls);
                }
            }
            var att = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Schedule));
            List<Schedule> schedules = new List<Schedule>();
            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var attE in att)
            {
                if (attE.Schedule.Date >= DateTime.Now.Date)
                {
                    schedules.Add(attE.Schedule);
                    dateTimes.Add(attE.Schedule.Date.Date);
                }
            }
            List<Class> filterClass2 = new List<Class>();
            foreach (var cls in filterCls)
            {
                var classSch = cls.Schedules;
                var check = false;
                foreach (var sch in classSch)
                {
                    var flag = dateTimes.Any(x => x.Date == sch.Date);
                    if (flag)
                    {
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    filterClass2.Add(cls);
                }
            }
            List<MyClassResponse> result = new List<MyClassResponse>();
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            foreach (var c in filterClass2)
            {
                var schedulex = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == c.Id)).FirstOrDefault();
                if (schedulex == null) { continue; }
                var room = (await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == schedulex.RoomId));
                if (room == null) { continue; }
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.LecturerId.ToString()));
                if (lecturer == null) { continue; }
                RoomResponse roomResponse = new RoomResponse
                {
                    Floor = room.Floor.Value,
                    Capacity = room.Capacity,
                    RoomId = room.Id,
                    Name = room.Name,
                    Status = room.Status,
                    LinkUrl = room.LinkURL,

                };
                LecturerResponse lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                };
                List<DailySchedule> schedules1 = new List<DailySchedule>();
                var DaysOfWeek = c.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
                foreach (var day in DaysOfWeek)
                {
                    var slot = slots.Where(x => x.Id.ToString().ToLower().Equals(day.SlotId.ToString().ToLower())).FirstOrDefault();
                    if (day.DayOfWeek == 1)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Sunday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 2)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Monday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 4)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Tuesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 8)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Wednesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 16)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Thursday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 32)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Friday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 64)
                    {
                        schedules1.Add(new DailySchedule
                        {
                            DayOfWeek = "Saturday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                }
                Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                var studentList = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id);
                var studentClassCount = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id && x.Status.Equals("Saved"));
                MyClassResponse myClassResponse = new MyClassResponse
                {
                    ClassId = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,
                    ClassCode = c.ClassCode,
                    LecturerName = lecturer.FullName,
                    CoursePrice = await GetDynamicPrice(course.Id, false),
                    EndDate = c.EndDate,
                    CourseId = c.CourseId,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = studentList.Count - studentClassCount.Count,
                    Schedules = schedules1,
                    CourseName = course.Name,
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,
                    CreatedDate = c.AddedDate.Value,
                };
                var syllabusCode = "undefined";
                var syllabusName = "undefined";
                var syllabusType = "undefined";
                if (course.Syllabus != null)
                {
                    syllabusCode = course.Syllabus.SubjectCode;
                    syllabusName = course.Syllabus.Name;
                    syllabusType = course.Syllabus.SyllabusCategory.Name;
                }
                CustomCourseResponse customCourseResponse = new CustomCourseResponse
                {
                    Image = course.Image,
                    MainDescription = course.MainDescription,
                    MaxYearOldsStudent = course.MaxYearOldsStudent,
                    MinYearOldsStudent = course.MinYearOldsStudent,
                    Name = course.Name,
                    Price = await GetDynamicPrice(course.Id, false),
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = string.Empty,
                };
                myClassResponse.CourseResponse = customCourseResponse;
                var canChange = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.ClassId == myClassResponse.ClassId && x.StudentId.ToString().Equals(studentId));
                result.Add(myClassResponse);
            }
            return result;
        }

        #endregion
    }
}

