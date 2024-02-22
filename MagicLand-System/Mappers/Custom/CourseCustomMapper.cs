using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.Mappers.Custom
{
    public class CourseCustomMapper
    {
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
                Price = (decimal)course.Price,
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
                Price = (decimal)course.Price,
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

        public static CourseDetailResponse fromCourseInforToCourseDetailResponse(Course course, IEnumerable<Course>? coursePrerequisites)
        {
            if (course == null)
            {
                return new CourseDetailResponse();
            }

            CourseDetailResponse response = new CourseDetailResponse
            {
                CourseName = course.Name,
                MinAgeStudent = course.MinYearOldsStudent.ToString(),
                MaxAgeStudent = course.MaxYearOldsStudent.ToString(),
                AddedDate = course.AddedDate,
                Method = string.Join(" / ", course.Classes.Select(c => c.Method!.ToString()).ToList().Distinct().ToList()),
                NumberOfSession = course.NumberOfSession,
                CoursePrerequisites = coursePrerequisites != null
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
                    //Subject = course.CourseCategory.Name,
                    Image = course.Image,
                    Price = course.Price,
                    MinAgeStudent = course.MinYearOldsStudent,
                    MaxAgeStudent = course.MaxYearOldsStudent,
                };

                relatedCourses.Add(relatedCourseResponse);
            }

            return relatedCourses;
        }
    }
}
