using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Attendances;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Slots;
using System.Dynamic;

namespace MagicLand_System.Mappers.Custom
{
    public class ScheduleCustomMapper
    {
        public static ScheduleCurrentLectureResponse fromClassAfterMapperToScheduleCurrentLectureResponse(Schedule schedule)
        {
            if (schedule == null)
            {
                return new ScheduleCurrentLectureResponse();
            }

            var response = new ScheduleCurrentLectureResponse
            {
                ScheduleId = schedule.Id,
                DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                Date = schedule.Date,
                Slot = SlotCustomMapper.fromSlotToSlotForLecturerResponse(schedule.Slot!),
                Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
            };

            return response;
        }

        public static List<ScheduleWithoutLectureResponse> fromScheduleToScheduleWithOutLectureList(List<Schedule> schedules)
        {
            if (schedules == null)
            {
                return new List<ScheduleWithoutLectureResponse>();
            }

            var responses = new List<ScheduleWithoutLectureResponse>();
            foreach (var schedule in schedules)
            {
                responses.Add(new ScheduleWithoutLectureResponse
                {
                    ScheduleId = schedule.Id,
                    DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                    Date = schedule.Date,
                    Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                    Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                });
            }

            return responses;
        }
        public Guid? Id { get; set; }
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }

        public SlotResponse Slot { get; set; } = new SlotResponse();
        public RoomResponse Room { get; set; } = new RoomResponse();
        public static List<DailySchedule> fromScheduleToDailyScheduleList(List<Schedule> schedules)
        {
            if (schedules == null)
            {
                return new List<DailySchedule>();
            }

            var responses = new List<DailySchedule>();
            foreach (var schedule in schedules)
            {
                responses.Add(new DailySchedule
                {
                    DayOfWeek = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                    StartTime = schedule.Slot!.StartTime,
                    EndTime = schedule.Slot!.EndTime,
                });
            }

            return responses;
        }

        public static List<LectureScheduleResponse> fromClassToListLectureScheduleResponse(Class cls)
        {
            var responses = new List<LectureScheduleResponse>();

            foreach (var schedule in cls.Schedules)
            {
                var response = new LectureScheduleResponse
                {
                    ClassId = cls.Id,
                    ClassCode = cls.ClassCode!,
                    ClassName = cls.ClassCode!,
                    ClassSubject = cls.Course!.SubjectName!,
                    Address = cls.City + " " + cls.District + " " + cls.Street,
                    Method = cls.Method!,
                    ScheduleId = schedule.Id,
                    DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                    Date = schedule.Date,
                    Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                    Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                };
                responses.Add(response);
            }

            return responses;
        }

        public static List<ScheduleResWithSession> fromClassRelatedItemsToScheduleResWithSession(List<Schedule> schedules, List<Topic> topics)
        {
            if (!schedules.Any())
            {
                return new List<ScheduleResWithSession>();
            }

            var responses = new List<ScheduleResWithSession>();

            if (!topics.Any())
            {
                foreach (var schedule in schedules)
                {
                    var response = new ScheduleResWithSession
                    {
                        ScheduleId = schedule.Id,
                        DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                        Date = schedule.Date,
                        Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                        Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                    };

                    responses.Add(response);
                }
            }
            else
            {
                int orderSchedule = 0;

                foreach (var topic in topics)
                {
                    foreach (var session in topic.Sessions!)
                    {
                        //Remove after insert Success Database
                        if (orderSchedule > schedules!.Count() - 1)
                        {
                            break;
                        }
                        //

                        var schedule = schedules![orderSchedule];

                        var response = new ScheduleResWithSession
                        {
                            ScheduleId = schedule.Id,
                            DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                            Date = schedule.Date,
                            Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                            Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                            Session = SessionCustomMapper.fromSessionToSessionResponse(session, topic)
                        };

                        responses.Add(response);
                        orderSchedule++;
                    }
                }
            }

            return responses;
        }
        public static OpeningScheduleResponse fromClassInforToOpeningScheduleResponse(Class cls)
        {
            if (cls == null)
            {
                return new OpeningScheduleResponse();
            }

            var WeekdayNumbers = cls.Schedules.Select(s => s.DayOfWeek).Distinct().ToList().Order();

            var slotInListString = cls.Schedules.Select(s => AddSuffixesTime(s.Slot!.StartTime) + " - " + AddSuffixesTime(s.Slot.EndTime))
                .Distinct().ToList();


            OpeningScheduleResponse response = new OpeningScheduleResponse
            {
                ClassId = cls.Id,
                Schedule = string.Join("-",
                WeekdayNumbers.Select(wdn => DateTimeHelper.ConvertDateNumberToDayweek(wdn)).ToList()),
                Slot = string.Join(" / ", slotInListString),
                OpeningDay = cls.StartDate,
                Method = cls.Method,
            };

            return response;
        }

        public static ScheduleWithAttendanceResponse fromClassScheduleToScheduleWithAttendanceResponse(Schedule schedule)
        {
            if (schedule == null)
            {
                return new ScheduleWithAttendanceResponse();
            }
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AddProfile<AttendancesMapper>();
            });
            var mapper = new Mapper(config);

            var response = new ScheduleWithAttendanceResponse
            {
                DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                Date = schedule.Date,
                Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                AttendanceInformation = schedule.Attendances.Select(att => mapper.Map<AttendanceResponse>(att)).ToList()
            };

            return response;
        }


        private static string AddSuffixesTime(string slotTime)
        {
            int hour = int.Parse(slotTime.Substring(0, slotTime.IndexOf(":")));
            if (hour >= 1 && hour <= 12)
            {
                return slotTime + " AM";
            }
            return slotTime + " PM";
        }

    }
}
