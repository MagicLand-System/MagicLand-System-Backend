using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Mappers.Custom
{
    public class StudentCustomMapper
    {
        public static StudentResponse fromStudentToStudentResponse(Student student)
        {
            if (student == null)
            {
                throw new NullReferenceException();
            }

            StudentResponse response = new StudentResponse
            {
                StudentId = student.Id,
                FullName = student.FullName ??= "Undefine",
                Age = DateTime.Now.Year - student.DateOfBirth.Year,
                Gender = student.Gender!.ToString(),
                AvatarImage = student.AvatarImage ??= ImageUrlConstant.DefaultAvatar(),
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
            };

            return response;
        }
    }
}
