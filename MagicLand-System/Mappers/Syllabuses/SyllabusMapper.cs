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
            CreateMap<CourseSyllabus, SyllabusResponse>()
            .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.StudentTasks, opt => opt.MapFrom(src => src.StudentTasks))
            .ForMember(dest => dest.ScoringScale, opt => opt.MapFrom(src => src.ScoringScale))
            .ForMember(dest => dest.TimePerSession, opt => opt.MapFrom(src => src.TimePerSession))
            .ForMember(dest => dest.MinAvgMarkToPass, opt => opt.MapFrom(src => src.MinAvgMarkToPass))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.SubjectCode))
            .ForMember(dest => dest.SyllabusLink, opt => opt.MapFrom(src => src.SyllabusLink))
            .ForMember(dest => dest.SyllabusInformations, opt => opt.MapFrom(src => SessionCustomMapper.fromTopicsToSyllabusInforResponse(src.Topics != null ? src.Topics : new List<Topic>())))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => MaterialCustomMapper.fromMaterialsToMaterialResponse(src.Materials)))
            .ForMember(dest => dest.QuestionPackages, opt => opt.MapFrom(src => QuestionPackageCustomMapper.fromTopicsToQuestionPackageResponse(src.Topics)))
            .ForMember(dest => dest.Exams, opt => opt.MapFrom(src => ExamSyllabusCustomMapper.fromExamSyllabusesToExamSyllabusResponse(src.ExamSyllabuses)));
        }
    }
}
