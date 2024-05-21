using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Message.SubMessage;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web_Dev.Pages
{
    public class TakeAttendanceModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public TakeAttendanceModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<ClassDefaultMessage> Classes { get; set; } = new List<ClassDefaultMessage>();

        [BindProperty]
        public StudentLearningInfor CurrentStudentLearningMessage { get; set; } = null;

        [BindProperty]
        public bool IsLoading { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var messages = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearning");
            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");

            if (messages != null && messages.Count > 0)
            {
                CurrentStudentLearningMessage = messages.First();
                ViewData["IndexPage"] = 0;
            }


            if (classes != null && classes.Count > 0)
            {
                Classes = classes;
            }
            else
            {
                await FetchClass();

                return Page();
            }

            return Page();
        }

        private async Task FetchClass()
        {
            var result = await _apiHelper.FetchApiAsync<List<ClassWithSlotShorten>>(ApiEndpointConstant.ClassEnpoint.GetAll, MethodEnum.GET, null);

            if (result.Data == null)
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Classes", Classes);
            }
            else
            {
                var classes = new List<ClassDefaultMessage>();
                var classFiltered = result.Data.Where(x => x.Status == ClassStatusEnum.PROGRESSING.ToString()).ToList();
                foreach (var cls in classFiltered)
                {
                    var schedules = new List<ScheduleMessage>();
                    int order = 0;
                    foreach (var schedule in cls.Schedules.OrderBy(sc => sc.Schedule))
                    {
                        schedules.Add(new ScheduleMessage
                        {
                            Slot = schedule.Slot!,
                            DayOfWeek = schedule.Schedule!,
                            Order = order++,
                        });
                    }
                    classes.Add(new ClassDefaultMessage
                    {
                        ClassId = cls.ClassId.ToString(),
                        ClassCode = cls.ClassName!,
                        CourseBeLong = cls.CourseName!,
                        StartDate = cls.StartDate.ToString("MM/dd/yyyy"),
                        LecturerBeLong = cls.Lecture!.FullName!,
                        LecturerPhone = cls.Lecture.Phone,
                        Schedules = schedules,
                    });
                }
                Classes = classes;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Classes", Classes);
            }
        }

        public async Task<IActionResult> OnPostProgressAsync(int inputField, string listId, string submitButton)
        {
            var a = new List<StudentLearningInfor>
            {
                new StudentLearningInfor
                {
                    StudentName = "a",
                    LearningInfors = new List<AttendanceAndEvaluateInfor>
                    {
                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "a",
                            Date = " a",
                            EvaluateNote = "a",
                            EvaluateStatus = "a",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "a",
                            Date = " a",
                            EvaluateNote = "a",
                            EvaluateStatus = "a",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "a",
                            Date = " a",
                            EvaluateNote = "a",
                            EvaluateStatus = "a",
                        },
                    }
                },
                 new StudentLearningInfor
                {
                    StudentName = "b",
                    LearningInfors = new List<AttendanceAndEvaluateInfor>
                    {
                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "b",
                            Date = " a",
                            EvaluateNote = "a",
                            EvaluateStatus = "a",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "b",
                            Date = " a",
                            EvaluateNote = "b",
                            EvaluateStatus = "b",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "b",
                            Date = " a",
                            EvaluateNote = "a",
                            EvaluateStatus = "a",
                        },
                    }
                },
                  new StudentLearningInfor
                {
                    StudentName = "c",
                    LearningInfors = new List<AttendanceAndEvaluateInfor>
                    {
                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "c",
                            Date = " c",
                            EvaluateNote = "c",
                            EvaluateStatus = "c",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "c",
                            Date = " c",
                            EvaluateNote = "c",
                            EvaluateStatus = "c",
                        },

                        new AttendanceAndEvaluateInfor
                        {
                            AttendanceStatus = "c",
                            Date = " c",
                            EvaluateNote = "c",
                            EvaluateStatus = "c",
                        },
                    }
                }
            };
            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataLearning", a);
            return Page();


            if (submitButton == "Refresh")
            {
                CurrentStudentLearningMessage = null;
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                return Page();
            }

            if (string.IsNullOrEmpty(listId))
            {
                ViewData["Message"] = "Lớp Chưa Được Chọn";
                CurrentStudentLearningMessage = null;
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                IsLoading = true;
                return Page();
            }

            ViewData["Message"] = "";

            var idParses = new List<string>();
            if (!string.IsNullOrEmpty(listId))
            {
                string pattern = @"\|([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\|";
                MatchCollection matches = Regex.Matches(listId, pattern);

                foreach (Match match in matches)
                {
                    idParses.Add(match.Groups[1].Value);
                }
            }

            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext!.Session, "Classes");
            if (idParses.Any())
            {
                classes = idParses.Select(id => classes.Single(c => c.ClassId == id)).ToList();
            }

            Random random = new Random();

            foreach (var cls in classes)
            {
                var evaluateNote = new List<EvaluateDataRequest>();

                EvaluateData.EvaluateNotes.ForEach(data => evaluateNote.Add(new EvaluateDataRequest
                {
                    Level = data.Item1,
                    Note = data.Item2,
                }));

                var result = await _apiHelper.FetchApiAsync<List<StudentLearningInfor>>(
                    ApiEndpointConstant.DeveloperEndpoint.TakeFullAttendanceAndEvaluate + $"?classId={cls.ClassId}&percentageAbsent={inputField}", MethodEnum.PUT, evaluateNote);
                CurrentStudentLearningMessage = result.Data.First();
                ViewData["IndexPage"] = 0;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "DataLearning", result.Data);
            }

            IsLoading = true;

            return Page();
        }

        public IActionResult OnPostTableControl(string indexPage, string tableButtonSubmit)
        {
            var messages = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearning");
            var messagesSearch = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearningSearch");

            int parseIndex = int.Parse(indexPage);
            int newIndex = tableButtonSubmit == "Next" ? parseIndex + 1 : parseIndex - 1;

            if (parseIndex == 0 && tableButtonSubmit == "Previous")
            {
                newIndex = parseIndex;
            }

            if (messagesSearch != null)
            {
                if (parseIndex == messagesSearch.Count - 1 && tableButtonSubmit == "Next")
                {
                    newIndex = parseIndex;
                }

                CurrentStudentLearningMessage = messagesSearch[newIndex];
            }
            else
            {
                if (parseIndex == messages.Count - 1 && tableButtonSubmit == "Next")
                {
                    newIndex = parseIndex;
                }

                CurrentStudentLearningMessage = messages[newIndex];
            }

            ViewData["IndexPage"] = newIndex;
            return Page();
        }

        public IActionResult OnPostSearch(string searchKey, string searchType)
        {
            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
            var messages = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearning");

            if (string.IsNullOrEmpty(searchKey))
            {
                if (classes != null && classes.Any())
                {
                    Classes = classes;
                    return Page();
                }

                if (messages != null && messages.Any())
                {
                    CurrentStudentLearningMessage = messages.First();
                }

            }

            var key = searchKey.Trim().ToLower();
            if (searchType == "MESSAGE")
            {
                messages = messages!.Where(mess => mess.StudentName.ToLower().Contains(key)).ToList();

                CurrentStudentLearningMessage = messages.First();
                SessionHelper.SetObjectAsJson(HttpContext.Session, "DataLearningSearch", messages);
            }
            if (searchType == "DATA")
            {
                Classes = classes!.Where(
                    c => c.ClassCode.ToLower().Contains(key) ||
                    c.CourseBeLong.ToLower().Contains(key) ||
                    c.LecturerBeLong.ToLower().Contains(key)
                    ).ToList();
            }

            return Page();
        }
    }
}
