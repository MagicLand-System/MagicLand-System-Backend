using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.PayLoad.Response;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;
using MagicLand_System_Web_Dev.Pages.Message.SubMessage;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System_Web_Dev.Pages
{
    public class RegisterClassModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public RegisterClassModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<ClassDefaultMessage> Classes { get; set; } = new List<ClassDefaultMessage>();

        [BindProperty]
        public List<RegisterInforMessage> RegisterInforMessages { get; set; } = new List<RegisterInforMessage>();

        [BindProperty]
        public bool IsLoading { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = false;
            var messages = SessionHelper.GetObjectFromJson<List<RegisterInforMessage>>(HttpContext.Session, "DataRegister");
            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
            var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");


            if (messages != null && messages.Count > 0)
            {
                RegisterInforMessages = messages;
            }

            if (parents == null || parents.Count == 0)
            {
                await FetchParent();
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

        private async Task FetchParent()
        {
            var result = await _apiHelper.FetchApiAsync<List<User>>(ApiEndpointConstant.UserEndpoint.RootEndpoint + "?role=" + RoleEnum.PARENT.ToString(), MethodEnum.GET, null);

            var parents = new List<LoginResponse>();
            foreach (var user in result.Data)
            {
                var authen = await _apiHelper.FetchApiAsync<LoginResponse>(ApiEndpointConstant.AuthenticationEndpoint.Authentication, MethodEnum.POST, new LoginRequest { Phone = user.Phone! });
                parents.Add(authen.Data);
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", parents);

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
                var classFiltered = result.Data.Where(x => x.Status == ClassStatusEnum.UPCOMING.ToString()).ToList();
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
                        StudentRegistered = cls.NumberStudentRegistered,
                        MinStudentRegistered = cls.LeastNumberStudent,
                        MaxStudentRegistered = cls.LimitNumberStudent,
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
                Classes.Clear();
                await FetchClass();
                return Page();
            }

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                IsLoading = true;
                return Page();
            }

            if (string.IsNullOrEmpty(listId))
            {
                ViewData["Message"] = "Lớp Chưa Được Chọn";
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
            var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext!.Session, "Parents");
            var idStudentStored = new List<Guid>();
            var phoneParentStored = new List<string>();
            var numberStudentLoop = new List<(string, int)>();

            var studentStored = new List<(string, List<Student>)>();

            int numberStudentRegistered = 0, numberParentLoop = 0;

            foreach (var cls in classes)
            {
                if (cls.MaxStudentRegistered < inputField)
                {
                    ViewData["Message"] = "Số Lượng Đăng Ký Vượt Quá Số Lượng Cho Phép Của Lớp Được Chọn";
                    Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                    IsLoading = true;
                    return Page();
                }

                while (numberStudentRegistered < inputField)
                {
                    if (numberParentLoop == parents.Count)
                    {
                        RegisterInforMessages.Add(new RegisterInforMessage
                        {
                            Status = "404",
                            Note = "Không Thể Tìm Thấy Học Sinh Hợp Lệ",
                        });

                        break;
                    }

                    LoginResponse parent;
                    do
                    {
                        parent = parents[random.Next(0, parents.Count)];
                    } while (phoneParentStored.Contains(parent.Phone));

                    var currentParentStudents = studentStored.FirstOrDefault(storage => storage.Item1 == parent.Phone).Item2;
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", parent.AccessToken!);
                    if (currentParentStudents == null || currentParentStudents.Count == 0)
                    {
                        var result = await _apiHelper.FetchApiAsync<List<StudentWithAccountResponse>>(ApiEndpointConstant.StudentEndpoint.GetStudentsOfCurrentUser, MethodEnum.GET, null);
                        if (result.Data != null)
                        {
                            currentParentStudents = new List<Student>();
                            foreach (var stu in result.Data)
                            {
                                currentParentStudents.Add(new Student
                                {
                                    Id = stu.Id,
                                    FullName = stu.FullName,
                                    DateOfBirth = stu.DateOfBirth,
                                    Email = stu.Email,
                                    Gender = stu.Gender,
                                    AddedTime = stu.AddedTime,
                                    AvatarImage = stu.AvatarImage,
                                    Parent = new User(),
                                });
                            }
                        }
                        if (currentParentStudents == null)
                        {
                            phoneParentStored.Add(parent.Phone);
                            numberParentLoop++;
                            continue;
                        }
                        else
                        {
                            studentStored.Add(new(parent.Phone, currentParentStudents));
                        }
                    }

                    var currentLStudentLoop = numberStudentLoop.FirstOrDefault(loop => loop.Item1 == parent.Phone);
                    if (!string.IsNullOrEmpty(currentLStudentLoop.Item1))
                    {
                        if (currentLStudentLoop.Item2 == currentParentStudents.Count)
                        {
                            phoneParentStored.Add(parent.Phone);
                            numberParentLoop++;
                            continue;
                        }
                    }

                    Student student;
                    do
                    {
                        student = currentParentStudents[random.Next(0, currentParentStudents.Count)];

                    } while (idStudentStored.Contains(student.Id));

                    idStudentStored.Add(student.Id);
                    if (!string.IsNullOrEmpty(currentLStudentLoop.Item1))
                    {
                        int index = numberStudentLoop.FindIndex(x => x.Item1 == parent.Phone);
                        numberStudentLoop[index] = (parent.Phone, currentLStudentLoop.Item2 + 1);
                    }
                    else
                    {
                        numberStudentLoop.Add(new(parent.Phone, 1));
                    }

                    var checkOutRequest = new List<CheckoutRequest>
                    {
                        new CheckoutRequest
                        {
                            ClassId = Guid.Parse(cls.ClassId),
                            StudentIdList = new List<Guid>
                            {
                               student.Id,
                            },
                        }
                    };

                    var registerInfor = await _apiHelper.FetchApiAsync<BillPaymentResponse>(ApiEndpointConstant.UserEndpoint.CheckoutClass, MethodEnum.POST, checkOutRequest);
                    if (registerInfor.IsSuccess)
                    {
                        numberStudentRegistered++;
                        RegisterInforMessages.Add(new RegisterInforMessage
                        {
                            StudentName = student.FullName!,
                            ParentBelong = parent.FullName!,
                            RegisteredClass = cls.ClassCode,
                            Status = registerInfor.StatusCode,
                            Note = registerInfor.Message,
                        });
                    }
                    else if (registerInfor.Message.Contains("Đã Đủ Chỉ Số"))
                    {
                        RegisterInforMessages.Add(new RegisterInforMessage
                        {
                            Status = "400",
                            Note = "Lớp Đã Đủ Chỉ Số",
                        });

                        break;
                    }
                }
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataRegister", RegisterInforMessages);
            var defaultToken = SessionHelper.GetObjectFromJson<string>(HttpContext.Session, "DeveloperToken");
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", defaultToken);
            IsLoading = true;

            return Page();
        }

        public IActionResult OnPostSearch(string searchKey, string searchType)
        {

            if (string.IsNullOrEmpty(searchKey))
            {
                Classes.Clear();
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                return Page();
            }


            var key = searchKey.Trim().ToLower();
            if (searchType == "MESSAGE")
            {
                var messages = SessionHelper.GetObjectFromJson<List<RegisterInforMessage>>(HttpContext.Session, "DataRegister");

                RegisterInforMessages = messages.Where(
                   mess => mess.StudentName.ToLower().Contains(key) ||
                   mess.ParentBelong.ToLower().Contains(key) ||
                   mess.RegisteredClass.ToLower().Contains(key)
                   ).ToList();
            }
            if (searchType == "DATA")
            {
                var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");

                Classes = classes.Where(
                    c => c.ClassCode.ToLower().Contains(key) ||
                    c.CourseBeLong.ToLower().Contains(key) ||
                    c.LecturerBeLong.ToLower().Contains(key)
                    ).ToList();
            }

            return Page();
        }
    }
}
