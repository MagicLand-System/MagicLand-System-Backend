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
                Name = request.ClassName,
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
                if (dayOfWeek.ToLower().Equals(DayOfWeek.Thursday))
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
            while(scheduleAdded < numberOfSessions)
            {
                if (convertedDateOfWeek.Contains(startDate.DayOfWeek))
                {
                    schedules.Add(new Schedule
                    {
                        Id = Guid.NewGuid(),
                        ClassId = createdClass.Id,
                        Date = startDate,
                        RoomId = request.RoomId,
                        SlotId = request.ScheduleRequests[0].SlotId, // để ý cái này
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

        public async Task<List<ClassResponse>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent)
        {
            var classes = await FetchClasses();

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

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public async Task<ClassResponse> GetClassByIdAsync(Guid id)
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

            return _mapper.Map<ClassResponse>(cls);
        }

        public async Task<List<ClassResponse>> GetClassesAsync()
        {
            var classes = await FetchClasses();

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public async Task<List<ClassResponse>> GetClassesByCourseIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == id);

            var classes = course == null
                ? throw new BadHttpRequestException("Course Id Not Exist", StatusCodes.Status400BadRequest)
                : await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.CourseId == id, include: x => x
                .Include(x => x.Lecture!)
                .Include(x => x.StudentClasses)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                .ThenInclude(s => s.Slot)!
                .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                .ThenInclude(s => s.Room)!);

            var responses = classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
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
