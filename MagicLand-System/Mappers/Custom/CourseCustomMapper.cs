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
        public static CourseResExtraInfor fromCourseToCourseResExtraInfor(Course course, List<Course> coursePrerequisites, List<Course> relatedCourses)
        {
            if (course == null)
            {
                return new CourseResExtraInfor();
            }

            var response = new CourseResExtraInfor
            {
                CourseId = course.Id,
                Image = course.Image,
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => CourseDescriptionCustomMapper.fromSubDesTileToSubDesTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInforToCourseDetailResponse(course, coursePrerequisites),
                OpeningSchedules = course.Classes.Select(cls => ScheduleCustomMapper.fromClassInforToOpeningScheduleResponse(cls)).ToList(),
                RelatedCourses = fromCourseInformationToRealtedCourseResponse(relatedCourses),
                UpdateDate = course.UpdateDate,
            };

            return response;
        }

        public static CourseWithScheduleShorten fromCourseToCourseWithScheduleShorten(Course course, Guid studentId, List<Course> prequisiteCourses, List<Course> relatedCourses)
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
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => CourseDescriptionCustomMapper.fromSubDesTileToSubDesTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInforToCourseDetailResponse(course, prequisiteCourses),
                ClassOpeningInfors = classOpeningInfors,
                RelatedCourses = fromCourseInformationToRealtedCourseResponse(relatedCourses),
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

        public static CourseDetailResponse fromCourseInforToCourseDetailResponse(Course course, List<Course>? prequisiteCourses)
        {
            if (course == null)
            {
                return new CourseDetailResponse();
            }

            var response = new CourseDetailResponse
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
                CoursePrerequisites = prequisiteCourses != null && prequisiteCourses.Any()
                ? prequisiteCourses.Select(cp => cp.Name).ToList()!
                : new List<string>(),
            };

            return response;
        }

        public static List<RelatedCourseResponse> fromCourseInformationToRealtedCourseResponse(List<Course> relatedCourses)
        {
            if (relatedCourses.Count == 0)
            {
                return new List<RelatedCourseResponse>();
            }

            var responses = new List<RelatedCourseResponse>();

            foreach (var course in relatedCourses)
            {
                var response = new RelatedCourseResponse
                {
                    CourseRelatedId = course.Id,
                    Name = course.Name,
                    Subject = course.SubjectName,
                    Image = course.Image,
                    MinAgeStudent = course.MinYearOldsStudent,
                    MaxAgeStudent = course.MaxYearOldsStudent,
                };

                responses.Add(response);
            }

            return responses;
        }
    }
}
