using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Courses.Custom;
using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.Mappers.Custom
{
    public class CourseCustomMapper
    {
        public static CourseSimpleResponse fromCourseToCourseSimpleResponse(Course course)
        {
            if (course == null)
            {
                return new CourseSimpleResponse();
            }

            var response = new CourseSimpleResponse
            {
                CourseId = course.Id,
                CourseName = course.Name,
            };

            return response;
        }
        public static CourseResponse fromCourseToCourseResponse(Course course)
        {
            if (course == null)
            {
                return new CourseResponse();
            }

            var response = new CourseResponse
            {
                CourseId = course.Id,
                Image = course.Image,
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => CourseDescriptionCustomMapper.fromSubDesTileToSubDesTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInforToCourseDetailResponse(course, default),
            };

            return response;
        }
        public static CourseResExtraInfor fromCourseToCourseResExtraInfor(
         Course course,
         IEnumerable<Course> coursePrerequisites,
         ICollection<Course> coureSubsequents)
        {
            if (course == null)
            {
                return new CourseResExtraInfor();
            }

            var response = new CourseResExtraInfor
            {
                CourseId = course.Id,
                Image = course.Image,
                //Price = (decimal)course.Price,
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => CourseDescriptionCustomMapper.fromSubDesTileToSubDesTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInforToCourseDetailResponse(course, coursePrerequisites),
                OpeningSchedules = course.Classes.Select(cls => ScheduleCustomMapper.fromClassInforToOpeningScheduleResponse(cls)).ToList(),
                RelatedCourses = fromCourseInformationToRealtedCourseResponse(coursePrerequisites, coureSubsequents),
                UpdateDate = course.UpdateDate,
            };

            return response;
        }

        public static CourseWithScheduleShorten fromCourseToCourseWithScheduleShorten(
      Course course,
      Guid studentId,
      IEnumerable<Course> coursePrerequisites,
      ICollection<Course> coureSubsequents)
        {
            if (course == null)
            {
                return new CourseWithScheduleShorten();
            }

            var classOpeningInfors = new List<ClassOpeningInfor>();
            foreach (var cls in course.Classes)
            {
                classOpeningInfors.Add(new ClassOpeningInfor()
                {
                    ClassId = cls.Id,
                    OpeningDay = cls.StartDate,
                    Schedules = ScheduleCustomMapper.fromScheduleToScheduleShortenResponses(cls),
                });
            }

            var response = new CourseWithScheduleShorten
            {
                CourseId = course.Id,
                Image = course.Image,
                //Price = (decimal)course.Price,
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => CourseDescriptionCustomMapper.fromSubDesTileToSubDesTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInforToCourseDetailResponse(course, coursePrerequisites),
                ClassOpeningInfors = classOpeningInfors,
                RelatedCourses = fromCourseInformationToRealtedCourseResponse(coursePrerequisites, coureSubsequents),
                UpdateDate = course.UpdateDate,
            };

            var classRegistereds = course.Classes.Where(cls => cls.StudentClasses.Any(sc => sc.StudentId == studentId));
            var status = new List<(string, int)>();
            if (classRegistereds != null && classRegistereds.Any())
            {
                foreach (var cls in classRegistereds)
                {
                    if (cls.Status == ClassStatusEnum.CANCELED.ToString())
                    {
                        continue;
                    }

                    status.Add(new(cls.Status!, cls.Status == ClassStatusEnum.UPCOMING.ToString() ? 1 : cls.Status == ClassStatusEnum.PROGRESSING.ToString() ? 2 : 3));
                }

                response.Status = status.OrderByDescending(x => x.Item2).First().Item1;
            }

            return response;
        }

        public static CourseDetailResponse fromCourseInforToCourseDetailResponse(Course course, IEnumerable<Course>? coursePrerequisites)
        {
            if (course == null)
            {
                return new CourseDetailResponse();
            }
            CourseDetailResponse response = new CourseDetailResponse
            {
                Id = course.Id,
                CourseName = course.Name,
                Subject = course.SubjectName,
                SubjectCode = course.Syllabus!.SubjectCode,
                MinAgeStudent = course.MinYearOldsStudent.ToString(),
                MaxAgeStudent = course.MaxYearOldsStudent.ToString(),
                AddedDate = course.AddedDate,
                Method = string.Join(" / ", course.Classes.Select(c => c.Method!.ToString()).ToList().Distinct().ToList()),
                NumberOfSession = course.NumberOfSession,
                CoursePrerequisites = coursePrerequisites != null && coursePrerequisites.Any()
                ? coursePrerequisites.Select(cp => cp.Name).ToList()!
                : new List<string>(),
            };

            return response;
        }

        public static List<RelatedCourseResponse> fromCourseInformationToRealtedCourseResponse(
         IEnumerable<Course> coursePrerequisites,
         IEnumerable<Course> coureSubsequents)
        {
            if (coursePrerequisites.Count() > 0 && coureSubsequents.Count() > 0)
            {
                return new List<RelatedCourseResponse>();
            }

            var response = new List<RelatedCourseResponse>();

            if (coursePrerequisites.Count() > 0)
            {
                response.AddRange(ProgressRelatedCourse(coursePrerequisites));
            }

            if (coureSubsequents.Count() > 0)
            {
                response.AddRange(ProgressRelatedCourse(coureSubsequents));
            }

            return response;
        }

        private static List<RelatedCourseResponse> ProgressRelatedCourse(IEnumerable<Course> courses)
        {
            var relatedCourses = new List<RelatedCourseResponse>();
            foreach (var course in courses)
            {
                var relatedCourseResponse = new RelatedCourseResponse
                {
                    CourseRelatedId = course.Id,
                    Name = course.Name,
                    Subject = course.SubjectName,
                    Image = course.Image,
                    //Price = course.Price,
                    MinAgeStudent = course.MinYearOldsStudent,
                    MaxAgeStudent = course.MaxYearOldsStudent,
                };

                relatedCourses.Add(relatedCourseResponse);
            }

            return relatedCourses;
        }
    }
}
