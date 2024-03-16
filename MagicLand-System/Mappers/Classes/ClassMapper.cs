using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;

namespace MagicLand_System.Mappers.Classes
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {

            CreateMap<User, LoginResponse>()
               .ForMember(des => des.AccessToken, src => src.Ignore())
               .ForMember(des => des.Role, src => src.MapFrom(src => src.Role!.Name));


            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
                .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
                .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
                .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
                .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
                ? ClassMethodEnum.ONLINE.ToString()
                : ClassMethodEnum.OFFLINE.ToString()))
                .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()));


            CreateMap<Class, ClassWithSlotOutSideResponse>()
             .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
             .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
             .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name))
             .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
             .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
             .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
             .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
             .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
             ? ClassMethodEnum.ONLINE.ToString()
             : ClassMethodEnum.OFFLINE.ToString()))
             .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()));

            CreateMap<Class, ClassResponseForLecture>()
             .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
             .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
             .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name))
             .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
             .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
             .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
             .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
             .ForMember(dest => dest.ScheduleInfors, opt => opt.MapFrom(src => ScheduleCustomMapper.fromScheduleToScheduleWithOutLectureList(src.Schedules.ToList())))
             .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
             ? ClassMethodEnum.ONLINE.ToString()
             : ClassMethodEnum.OFFLINE.ToString()))
             .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()));

            CreateMap<Class, ClassResExtraInfor>()
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
                .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name))
                .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
                .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
                .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
                ? ClassMethodEnum.ONLINE.ToString()
                : ClassMethodEnum.OFFLINE.ToString()))
                .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()))
                .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.Lecture!)))
                .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => ScheduleCustomMapper.fromClassRelatedItemsToScheduleResWithSession(
                src.Schedules.ToList(),
                src.Course!.Syllabus != null
                ? src.Course.Syllabus.Topics!.ToList()
                : new List<Topic>())));


            CreateMap<Class, ClassWithSlotShorten>()
              .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
              .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
              .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
              .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name))
              .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
              .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
              .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
              .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
              ? ClassMethodEnum.ONLINE.ToString()
              : ClassMethodEnum.OFFLINE.ToString()))
              .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
              .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()))
              .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.Lecture!)))
              //.ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => ScheduleCustomMapper.fromScheduleToScheduleShortenResponse(src)));
              .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => ScheduleCustomMapper.fromScheduleToScheduleShortenResponses(src)));


            CreateMap<Class, ClassWithDailyScheduleRes>()
               .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
               .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassCode))
               .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name))
               .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
               .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
               .ForMember(dest => dest.Video, opt => opt.MapFrom(src => src.Video))
               .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
               .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.SubjectName))
               .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
               .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
               .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassMethodEnum.ONLINE.ToString())
               ? ClassMethodEnum.ONLINE.ToString()
               : ClassMethodEnum.OFFLINE.ToString()))
               .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
               .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Street + " " + src.District + " " + src.City))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()))
               .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.Lecture!)))
               .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => ScheduleCustomMapper.fromScheduleToDailyScheduleList(src.Schedules.ToList())));


        }
    }
}
