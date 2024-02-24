using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Syllabuses
{
    public class SyllabusMapper : Profile
    {
        public SyllabusMapper()
        {
            CreateMap<Syllabus, SyllabusResponse>()
            .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.SyllabusCategory!.Name))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.StudentTasks, opt => opt.MapFrom(src => src.StudentTasks))
            .ForMember(dest => dest.ScoringScale, opt => opt.MapFrom(src => src.ScoringScale))
            .ForMember(dest => dest.TimePerSession, opt => opt.MapFrom(src => src.TimePerSession))
            .ForMember(dest => dest.SessionsPerCourse, opt => opt.MapFrom(src => src.Topics!.SelectMany(tp => tp.Sessions!).Count()))
            .ForMember(dest => dest.MinAvgMarkToPass, opt => opt.MapFrom(src => src.MinAvgMarkToPass))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.SubjectCode))
            .ForMember(dest => dest.SyllabusLink, opt => opt.MapFrom(src => src.SyllabusLink))
            .ForMember(dest => dest.SyllabusInformations, opt => opt.MapFrom(src => SessionCustomMapper.fromTopicsToSyllabusInforResponse(src.Topics != null ? src.Topics : new List<Topic>())))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => MaterialCustomMapper.fromMaterialsToMaterialResponse(src.Materials!)))
            .ForMember(dest => dest.QuestionPackages, opt => opt.MapFrom(src => QuestionCustomMapper.fromTopicsToQuestionPackageResponse(src.Topics!)))
            .ForMember(dest => dest.Exams, opt => opt.MapFrom(src => ExamSyllabusCustomMapper.fromExamSyllabusesToExamSyllabusResponse(src.ExamSyllabuses!)));


            CreateMap<Syllabus, SyllabusWithCourseResponse>()
            .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.SyllabusCategory!.Name))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.StudentTasks, opt => opt.MapFrom(src => src.StudentTasks))
            .ForMember(dest => dest.ScoringScale, opt => opt.MapFrom(src => src.ScoringScale))
            .ForMember(dest => dest.TimePerSession, opt => opt.MapFrom(src => src.TimePerSession))
            .ForMember(dest => dest.SessionsPerCourse, opt => opt.MapFrom(src => src.Topics!.SelectMany(tp => tp.Sessions!).Count()))
            .ForMember(dest => dest.MinAvgMarkToPass, opt => opt.MapFrom(src => src.MinAvgMarkToPass))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.SubjectCode))
            .ForMember(dest => dest.SyllabusLink, opt => opt.MapFrom(src => src.SyllabusLink))
            .ForMember(dest => dest.SyllabusInformations, opt => opt.MapFrom(src => SessionCustomMapper.fromTopicsToSyllabusInforResponse(src.Topics != null ? src.Topics : new List<Topic>())))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => MaterialCustomMapper.fromMaterialsToMaterialResponse(src.Materials!)))
            .ForMember(dest => dest.QuestionPackages, opt => opt.MapFrom(src => QuestionCustomMapper.fromTopicsToQuestionPackageResponse(src.Topics!)))
            .ForMember(dest => dest.Exams, opt => opt.MapFrom(src => ExamSyllabusCustomMapper.fromExamSyllabusesToExamSyllabusResponse(src.ExamSyllabuses!)))
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => CourseCustomMapper.fromCourseToCourseSimpleResponse(src.Course!)));


        }
    }
}
