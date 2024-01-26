using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Attendances;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.Mappers.Custom
{
    public class ScheduleCustomMapper
    {
        public static List<ScheduleResWithTopic> fromClassRelatedItemsToScheduleResWithTopic(List<Schedule> schedules, List<Topic> topics)
        {
            if (!schedules.Any())
            {
                return new List<ScheduleResWithTopic>();
            }

            var responses = new List<ScheduleResWithTopic>();

            if (!topics.Any())
            {
                foreach (var schedule in schedules)
                {
                    var response = new ScheduleResWithTopic
                    {
                        DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                        Date = schedule.Date,
                        Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                        Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                        Topic = default,
                    };

                    responses.Add(response);
                }
            }
            else
            {
                int orderSchedule = 0;

                foreach (var topic in topics)
                {
                    foreach (var session in topic.Sessions)
                    {
                        //Remove after insert Success Database
                        if (orderSchedule > schedules!.Count() - 1)
                        {
                            break;
                        }
                        //

                        var schedule = schedules![orderSchedule];

                        var response = new ScheduleResWithTopic
                        {
                            DayOfWeeks = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                            Date = schedule.Date,
                            Room = RoomCustomMapper.fromRoomToRoomResponse(schedule.Room!),
                            Slot = SlotCustomMapper.fromSlotToSlotResponse(schedule.Slot!),
                            Topic = TopicCustomMapper.fromTopicToTopicResponse(topic, session),
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
                ClassName = cls.Name,
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
