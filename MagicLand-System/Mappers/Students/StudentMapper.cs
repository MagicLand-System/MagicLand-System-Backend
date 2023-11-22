using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Student;

namespace MagicLand_System.Mappers.Students
{
    public class StudentMapper : Profile
    {
        public StudentMapper()
        {
            CreateMap<CreateStudentRequest,Student>();
        }
    }
}
