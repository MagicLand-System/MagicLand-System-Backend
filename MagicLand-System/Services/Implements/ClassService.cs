using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
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

        public async Task<List<ClassResponseV2>> GetAllClass(string classCode = null)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(include : x => x.Include(x => x.Lecture).Include(x => x.Course).Include(x => x.Schedules).Include(x => x.StudentClasses));
            List<ClassResponseV2> result = new List<ClassResponseV2>();
            foreach (var c in classes)
            {
                List<string> schedules = new List<string>();
                List<int> DaysOfWeek = c.Schedules.Select(c => c.DayOfWeek).Distinct().ToList();
                foreach (var day in DaysOfWeek)
                {
                    if(day == 1)
                    {
                        schedules.Add("Chủ Nhật");
                    }
                    if (day == 2)
                    {
                        schedules.Add("Thứ 2");
                    }
                    if (day == 4)
                    {
                        schedules.Add("Thứ 3");
                    }
                    if (day == 8)
                    {
                        schedules.Add("Thứ 4");
                    }
                    if (day == 16)
                    {
                        schedules.Add("Thứ 5");
                    }
                    if (day == 32)
                    {
                        schedules.Add("Thứ 6");
                    }
                    if (day == 64)
                    {
                        schedules.Add("Thứ 7");
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
                    Schedules = schedules.OrderBy(str => str).ToList(),
                };
                result.Add(myClassResponse);
            }
            if (result.Count == 0)
            {
                return null;
            }
            if(classCode == null)
            {
                return result;
            }
            return (result.Where(x => x.ClassCode.ToLower().Contains(classCode.ToLower()))).ToList();
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
