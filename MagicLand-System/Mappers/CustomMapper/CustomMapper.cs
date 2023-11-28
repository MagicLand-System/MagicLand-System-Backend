using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Address;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Course;
using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.Slot;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.Mappers.CustomMapper
{
    public class CustomMapper
    {

        // Use to handel and support complicated mapping //
        public static CartResponse fromCartToCartResponse(Cart cart, List<Student> students, List<ClassResponse> cls)
        {
            if (cart == null || cart.CartItems == null || students.Count == 0 || cls == null)
            {
                return new CartResponse();
            }

            // Leave Incase Error

            // Way 1
            //var cartResponses = cart.Carts.Select(cts =>
            //{
            //    var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //    List<Student> studentsForCartItem = new List<Student>();
            //    foreach (var s in cts.CartItemRelations)
            //    {
            //        studentsForCartItem.Add(students.FirstOrDefault(stu => stu.Id == s.StudentId)!);
            //    }
            //    return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //}).ToList();

            // Way 2
            //CartResponse response = new CartResponse
            //{
            //    Id = cart.Id,
            //    CartItems = cart.Carts.Select(cts =>
            //    {
            //        var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //        var studentsForCartItem = cts.CartItemRelations
            //            .Select(cir => students.FirstOrDefault(stu => stu.Id == cir.StudentId))
            //            .ToList();

            //        return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //    }).ToList()
            //};


            CartResponse response = new CartResponse
            {
                Id = cart.Id,
                CartItems = cart.CartItems.Select(cts => fromCartItemToCartItemResponse(
                cts.Id,
                cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!,
                cts.StudentInCarts.Select(cir => students.FirstOrDefault(stu => stu.Id == cir.StudentId))!)).ToList()
            };

            return response;
        }

        public static CartItemResponse fromCartItemToCartItemResponse(Guid cartItemId, ClassResponse cls, IEnumerable<Student> students)
        {
            CartItemResponse response = new CartItemResponse
            {
                Id = cartItemId,
                CurrentTotalPrice = (double)(cls.Price * students.Count()),
                Students = students.Select(s => fromStudentToStudentResponse(s)).ToList(),
                Class = cls
            };


            return response;
        }

        public static StudentResponse fromStudentToStudentResponse(Student student)
        {
            if (student == null)
            {
                throw new NullReferenceException();
            }

            StudentResponse response = new StudentResponse
            {
                Id = student.Id,
                FullName = student.FullName,
                Age = DateTime.Now.Year - student.DateOfBirth.Year,
                Gender = student.Gender.ToString(),
                Avatar = student.AvatarImage ??= DefaultAvatarConstant.DefaultAvatar()
            };
            return response;
        }
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

        public static SessionResponse fromSessionToSessionResponse(Schedule session)
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
                Floor = room.Floor ?? 0,
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
                //Address = fromAddressToAddressResponse(user.Address),
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
