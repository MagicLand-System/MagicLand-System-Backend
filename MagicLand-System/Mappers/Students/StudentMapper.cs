﻿using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Mappers.Students
{
    public class StudentMapper : Profile
    {
        public StudentMapper()
        {
            CreateMap<CreateStudentRequest, Student>();

            CreateMap<Student, StudentResponse>()
           .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
           .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.AvatarImage))
           .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.DateOfBirth.Year))
           .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));


            CreateMap<Student, StudentStatisticResponse>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.AvatarImage))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.DateOfBirth.Year))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.AddedTime, opt => opt.MapFrom(src => src.AddedTime));
        }
    }
}
