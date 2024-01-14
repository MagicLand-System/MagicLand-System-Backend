using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.User;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<string> AutoCreateClassCode(string courseId)
        {
            return "success";
            
        }

        public async Task<bool> CreateNewClass(CreateClassRequest request)
        {
            if(request.StartDate <  DateTime.Now) 
            {
                throw new BadHttpRequestException("start date is wrong", StatusCodes.Status400BadRequest);
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
                Status = "UPCOMMING",
                Method = request.Method,
                District = "Tân Bình",
                City = "Hồ Chí Minh",
                Street = "138 Lương Định Của",
            };
            await _unitOfWork.GetRepository<Class>().InsertAsync(createdClass);
            var isSuccessAtClass = await _unitOfWork.CommitAsync() > 0;
            if(!isSuccessAtClass) 
            {
                throw new BadHttpRequestException("insert class failed", StatusCodes.Status400BadRequest);
            }
            List<ScheduleRequest> scheduleRequests = request.ScheduleRequests;
            List<string> daysOfWeek = new List<string>();
            foreach(ScheduleRequest scheduleRequest in scheduleRequests)
            {
                daysOfWeek.Add(scheduleRequest.DateOfWeek);
            }
            List<DayOfWeek> convertedDateOfWeek = new List<DayOfWeek>();
            foreach(var dayOfWeek in daysOfWeek)
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
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate : x => x.Id.ToString().Equals(request.CourseId.ToString()));
            if(course == null)
            {
                throw new BadHttpRequestException("not found course matches",StatusCodes.Status400BadRequest);
            }
            int numberOfSessions = course.NumberOfSession;
            int scheduleAdded = 0;
            DateTime startDate = request.StartDate;
            List<Schedule> schedules = new List<Schedule>();
            List<ScheduleRequest> sc = request.ScheduleRequests;
            while(scheduleAdded < numberOfSessions)
            {
                if (convertedDateOfWeek.Contains(startDate.DayOfWeek))
                {
                    string dateString = startDate.DayOfWeek.ToString().ToLower();
                    Guid slotId = Guid.NewGuid();
                    foreach(var sq in sc)
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
                        DayOfWeek = (int) Math.Pow(2, (int) startDate.DayOfWeek),
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
                    throw new BadHttpRequestException("insert schedule is failed", StatusCodes.Status400BadRequest);
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
            if(!isSuccess)
            {
                throw new BadHttpRequestException("updated class is failed");
            }
            return isSuccess;
        }

        public async Task<List<ClassResponseV1>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent)
        {
            var classes = await FetchClasses();

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
                    (c.Name!.ToLower().Contains(k.ToLower()) ||
                    c.ClassCode!.ToLower().Contains(k.ToLower()) ||
                    c.StartDate.ToString().ToLower().Contains(k.ToLower()) ||
                    c.EndDate.ToString().ToLower().Contains(k.ToLower()) ||
                    c.Method!.ToString().ToLower().Contains(k.ToLower()) ||
                    c.Status!.ToString().ToLower().Contains(k.ToLower()) ||
                    (c.City + c.District + c.Street).ToLower().Contains(k.ToLower())))).ToList();

            leastNumberStudent ??= 1;
            limitStudent ??= int.MaxValue;

            classes = classes.Where(c => c.LeastNumberStudent >= leastNumberStudent || c.LimitNumberStudent == limitStudent).ToList();

            return classes.Select(c => _mapper.Map<ClassResponseV1>(c)).ToList();
        }

        public async Task<List<ClassResponseV2>> GetAllClass(string searchString = null , string status = null)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(include : x => x.Include(x => x.Lecture).Include(x => x.Course).Include(x => x.Schedules).Include(x => x.StudentClasses));
            var roomId = classes.First(x => x.Id == x.Id).Schedules.First().RoomId;
            var lecturerId = classes.First(x => x.Id == x.Id).LecturerId;
            var room = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate : x => x.Id.ToString().Equals(roomId.ToString()));
            var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(lecturerId.ToString()));
            RoomResponse roomResponse = new RoomResponse
            {
                Floor = room.Floor.Value,
                Capacity = room.Capacity,
                Id = room.Id,
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
                Id = lecturer.Id,
                Phone = lecturer.Phone,
            };
            List<ClassResponseV2> result = new List<ClassResponseV2>();
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
                ClassResponseV2 myClassResponse = new ClassResponseV2
                {
                    Id = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,  
                    ClassCode = c.ClassCode,
                    LecturerName = c.Lecture.FullName,
                    CoursePrice = c.Course.Price,
                    EndDate = c.EndDate,
                    CourseId = c.Course.Id,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    Name = c.Name,  
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = c.StudentClasses.Count(),
                    Schedules = schedules,
                    CourseName = c.Course.Name, 
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,    
                };
                result.Add(myClassResponse);
            }
            if (result.Count == 0)
            {
                return null;
            }
            if(searchString == null && status == null)
            {
                return result;
            }
            if (searchString == null)
            {   
                return (result.Where(x => x.Status.ToLower().Equals(status.ToLower())).ToList());     
            }
            if(status == null)
            {
                List<ClassResponseV2> res = new List<ClassResponseV2>();
                var filter1 = result.Where(x => x.ClassCode.ToLower().Contains(searchString.ToLower()));
                if(filter1 != null)
                {
                    res.AddRange(filter1);
                }
                var filter2 = result.Where(x => x.CourseName.ToLower().Contains((searchString.ToLower())));   
                if(filter2 != null)
                {
                    res.AddRange(filter2);
                }
                return res;
            }
            return (result.Where(x => ((x.ClassCode.ToLower().Contains(searchString.ToLower()) || x.CourseName.ToLower().Contains(searchString.ToLower()))&& x.Status.ToLower().Equals(status.ToLower())))).ToList();
        }

        public async Task<List<StudentInClass>> GetAllStudentInClass(string id)
        {
            var studentIds = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate : x => x.ClassId.ToString().Equals(id), selector : x => x.StudentId);
            if(studentIds == null)
            {
                return null;
            }
            List<StudentInClass> result = new List<StudentInClass>();
            foreach (var studentId in studentIds)
            {
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().ToLower().Equals(studentId.ToString().ToLower()), include: x => x.Include(x => x.User));
                StudentInClass studentInClass = new StudentInClass
                {
                    DateOfBirth = student.DateOfBirth,
                    FullName = student.FullName,
                    Gender = student.Gender,
                    ParentName = student.User.FullName,
                    ParentPhoneNumber = student.User.Phone,
                    StudentId = student.Id,
                    ImgAvatar = student.AvatarImage,
                };
                result.Add(studentInClass);
            }
            return result;
        }

        public async Task<ClassResponseV1> GetClassByIdAsync(Guid id)
        {

            var cls = await _unitOfWork.GetRepository<Class>()
               .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
               .Include(x => x.Lecture!)
               .Include(x => x.StudentClasses)
               .Include(x => x.Course)
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Slot)!
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Room)!);

            return _mapper.Map<ClassResponseV1>(cls);
        }

        public async Task<ClassResponseV2> GetClassDetail(string id)
        {
            var listClass = await GetAllClass();
            if(listClass == null)
            {
                return null;
            } 
            var matchClass = listClass.SingleOrDefault(x => x.Id.ToString().Equals(id.ToString()));
            return matchClass;
        }

        public async Task<List<ClassResponseV1>> GetClassesAsync()
        {
            var classes = await FetchClasses();

            return classes.Select(c => _mapper.Map<ClassResponseV1>(c)).ToList();
        }

        public async Task<List<ClassResponseV1>> GetClassesByCourseIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == id);

            var classes = course == null
                ? throw new BadHttpRequestException("Course Id Not Exist", StatusCodes.Status400BadRequest)
                : await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.CourseId == id, include: x => x
                .Include(x => x.Lecture!)
                .Include(x => x.StudentClasses)
                .Include(x =>x.Course)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                .ThenInclude(s => s.Slot)!
                .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                .ThenInclude(s => s.Room)!);

            var responses = classes.Select(c => _mapper.Map<ClassResponseV1>(c)).ToList();
            foreach(var res in responses)
            {
                res.CoursePrice = course.Price;
            }

            return responses;
        }

        public async Task<bool> InsertAttandance()
        {
            var allStudentInClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(include : x => x.Include(x => x.Class));
            List<Attendance> attendances = new List<Attendance>();
            foreach (var studentClass in allStudentInClass)
            {
                var classId = studentClass.Class.Id;
                var schedules = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()),selector : x => x.Schedules);
                var studentId = studentClass.StudentId;
                foreach(var schedule in schedules)
                {
                    Attendance attendance = new Attendance
                    {
                        Id = Guid.NewGuid(),
                        IsPresent = null,
                        ScheduleId = schedule.Id,
                        StudentId = studentId,
                    };
                    attendances.Add(attendance);
                }
            }
            await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(attendances);
            bool success = await _unitOfWork.CommitAsync() > 0;
            return success;
            
        }

        private async Task<ICollection<Class>> FetchClasses()
        {
            return await _unitOfWork.GetRepository<Class>()
                .GetListAsync(include: x => x
                .Include(x => x.Lecture!)
                .Include(x => x.Course!)
               .Include(x => x.StudentClasses)
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Slot)!
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Room)!);

        }
    }
}
