using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace MagicLand_System.Services.Implements
{
    public class RoomService : BaseService<RoomService>, IRoomService
    {
        public RoomService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<RoomService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<AdminRoomResponse>> GetAdminRoom(DateTime? startDate, DateTime? endDate,string? searchString, string? slotId)
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync(include : x => x.Include(x => x.Schedules).ThenInclude(x => x.Slot).Include(x => x.Schedules).ThenInclude(x => x.Class));
            List<AdminRoomResponse> responses = new List<AdminRoomResponse>();
            foreach (var room in rooms)
            {
                var schedules = room.Schedules;
                if(rooms.Count > 0) 
                {
                    foreach (var schedule in schedules)
                    {
                        var response = new AdminRoomResponse
                        {
                            Capacity = room.Capacity,
                            Date = schedule.Date,
                            EndTime = schedule.Slot.EndTime,
                            StartTime = schedule.Slot.StartTime,
                            Floor = room.Floor,
                            LinkURL = room.LinkURL,
                            Name = room.Name,
                            ClassCode = schedule.Class.ClassCode,
                        };
                        responses.Add(response);
                    }
                }
            }
            if (responses.Count > 0)
            {
                if(searchString != null)
                {
                    responses = responses.Where(x =>( x.Name.ToLower().Trim().Contains(searchString.ToLower().Trim()))).ToList();
                }
                
                if(startDate != null)
                {
                    responses = responses.Where(x => x.Date >= startDate).ToList();
                }
                if(endDate != null)
                {
                    endDate = endDate.Value.AddHours(23); 
                    responses = responses.Where(x => x.Date <= endDate).ToList();
                }
                if(slotId != null)
                {
                    var startTime = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(slotId), selector: x => x.StartTime);
                    responses = responses.Where(x => x.StartTime.Equals(startTime)).ToList();
                } 
                responses = responses.OrderByDescending(x => x.Date).ThenBy(x => x.Name).ToList();

            }
            return responses;
        }

        public async  Task<List<AdminRoomResponseV2>> GetAdminRoomV2(DateTime date)
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync(include: x => x.Include(x => x.Schedules).ThenInclude(x => x.Slot).Include(x => x.Schedules).ThenInclude(x => x.Class));
            List<AdminRoomResponseV2> responses = new List<AdminRoomResponseV2>();
            foreach (var room in rooms)
            {
                var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
                List<RoomSchedule> roomSchedules = new List<RoomSchedule>();
                foreach (var slot in slots)
                {
                    var isExist = room.Schedules.Where(x => (x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year && x.SlotId == slot.Id)).Any();
                    var filterSlot = room.Schedules.SingleOrDefault(x => (x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year && x.SlotId == slot.Id));
                    string classCode = string.Empty;
                    if (filterSlot != null)
                    {
                        classCode = filterSlot.Class.ClassCode;
                    }
                    LecturerResponse lecturer = new LecturerResponse();
                    if(classCode != string.Empty)
                    {
                        var classRes = await GetClassFromClassCode(classCode);
                        lecturer = classRes.LecturerResponse;
                    }

                    var sch = new RoomSchedule
                    {
                        Date = date,
                        ClassCode = classCode,
                        EndTime = slot.EndTime,
                        IsUse = isExist,
                        StartTime = slot.StartTime,
                        LecturerResponse = lecturer,
                    };
                    roomSchedules.Add(sch);
                }
                roomSchedules = roomSchedules.OrderBy(x => x.StartTime).ToList();
                var roomR = new AdminRoomResponseV2
                {
                    Capacity = room.Capacity,
                    Name = room.Name,
                    Floor = room.Floor,
                    LinkURL = room.LinkURL,
                    Schedules = roomSchedules,
                };
                responses.Add(roomR);
            }
            return responses.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<Room>> GetRoomList(FilterRoomRequest? request)
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync();
            if(rooms == null)
            {
                return null;
            } 
            if(request != null)
            {
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
                    foreach(var convert in convertSchedule) 
                    {
                        var newFilter = allSchedule.Where(x => (x.DayOfWeek == convert.DateOfWeek && x.SlotId.ToString().Equals(convert.SlotId.ToString()))).ToList();
                        if(newFilter != null)
                        {
                            result.AddRange(newFilter);
                        } 
                    }
                    List<Guid> roomIds = new List<Guid>();
                    List<Room> exceptRooms = new List<Room>();
                    if(result.Count > 0)
                    {
                        var groupByRoom = result.GroupBy(x => x.RoomId);
                        roomIds = groupByRoom.Select(x => x.Key).ToList();

                    }
                    if(roomIds.Count == 0) {
                        return rooms.OrderBy(x => x.Name).ToList();
                    }
                    List<Room> finalResult = new List<Room>();
                    foreach (var room in rooms)
                    {
                        if (!roomIds.Contains(room.Id))
                        {
                            finalResult.Add(room);
                        }
                    }
                    return finalResult.OrderBy(x => x.Name).ToList();
                }
            }
            return rooms.OrderBy(x => x.Name).ToList();
        }

        private async Task<ClassFromClassCode> GetClassFromClassCode(string classCode)
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
                Status = course.Status,
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
    }
}
