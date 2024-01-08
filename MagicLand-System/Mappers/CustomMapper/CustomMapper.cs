using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Course;
using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.Slot;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.PayLoad.Response.Syllabus;
using MagicLand_System.PayLoad.Response.Topic;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.Mappers.CustomMapper
{
    public class CustomMapper
    {

        // Use to handel and support complicated mapping //
        public static CartResponse fromCartToCartResponse(Cart cart, List<Student> students, List<ClassResponseV1> cls)
        {
            if (cart == null || cart.CartItems == null || cls == null)
            {
                return new CartResponse();
            }

            // Leave Incase Error

            #region
            // Way 1
            //var cartResponses = cart.Carts.Select(cts =>
            //{
            //    var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //    List<Student> studentsForCartItem = new List<Student>();
            //    foreach (var s in cts.CartItemRelations)
            //    {
            //        studentsForCartItem.Add(students.FirstOrDefault(stu => stu.Id == s.StudentId)!);
            //    }
            //    return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //}).ToList();

            // Way 2
            //CartResponse response = new CartResponse
            //{
            //    Id = cart.Id,
            //    CartItems = cart.Carts.Select(cts =>
            //    {
            //        var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //        var studentsForCartItem = cts.CartItemRelations
            //            .Select(cir => students.FirstOrDefault(stu => stu.Id == cir.StudentId))
            //            .ToList();

            //        return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //    }).ToList()
            //};
            #endregion

            CartResponse response = new CartResponse
            {
                Id = cart.Id,
                CartItems = cart.CartItems.Select(cts => fromCartItemToCartItemResponse(
                cts.Id,
                cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!,
                cts.StudentInCarts.Select(sic => students.FirstOrDefault(stu => stu.Id == sic.StudentId))!)).ToList()
            };

            return response;
        }

        public static CartItemResponse fromCartItemToCartItemResponse(Guid cartItemId, ClassResponseV1 cls, IEnumerable<Student> students)
        {
            CartItemResponse response = new CartItemResponse
            {
                Id = cartItemId,
                Students = students.Count() == 0
                ? new List<StudentResponse>()
                : students.Select(s => fromStudentToStudentResponse(s)).ToList(),
                Class = cls
            };


            return response;
        }

        public static StudentResponse fromStudentToStudentResponse(Student student)
        {
            if (student == null)
            {
                throw new NullReferenceException();
            }

            StudentResponse response = new StudentResponse
            {
                Id = student.Id,
                FullName = student.FullName ??= "Undefine",
                Age = DateTime.Now.Year - student.DateOfBirth.Year,
                Gender = student.Gender!.ToString(),
                Avatar = student.AvatarImage ??= DefaultAvatarConstant.DefaultAvatar()
            };
            return response;
        }
        public static CourseResponse fromCourseToCourseResponse(
            Course course, 
            IEnumerable<Course> coursePrerequisites,
            ICollection<Course> coureSubsequents)
        {
            if (course == null)
            {
                return new CourseResponse();
            }

            CourseResponse response = new CourseResponse
            {
                CourseId = course.Id,
                Image = course.Image,
                Price = (decimal)course.Price,
                MainDescription = course.MainDescription,
                SubDescriptionTitle = course.SubDescriptionTitles
                .Select(sdt => fromSubDescriptionTileToSubDescriptionTitleResponse(sdt)).ToList(),
                CourseDetail = fromCourseInformationToCourseDetailResponse(course, coursePrerequisites),
                OpeningSchedules = course.Classes.Select(cls => fromClassInformationToOpeningScheduleResponse(cls)).ToList(),
                RelatedCourses = fromCourseInformationToRealtedCourseResponse(coursePrerequisites, coureSubsequents),
            };
            return response;
        }

        public static OpeningScheduleResponse fromClassInformationToOpeningScheduleResponse(Class cls)
        {
            if (cls == null)
            {
                return new OpeningScheduleResponse();
            }

            var WeekdayNumbers = cls.Schedules.Select(s => s.DayOfWeek).Distinct().ToList().Order();

            var slotInListString = cls.Schedules.Select(s => AddSuffixesTime(s.Slot!.StartTime) + " - " + AddSuffixesTime(s.Slot.EndTime))
                .Distinct().ToList();            

            
            OpeningScheduleResponse response = new OpeningScheduleResponse
            {
                ClassId = cls.Id,
                ClassName = cls.Name,
                Schedule =  string.Join("-",
                WeekdayNumbers.Select(wdn => DateTimeHelper.ConvertDateNumberToDayweek(wdn)).ToList()),
                Slot = string.Join(" / ", slotInListString),
                OpeningDay = cls.StartDate,
                Method = cls.Method,
            };

            return response;
        }

        public static CourseDetailResponse fromCourseInformationToCourseDetailResponse(Course course, IEnumerable<Course>? coursePrerequisites)
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
                Subject = course.CourseCategory.Name,
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
            if(coursePrerequisites.Count() > 0 && coureSubsequents.Count() > 0)
            {
                return new List<RelatedCourseResponse>();
            }

            var response = new List<RelatedCourseResponse>();

            if(coursePrerequisites.Count() > 0)
            {
                response.AddRange(ProgressRelatedCourse(coursePrerequisites));
            }

            if (coureSubsequents.Count() > 0)
            {
                response.AddRange(ProgressRelatedCourse(coureSubsequents));
            }

            return response;
        }

        private static string AddSuffixesTime(string slotTime)
        {
            int hour = int.Parse(slotTime.Substring(0, slotTime.IndexOf(":")));
            if(hour >= 1 && hour <= 12)
            {
                return slotTime + " AM";
            }
            return slotTime + " PM";
        }
           
        private static List<RelatedCourseResponse> ProgressRelatedCourse(IEnumerable<Course> courses)
        {
            var relatedCourses = new List<RelatedCourseResponse>();
            foreach (var course in courses)
            {
                var relatedCourseResponse = new RelatedCourseResponse
                {
                    Id = course.Id,
                    Name = course.Name,
                    Subject = course.CourseCategory.Name,
                    Image = course.Image,
                    Price = course.Price,
                    MinAgeStudent = course.MinYearOldsStudent,
                    MaxAgeStudent = course.MaxYearOldsStudent,
                };

                relatedCourses.Add(relatedCourseResponse);
            }

            return relatedCourses;
        }

        public static SyllabusResponse fromSyllabusToSyllabusResponse(CourseSyllabus courseSyllabus)
        {
            if (courseSyllabus == null)
            {
                return new SyllabusResponse();
            }

            SyllabusResponse response = new SyllabusResponse()
            {
                Name = courseSyllabus.Name ??= "Undefined",
                UpdateTime = courseSyllabus.UpdateTime,
                Topics = courseSyllabus.Topics.Select(tp => fromTopicToTopicResponse(tp)).ToList(),
            };

            return response;
        }
        public static TopicResponse fromTopicToTopicResponse(Topic topic)
        {
            if (topic == null)
            {
                return new TopicResponse();
            }

            TopicResponse response = new TopicResponse
            {
                Name = topic.Name ??= "Undefined",
                OrderNumber = topic.OrderNumber,
                Sessions = topic.Sessions.Select(s => fromSessionToSessionResponse(s)).ToList(),
            };

            return response;
        }
        public static SubDescriptionTitleResponse fromSubDescriptionTileToSubDescriptionTitleResponse(SubDescriptionTitle subDescriptionTitle)
        {
            if (subDescriptionTitle == null)
            {
                return new SubDescriptionTitleResponse();
            }
            SubDescriptionTitleResponse response = new SubDescriptionTitleResponse
            {
                Title = subDescriptionTitle.Title,
                Contents = subDescriptionTitle.SubDescriptionContents
                .Select(sdc => fromSubDescriptionContentToSubDescriptionContentResponse(sdc)).ToList(),
            };
            return response;
        }
        public static SubDescriptionContentResponse fromSubDescriptionContentToSubDescriptionContentResponse(SubDescriptionContent subDescriptionContent)
        {
            if (subDescriptionContent == null)
            {
                return new SubDescriptionContentResponse();
            }
            SubDescriptionContentResponse response = new SubDescriptionContentResponse
            {
                Content = subDescriptionContent.Content,
                Description = subDescriptionContent.Description,
            };
            return response;
        }
        public static SessionResponse fromSessionToSessionResponse(Session session)
        {
            if (session == null)
            {
                return new SessionResponse();
            }
            SessionResponse response = new SessionResponse
            {
                Id = session.Id,
                NoSession = session.NoSession,
                Content = session.Content ??= string.Empty,
                Description = session.Description ??= string.Empty
            };
            return response;
        }
        public static SlotResponse fromSlotToSlotResponse(Slot slot)
        {
            if (slot == null)
            {
                return new SlotResponse();
            }
            SlotResponse response = new SlotResponse
            {
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            };
            return response;
        }
        public static RoomResponse fromRoomToRoomResponse(Room room)
        {
            if (room == null)
            {
                return new RoomResponse();
            }
            RoomResponse response = new RoomResponse
            {
                Id = room.Id,
                Name = room.Name,
                Floor = room.Floor ?? 0,
                Status = room.Status!.ToString(),
                LinkUrl = room.LinkURL != null ? room.LinkURL : "NoLink",
                Capacity = room.Capacity,

            };
            return response;
        }
        public static UserResponse fromUserToUserResponse(User user)
        {
            if (user == null)
            {
                return new UserResponse();
            }
            UserResponse response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                AvatarImage = string.IsNullOrEmpty(user.AvatarImage) ? DefaultAvatarConstant.DefaultAvatar() : user.AvatarImage,
                Address = user.City + " " + user.District + " " + user.Street,
            };
            return response;
        }

    }
}
