using AutoMapper;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region gia_thuong_code
        public async Task<string> AutoCreateClassCode(string courseId)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId),include : x => x.Include(x => x.Syllabus));
            if (course == null)
            {
                return null;
            }
            //var name = course.Name;
            //var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //string abbreviation = string.Join("", words.Select(word => word[0]));
            if(course.Syllabus == null)
            {
                throw new BadHttpRequestException("course chưa gắn syllabys",StatusCodes.Status400BadRequest);
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

        public async Task<bool> CreateNewClass(CreateClassRequest request)
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
                EndDate = DateTime.Now,
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
            return isSuccess;
        }


        public async Task<List<MyClassResponse>> GetAllClass(string searchString = null, string status = null)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(include: x => x.Include(x => x.Lecture).Include(x => x.Course).Include(x => x.Schedules).Include(x => x.StudentClasses));
            classes = (classes.OrderByDescending(x => x.AddedDate)).ToList();
            var cls = classes.First();

            var roomId = classes.First(x => x.Id == x.Id).Schedules.First().RoomId;
            var lecturerId = classes.First(x => x.Id == x.Id).LecturerId;
            var room = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(roomId.ToString()));
            var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(lecturerId.ToString()));
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
            List<MyClassResponse> result = new List<MyClassResponse>();
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            foreach (var c in classes)
            {
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
                MyClassResponse myClassResponse = new MyClassResponse
                {
                    ClassId = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,
                    ClassCode = c.ClassCode,
                    LecturerName = c.Lecture.FullName,
                    CoursePrice = c.Course.Price,
                    EndDate = c.EndDate,
                    CourseId = c.Course.Id,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = c.StudentClasses.Count(),
                    Schedules = schedules,
                    CourseName = c.Course.Name,
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,
                    CreatedDate = c.AddedDate.Value,
                };
                Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.Course.Id.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
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
                    MinYearOldsStudent = course.MaxYearOldsStudent,
                    Name = course.Name,
                    Price = course.Price,
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = course.Status
                };
                myClassResponse.CourseResponse = customCourseResponse;
                result.Add(myClassResponse);
            }
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
                return result;
            }
            if (searchString == null)
            {
                return (result.Where(x => x.Status.ToLower().Equals(status.ToLower())).ToList());
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
                return res;
            }
            return (result.Where(x => ((x.ClassCode.ToLower().Contains(searchString.ToLower()) || x.CourseName.ToLower().Contains(searchString.ToLower())) && x.Status.ToLower().Equals(status.ToLower())))).ToList();
        }

        public async Task<MyClassResponse> GetClassDetail(string id)
        {
            var listClass = await GetAllClass();
            if (listClass == null)
            {
                return null;
            }
            var matchClass = listClass.SingleOrDefault(x => x.ClassId.ToString().Equals(id.ToString()));
            return matchClass;
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
            if (request.LeastNumberStudent <= request.LimitNumberStudent)
            {
                throw new BadHttpRequestException("số lượng tối thiểu phải nhỏ hơn số lượng tối đa", StatusCodes.Status400BadRequest);
            }
            classFound.Status = "UPCOMING";
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
                    DateOfBirth = lecturer.DateOfBirth,
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
                };
                var status = "completed";
                if (schedule.Date > DateTime.Now.AddHours(-23))
                {
                    status = "future";
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
                    dateOfWeek = "=wednesday";
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
                var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId.ToString().Equals(schedule.Id.ToString()));
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
                        CoursePrice = schedule.Class.Course.Price,
                        CourseId = schedule.Class.Course.Id,
                        CourseName = schedule.Class.Course.Name,
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
                classForAttendances = classForAttendances.Where(x => (x.ClassCode.ToLower().Equals(searchString.Trim().ToLower()) || x.CourseName.Trim().ToLower().Equals(searchString.Trim().ToLower()))).ToList();
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
                string errorMessage = currentClass.Status == ClassStatusEnum.LOCKED.ToString()
                    ? "Đã Chốt Số Lượng Học Sinh"
                    : currentClass.Status == ClassStatusEnum.COMPLETED.ToString()
                    ? "Đã Hoàn Thành" : "Đã Bắt Đầu";

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
            if (isExist != null)
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
            var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => (x.StudentId.ToString().Equals(StudentId) && x.ScheduleId.ToString().Equals(ScheduleId)));
            if (attendance == null) { return false; }
            attendance.ScheduleId = Guid.Parse(request.ScheduleId);
            _unitOfWork.GetRepository<Attendance>().UpdateAsync(attendance);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
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
            var scheduleClassIndex = allSchedule.Where(x => x.Class.Id.ToString().Equals(schedule.Class.Id.ToString())).ToList();
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
                    dateOfWeek = "=wednesday";
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
                    Id = schedulex.Id,
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
                var isExist = attendance.SingleOrDefault(x => x.ScheduleId.ToString().Equals(response.Id.ToString()));
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
                var courseId = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Name.Trim().ToLower().Equals(rq.CourseName.Trim().ToLower()), selector: x => x.Id);
                var roomId = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Name.Trim().ToLower().Equals(rq.RoomName.Trim().ToLower()), selector: x => x.Id);
                var lecturerId = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().ToLower().Equals(rq.LecturerPhone.Trim().ToLower()), selector: x => x.Id);
                if (courseId == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Không tìm thấy khóa học có tên như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (roomId == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Không tìm thấy phòng có tên như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (lecturerId == null)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Không tìm thấy giáo viên có sđt như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var schedules = rq.ScheduleRequests;
                List<ScheduleRequest> scheduleRequests = new List<ScheduleRequest>();
                foreach (var schedule in schedules)
                {
                    string[] parts = schedule.ScheduleTime.Split(':');
                    string day = parts[0].Trim(); // Thứ 2
                    string timeRange = parts[1].Trim(); // 15:00 - 20:00
                    string startTime = timeRange.Split('-')[0].Trim(); // 15:00
                    var slotId = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.StartTime.Trim().Contains(startTime.Trim()), selector: x => x.Id);
                    if (slotId == null)
                    {
                        throw new BadHttpRequestException($"cột {rq.Index} không tồn tại slot như v", StatusCodes.Status400BadRequest);
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

                string format = "dd/MM/yyyy";

                var date = DateTime.TryParseExact(rq.StartDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                    ? (DateTime?)parsedDate
                    : DateTime.Parse(rq.StartDate);
                var resultCheck = await CheckValidateSchedule(new FilterLecturerRequest
                {
                    CourseId = courseId.ToString(),
                    Schedules = scheduleRequests,
                    StartDate = date,
                },lecturerId.ToString(),roomId.ToString());
                if (resultCheck.Equals("FBRL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Xuất hiện sự trùng cả giáo viên và phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBR"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Xuất hiện sự trùng phòng học nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                if (resultCheck.Equals("FBL"))
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Xuất hiện sự trùng giảng viên nếu xếp  như vậy",
                        IsSucess = false
                    });
                    continue;
                }
                var myRequest = new CreateClassRequest
                {
                    ClassCode = await AutoCreateClassCode(courseId.ToString()),
                    CourseId = courseId,
                    LecturerId = lecturerId,
                    LeastNumberStudent = rq.LeastNumberStudent,
                    LimitNumberStudent = rq.LimitNumberStudent,
                    Method = rq.Method,
                    ScheduleRequests = scheduleRequests,
                    StartDate = date.Value,
                    RoomId = roomId,
                };
                var isSuccess = await CreateNewClass(myRequest);
                if (!isSuccess)
                {
                    rows.Add(new RowInsertResponse
                    {
                        Index = rq.Index,
                        Error = "Có lỗi trong quá trình insert",
                        IsSucess = false
                    });
                    continue;
                }
                rows.Add(new RowInsertResponse
                {
                    Index = rq.Index,
                    IsSucess = true,
                });
            }
            response.RowInsertResponse = rows;
            var succ = response.RowInsertResponse.Where(x => x.IsSucess).Count();
            var fail = response.RowInsertResponse.Where(x => !x.IsSucess).Count();
            response.SuccessRow = succ;
            response.FailureRow = fail;
            return response;
        }
        private async Task<string> CheckValidateSchedule(FilterLecturerRequest request,string LecturerId,string RoomId)
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
            var allSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(include : x => x.Include(x => x.Class));
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
            if((rx1 != null && rx1.Count > 0) || (rx2 != null && rx2.Count > 0))
            {
                return "FBRL";
            }
            var rs = result.Where(x => x.RoomId.ToString().Equals(RoomId)).ToList();
            if(rs != null && rs.Count > 0)
            {
                return "FBR";
            }
            var rs1 = result.Where(x => (x.Class.LecturerId.ToString().Equals(LecturerId))).ToList();
            var rs2 = result.Where(x => x.SubLecturerId != null).Where(x => x.SubLecturerId.ToString().Equals(LecturerId)).ToList();
            if((rs1 != null && rs1.Count > 0) || (rs2 != null && rs2.Count > 0)){
                return "FBL";
            }
            return "S";
        }
        #endregion
        #region thanh_lee_code
        public async Task<List<ClassResExtraInfor>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent, PeriodTimeEnum time)
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

            return classes.Select(c => _mapper.Map<ClassResExtraInfor>(c)).ToList();
        }

        public async Task<ClassResExtraInfor> GetClassByIdAsync(Guid id)
        {

            var cls = await _unitOfWork.GetRepository<Class>()
               .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
               .Include(x => x.Lecture!)
               .Include(x => x.StudentClasses)
               .Include(x => x.Course).ThenInclude(c => c!.Syllabus)
               .ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
               .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession))
               .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Slot)!
               .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Room)!);

            return _mapper.Map<ClassResExtraInfor>(cls);
        }

        public async Task<List<ClassResExtraInfor>> GetClassesAsync(PeriodTimeEnum time)
        {
            var classes = await FetchClasses(time);

            return classes.Select(c => _mapper.Map<ClassResExtraInfor>(c)).ToList();
        }

        public async Task<List<ClassWithSlotShorten>> GetClassesByCourseIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                include: x => x.Include(x => x.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
                .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)));

            var classes = course == null
                ? throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại", StatusCodes.Status400BadRequest)
                : await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.CourseId == id && x.Status == ClassStatusEnum.UPCOMING.ToString(), include: x => x
                .Include(x => x.Lecture!)
                .Include(x => x.StudentClasses)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Slot)!
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Room)!);

            var responses = new List<ClassWithSlotShorten>();

            foreach (var cls in classes)
            {
                cls.Course = course;
                responses.Add(_mapper.Map<ClassWithSlotShorten>(cls));
            }
            return responses;
        }

        private async Task<ICollection<Class>> FetchClasses(PeriodTimeEnum time)
        {
            var classes = new List<Class>();

            if (time != default)
            {
                classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => DateTime.Now <= x.StartDate && x.StartDate <= DateTime.Now.AddDays((int)time),
                include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses))).ToList();
            }
            else
            {
                classes = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses))).ToList();
            }

            foreach (var cls in classes)
            {
                cls.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == cls.CourseId,
                include: x => x.Include(x => x.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
               .ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)).ThenInclude(ses => ses.SessionDescriptions!));

                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == cls.Id,
                include: x => x.Include(x => x.Slot!).Include(x => x.Room!));
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
            var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(id),
               include: x => x.Include(x => x.Student)!.ThenInclude(x => x.User));

            if (students == null)
            {
                return default!;
            }

            return students.Select(x => _mapper.Map<StudentInClass>(x)).ToList();

        }

        public async Task ValidateScheduleAmongClassesAsync(List<Guid> classIdList)
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
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sche => sche.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sche => sche.Room).Include(x => x.Course!));

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Giáo Viên Chưa Được Phân Dạy Ở Bất Kỳ Lớp Nào", StatusCodes.Status400BadRequest);
            }

            var currentClasses = classes.Where(cls => cls.Schedules.Any(sc => sc.Date.Date == DateTime.Now.Date)).ToList();

            return GenerateClassSchedule(currentClasses);
        }

        private List<ClassWithSlotOutSideResponse> GenerateClassSchedule(List<Class> currentClasses)
        {
            var responses = new List<ClassWithSlotOutSideResponse>();
            foreach (var cls in currentClasses)
            {
                var currentSchedule = cls.Schedules.SingleOrDefault(sc => sc.Date.Date == DateTime.Now.Date);
                if (currentSchedule == null)
                {
                    throw new BadHttpRequestException("Giáo Viên Không Có Lịch Dạy Ở Hiện Tại", StatusCodes.Status400BadRequest);
                }
                var response = _mapper.Map<ClassWithSlotOutSideResponse>(cls);
                response.ScheduleId = currentSchedule.Id;
                response.DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(currentSchedule.DayOfWeek)[0].ToString();
                response.Date = currentSchedule.Date;
                response.Slot = SlotCustomMapper.fromSlotToSlotForLecturerResponse(currentSchedule.Slot!);
                response.Room = RoomCustomMapper.fromRoomToRoomResponse(currentSchedule.Room!);
                response.SlotOrder = StringHelper.GetSlotNumber(currentSchedule.Slot!.StartTime);

                responses.Add(response);
            }
            return responses;
        }

        public async Task<ScheduleWithAttendanceResponse> GetAttendanceOfClassesInDateAsync(Guid classId, DateTime date)
        {

            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.LecturerId == GetUserIdFromJwt() && x.Id == classId,
             include: x => x.Include(x => x.Lecture).Include(x => x.Course)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Không Tồn Tại Hoặc Lớp Học Không Thuộc Phân Công Của Giáo Viên Hiện Tại", StatusCodes.Status400BadRequest);
            }

            if (cls.Status!.Trim() != ClassStatusEnum.PROGRESSING.ToString())
            {

                throw new BadHttpRequestException("Chỉ Có Thể Truy Suất Danh Sách Điểm Danh Của Các Lớp Học Đang Diễn Ra," +
                    $" Lớp [{cls.ClassCode}] [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status)}]", StatusCodes.Status400BadRequest);
            }

            return GenerateSchedultAttendance(cls, date);
        }

        private ScheduleWithAttendanceResponse GenerateSchedultAttendance(Class cls, DateTime date)
        {
            var schedule = cls.Schedules.SingleOrDefault(sc => sc.Date.Date == date.Date);
            if (schedule == null)
            {
                return new ScheduleWithAttendanceResponse();
            }

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
                    include: x => x.Include(x => x.StudentClasses.Where(sc => sc.ClassId != fromClassId)).ThenInclude(sc => sc.Class!).ThenInclude(cls => cls.Schedules)!.ThenInclude(x => x.Slot!).Include(x => x.User));

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
            foreach (var schedule in fromClass.Schedules)
            {
                var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.StudentId == student.Id && x.ScheduleId == schedule.Id);
                if (attendance != null)
                {
                    oldStudentAttendance.Add(attendance);
                }
            }

            var newStudentAttendance = new List<Attendance>();
            foreach (var schedule in toClass.Schedules)
            {
                newStudentAttendance.Add(new Attendance
                {
                    Id = new Guid(),
                    IsPublic = true,
                    IsPresent = default,
                    StudentId = student.Id,
                    ScheduleId = schedule.Id
                });
            }

            var newNotification = new Notification
            {
                Id = new Guid(),
                Title = NotificationMessageContant.ChangeClassTitle,
                Body = NotificationMessageContant.ChangeClassBody(fromClass.ClassCode!, toClass.ClassCode!,
                      student.FullName!, fromClass.Status == ClassStatusEnum.UPCOMING.ToString() ? ChangeClassReasoneEnum.REQUEST : ChangeClassReasoneEnum.CANCELED),
                Priority = NotificationPriorityEnum.IMPORTANCE.ToString(),
                Image = ImageUrlConstant.SystemImageUrl,
                CreatedAt = DateTime.Now,
                IsRead = false,
                ActionData = StringHelper.GenerateJsonString(new List<(string, string)>
                      {
                        ($"{AttachValueEnum.ClassId}", $"{fromClass.Id} , {toClass.Id}"),
                        ($"{AttachValueEnum.StudentId}", $"{student.Id}"),
                      }),
                UserId = student.User.Id,
            };

            await SaveChangeProgress(fromClass, oldStudentClass, newStudentClass, oldStudentAttendance, newStudentAttendance, newNotification);
        }

        private async Task SaveChangeProgress(Class fromClass, StudentClass oldStudentClass, StudentClass newStudentClass, List<Attendance> oldStudentAttendance, List<Attendance> newStudentAttendance, Notification newNotification)
        {

            await _unitOfWork.GetRepository<StudentClass>().InsertAsync(newStudentClass);
            await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(newStudentAttendance);
            await _unitOfWork.GetRepository<Notification>().InsertAsync(newNotification);

            if (oldStudentAttendance.Any())
            {
                _unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(oldStudentAttendance);
            }

            if (fromClass.Status == ClassStatusEnum.UPCOMING.ToString())
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

                var syllabusRequired = await _unitOfWork.GetRepository<SyllabusPrerequisite>().GetListAsync(predicate: x => x.CurrentSyllabusId == course.SyllabusId);
                if (syllabusRequired != null)
                {
                    var syllabusCompleted = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        selector: x => x.Course!.Syllabus!.Id,
                        predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId) && x.Status == ClassStatusEnum.COMPLETED.ToString());

                    var result = syllabusRequired.All(sylr => syllabusCompleted.Any(sylc => sylr.PrerequisiteSyllabusId == sylc));
                    if (!result)
                    {
                        continue;
                    }
                }

                responses.Add(_mapper.Map<ClassWithSlotShorten>(cls));
            }

            return responses;

        }

        public async Task<List<StudentResponse>> GetValidStudentForClassAsync(Guid classId, List<Student> students)
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

            var responses = new List<StudentResponse>();

            foreach (var student in students)
            {
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

                var syllabusRequired = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync(
                    predicate: x => x.CourseId == cls.CourseId, 
                    include: x => x.Include(x => x.SyllabusPrerequisites)!)).SelectMany(x => x.SyllabusPrerequisites!.Select(sp => sp.PrerequisiteSyllabusId));

                if (syllabusRequired != null)
                {
                    var syllabusCompleted = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        selector: x => x.Course!.Syllabus!.Id,
                        predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id) && x.Status == ClassStatusEnum.COMPLETED.ToString());

                    var result = syllabusRequired.All(sylr => syllabusCompleted.Any(sylc => sylr == sylc));
                    if (!result)
                    {
                        continue;
                    }
                }
                responses.Add(_mapper.Map<StudentResponse>(student));
            }

            return responses;
        }
        #endregion
    }
}

