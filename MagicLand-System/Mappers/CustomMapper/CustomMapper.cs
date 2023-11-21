using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Mappers.CustomMapper
{
    public class CustomMapper
    {
        // Use to handel and support complicated mapping //
        public static CourseResponse fromCourseToCourseResponse(Course course, IEnumerable<Course>? coursePrerequisites)
        {
            if (course == null)
            {
                return new CourseResponse();
            }

            CourseResponse response = new CourseResponse
            {
                Id = course.Id,
                Name = course.Name,
                NumberOfSession = course.NumberOfSession,
                MinAgeStudent = course.MinYearOldsStudent,
                MaxAgeStudent = course.MaxYearOldsStudent,
                Image = course.Image,
                CoursePrerequisites = coursePrerequisites != null
                ? coursePrerequisites.Select(c => fromCourseToCourseResponse(c, null)).ToList()
                : new List<CourseResponse>()
            };
            return response;
        }

        public static SessionResponse fromSessionToSessionResponse(Session session)
        {
            if (session == null)
            {
                return new SessionResponse();
            }
            SessionResponse response = new SessionResponse
            {
                Id = session.Id,
                DayOfWeek = DateTimeHelper.GetDatesFromDateFilter(session.DayOfWeek)[0].ToString(),
                Date = session.Date,
                Slot = fromSlotToSlotResponse(session.Slot),
                RoomResponse = fromRoomToRoomResponse(session.Room)
            };
            return response;
        }

        public static SlotResponse fromSlotToSlotResponse(Slot slot)
        {
            if (slot == null)
            {
                return new SlotResponse();
            }
            SlotResponse response = new SlotResponse
            {
               StartTime = TimeOnly.Parse(slot.StartTime),
               EndTime = TimeOnly.Parse(slot.EndTime)
            };
            return response;
        }

        public static RoomResponse fromRoomToRoomResponse(Room room)
        {
            if (room == null)
            {
                return new RoomResponse();
            }
            RoomResponse response = new RoomResponse
            {
                Id = room.Id,
                Name = room.Name,
                Floor = room.Floor,
                Status = room.Status!.ToString(),
                LinkUrl = room.LinkURL != null ? room.LinkURL : "NoLink"
            };
            return response;
        }
        public static UserResponse fromUserToUserResponse(User user)
        {
            if (user == null)
            {
                return new UserResponse();
            }
            UserResponse response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = fromAddressToAddressResponse(user.Address),
                AvatarImage = string.IsNullOrEmpty(user.AvatarImage) ? DefaultAvatarConstant.DefaultAvatar() : user.AvatarImage,
            };
            return response;
        }

        public static AddressResponse fromAddressToAddressResponse(Address address)
        {
            if (address == null)
            {
                return new AddressResponse();
            }
            AddressResponse response = new AddressResponse
            {
                Id = address.Id,
                Street = address.Street,
                District = address.District,
                City = address.City
            };
            return response;
        }
    }
}
