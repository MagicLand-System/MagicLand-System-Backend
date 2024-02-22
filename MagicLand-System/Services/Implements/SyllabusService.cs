using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.PayLoad.Response.Topics;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class SyllabusService : BaseService<SyllabusService>, ISyllabusService
    {
        public SyllabusService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<SyllabusService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword)
        {
            var syllabuses = await _unitOfWork.GetRepository<CourseSyllabus>().GetListAsync(include : x => x.Include(x => x.Course));
            List<SyllabusResponseV2> responses = new List<SyllabusResponseV2>();
            foreach(var syl in syllabuses)
            {
                var name = "undefined";
                var subjectCode = "undefined";
                var syllabusName = "undefined"; 
                if (syl.Course != null)
                {
                    name = syl.Course.Name;
                }
                if(syl.SubjectCode != null)
                {
                    subjectCode = syl.SubjectCode;
                }
                if(syl.Name != null)
                {
                    syllabusName = syl.Name;
                }
                if(syl.SubjectCode != null) { }
                SyllabusResponseV2 syllabusResponseV2 = new SyllabusResponseV2
                {
                    Id = syl.Id,
                    CourseName = name,
                    EffectiveDate = syl.EffectiveDate,
                    SubjectCode = subjectCode,
                    SyllabusName = syllabusName,
                };
                responses.Add(syllabusResponseV2);  
            }
            if (keyword != null)
            {
                responses = (responses.Where(x => (x.SyllabusName.ToLower().Trim().Contains(keyword.ToLower().Trim()) || x.SubjectCode.ToLower().Trim().Contains(keyword.ToLower().Trim())))).ToList();
            }
            return responses;
        }

        public async Task<SyllabusResponse> GetSyllasbusResponse(string courseId)
        {
            CourseSyllabus courseSyllabus = (await _unitOfWork.GetRepository<CourseSyllabus>().SingleOrDefaultAsync(predicate :  x => x.CourseId.ToString().Equals(courseId), include : x => x.Include(x => x.Course).Include(x => x.Topics)));
            if(courseSyllabus == null)
            {
                return new SyllabusResponse();
            }
            var topics = courseSyllabus.Topics;
            foreach(var topic in topics)
            {
                var sessons = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(topic.Id.ToString()));
                (courseSyllabus.Topics.Single(x => x.Id.ToString() == topic.Id.ToString())).Sessions = sessons;
            }
            List<TopicResponse> topicResponses = new List<TopicResponse>(); 
            foreach(var topic in topics)
            {
                var sessonsx = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(topic.Id.ToString()));
                List<SessionResponse> sessons = new List<SessionResponse>();
                foreach (var session in sessonsx)
                {
                    SessionResponse sessionResponse = new SessionResponse
                    {
                        Description = session.Description,
                        NoOfSession = session.NoSession,
                    };
                    sessons.Add(sessionResponse);
                }
                sessons =  sessons.OrderBy(x => x.NoOfSession).ToList();
                TopicResponse topicResponse = new TopicResponse
                {
                    OrderNumber = topic.OrderNumber,
                    TopicName = topic.Name,
                    Sessions = sessons

                };
                topicResponses.Add(topicResponse);
            }
            topicResponses = topicResponses.OrderBy(x => x.OrderNumber).ToList();
            SyllabusResponse syllabusResponse = new SyllabusResponse
            {
                SyllabusId = courseSyllabus.Id,
                Name = courseSyllabus.Name,
                UpdateTime = courseSyllabus.UpdateTime,
                Topics = topicResponses
            };
            return syllabusResponse;
        }
    }
}
