using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
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
                Address = fromAddressToAddressResponse(user.Address)
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
