﻿using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;

namespace MagicLand_System.Mappers.Classes
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {
            CreateMap<User, LoginResponse>()
               .ForMember(des => des.AccessToken, src => src.Ignore())
               .ForMember(des => des.Role, src => src.MapFrom(src => src.Role!.Name));


            CreateMap<Class, ClassResExtraInfor>()
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
                .ForMember(dest => dest.ClassSubject, opt => opt.MapFrom(src => src.Course!.CourseCategory.Name))
                .ForMember(dest => dest.LeastNumberStudent, opt => opt.MapFrom(src => src.LeastNumberStudent))
                .ForMember(dest => dest.CoursePrice, opt => opt.MapFrom(src => src.Course!.Price))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method!.ToString().Equals(ClassEnum.ONLINE.ToString())
                ? ClassEnum.ONLINE.ToString()
                : ClassEnum.OFFLINE.ToString()))
                .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.StudentClasses.Count()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.City + " " + src.District + " " + src.Street))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status!.ToString()))
                .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.Lecture!)))
                .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => ScheduleCustomMapper.fromClassRelatedItemsToScheduleResWithTopic(
                src.Schedules.ToList(), 
                src.Course!.CourseSyllabus != null
                ? src.Course.CourseSyllabus.Topics.ToList()
                : new List<Topic>())));
        }
    }
}
