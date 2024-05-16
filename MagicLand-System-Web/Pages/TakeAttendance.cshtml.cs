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
            var classes = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearning");
            int parseIndex = int.Parse(indexPage);
            int newIndex = tableButtonSubmit == "Next" ? parseIndex + 1 : parseIndex - 1;

            if (parseIndex == 0 && tableButtonSubmit == "Previous")
            {
                newIndex = parseIndex;
            }
            if (parseIndex == classes.Count - 1 && tableButtonSubmit == "Next")
            {
                newIndex = parseIndex;
            }

            CurrentStudentLearningMessage = classes[newIndex];
            ViewData["IndexPage"] = newIndex;

            return Page();
        }

        public IActionResult OnPostSearch(string searchKey, string searchType)
        {

            //if (string.IsNullOrEmpty(searchKey))
            //{
            //    CurrentStudentLearningMessage = null;
            //    Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
            return Page();
            //}

            //var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");

            //var key = searchKey.Trim().ToLower();
            //if (searchType == "MESSAGE")
            //{
            //    var messages = SessionHelper.GetObjectFromJson<List<StudentLearningInfor>>(HttpContext.Session, "DataLearning");

            //    StudentMessages = messages.Where(
            //       mess => mess.StudentName.ToLower().Contains(key) ||
            //       mess.ParentBelong.ToLower().Contains(key) ||
            //       mess.AccountArise.ToLower().Contains(key)
            //       ).ToList();
            //}
            //if (searchType == "DATA")
            //{
            //    var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");

            //    Parents = parents.Where(
            //        c => c.FullName.ToLower().Contains(key) ||
            //        c.Phone.ToLower().Contains(key)
            //        ).ToList();
            //}

            //return Page();
        }
    }
}
