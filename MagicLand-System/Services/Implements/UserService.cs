using AutoMapper;
using Azure;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using System;

namespace MagicLand_System.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<LoginResponse> Authentication(LoginRequest loginRequest)
        {
            //var date = DateTime.Now;
            //var parts = loginRequest.Phone.Split("_");
            //if (parts.Length == 1) 
            //{
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: u => u.Phone!.Trim().Equals(loginRequest.Phone.Trim()),
                include: u => u.Include(u => u.Role!));


            if (user == null)
            {
                return default!;
            }

            if (user.Role!.Name == RoleEnum.STUDENT.ToString())
            {
                var isActive = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(
                selector: x => x.IsActive,
                predicate: x => x.Id == user.StudentIdAccount);

                if (!isActive.Value)
                {
                    throw new BadHttpRequestException("Tài Khoản Đã Ngưng Hoạt Động", StatusCodes.Status400BadRequest);
                }
            }

            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
            var loginResponse = new LoginResponse
            {
                Role = user.Role!.Name,
                AccessToken = token,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email != null ? user.Email : string.Empty,
                FullName = user.FullName,
                Gender = user.Gender,
                Phone = user.Phone!,
            };
            return loginResponse;
            #region
            //}
            //if (parts.Length == 2)
            //{
            //    var parentPhone = parts[0];
            //    var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate : u => u.Phone.Trim().Equals(parentPhone));
            //    var students = (await _unitOfWork.GetRepository<Student>().GetListAsync(predicate : x => x.ParentId.ToString().Equals(user.Id.ToString()),include : x => x.Include(x => x.User))).ToArray();
            //    if(students != null && students.Length > 0)
            //    { 
            //       try
            //        {
            //            students = students.OrderBy(x => x.AddedTime).ToArray();
            //            var order = int.Parse(parts[1]);
            //            var student = students[order - 1];
            //            string Role = RoleEnum.STUDENT.ToString();
            //            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", student.Id);
            //            var token = JwtUtil.GenerateJwtToken(null,student, guidClaim);
            //            LoginResponse loginResponse = new LoginResponse
            //            {
            //                Role = Role,
            //                AccessToken = token,
            //                DateOfBirth = student.DateOfBirth,
            //                Email = student.Email,
            //                FullName = student.FullName,
            //                Gender = student.Gender,
            //                Phone = parts[0] ,
            //            };
            //            return loginResponse;
            //        } 
            //        catch(Exception ex) { }
            //        {
            //            return null;
            //        }
            //    }
            //    return null;
            //}
            //return null;
            #endregion
        }

        public async Task<UserExistRespone> CheckUserExistByPhone(string phone)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phone.Trim()), include: x => x.Include(x => x.Role));
            if (user == null)
            {
                #region
                //var parts = phone.Split('_');
                //if (parts.Length == 2)
                //{
                //    var phoneInput = parts[0];
                //    var userFound = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phoneInput.Trim()), include: x => x.Include(x => x.Role).Include(x => x.Students));
                //    try
                //    {
                //        var count = int.Parse(parts[1].Trim());
                //        if (count - 1 > userFound.Students.Count)
                //        {
                //            return new UserExistRespone
                //            {
                //                IsExist = false,
                //            };
                //        }
                //        else
                //        {
                //            return new UserExistRespone
                //            {
                //                IsExist = true,
                //                Role = "student",
                //            };
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        return new UserExistRespone
                //        {
                //            IsExist = false,
                //        };
                //    }
                //}
                #endregion
                return new UserExistRespone
                {
                    IsExist = false,
                };
            }
            return new UserExistRespone
            {
                IsExist = true,
                Role = user.Role.Name,
            };
        }

        public async Task<User> GetCurrentUser()
        {
            var account = await GetUserFromJwt();
            return account;
        }

        public async Task<List<User>> GetUsers()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(predicate: x => x.Id == x.Id);
            return users.ToList();
        }

        public async Task<NewTokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var userId = JwtUtil.ReadToken(refreshTokenRequest.OldToken);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(userId), include: u => u.Include(u => u.Role));
            Tuple<string, Guid> guidClaim = null;

            if (user != null)
            {
                guidClaim = new Tuple<string, Guid>("userId", user.Id);
            }
            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
            return new NewTokenResponse { Token = token };
        }

        public async Task<bool> RegisterNewUser(RegisterRequest registerRequest)
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()), selector: x => x.Id);
            if (registerRequest.DateOfBirth > DateTime.Now)
            {
                throw new BadHttpRequestException("Ngày sinh phải trước ngày hiện tại", StatusCodes.Status400BadRequest);
            }
            User user = new User
            {
                DateOfBirth = registerRequest.DateOfBirth,
                Email = registerRequest.Email,
                FullName = registerRequest.FullName,
                Gender = registerRequest.Gender,
                Phone = registerRequest.Phone,
                RoleId = role,
                Address = registerRequest.Address,
                Id = Guid.NewGuid(),
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            var isUserSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isUserSuccess)
            {
                throw new BadHttpRequestException("Không thể thêm user này", StatusCodes.Status400BadRequest);
            }
            Cart cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
            };
            await _unitOfWork.GetRepository<Cart>().InsertAsync(cart);
            var isCartSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isCartSuccess)
            {
                throw new BadHttpRequestException("Không thể thêm user này", StatusCodes.Status400BadRequest);
            }
            PersonalWallet personalWallet = new PersonalWallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 0
            };
            user.CartId = cart.Id;
            user.PersonalWalletId = personalWallet.Id;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            await _unitOfWork.GetRepository<PersonalWallet>().InsertAsync(personalWallet);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return true; //isSuccess;
        }
        public async Task<List<LecturerResponse>> GetLecturers(FilterLecturerRequest? request)
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role));
            if (users == null)
            {
                return null;
            }
            var lecturers = users.Where(x => x.Role.Name.Trim().Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
            List<LecturerResponse> lecturerResponses = new List<LecturerResponse>();
            foreach (var user in lecturers)
            {
                var lecturerField = await _unitOfWork.GetRepository<LecturerField>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(user.LecturerFieldId.ToString()), selector: x => x.Name);
                var cls = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId.ToString().Equals(user.Id.ToString()));
                var count = 0;
                if (cls != null)
                {
                    count = cls.Count;
                }
                var schedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.SubLecturerId.ToString().Equals(user.Id.ToString()));
                if (schedule != null)
                {
                    var sc = schedule.GroupBy(x => x.ClassId).ToList();
                    count = count + sc.Count;
                }
                LecturerResponse response = new LecturerResponse
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    AvatarImage = user.AvatarImage,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    LectureId = user.Id,
                    Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>(),
                    LecturerField = lecturerField,
                    NumberOfClassesTeaching = count,
                };
                lecturerResponses.Add(response);
            }
            if (lecturerResponses.Count == 0)
            {
                return null;
            }
            if (request != null)
            {
                var type = "all";
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                if (course != null)
                {
                    if (course.Syllabus != null)
                    {
                        type = course.Syllabus.SyllabusCategory.Name;
                    }
                }
                if (type.Equals("all"))
                {
                    lecturerResponses = lecturerResponses;
                }
                else
                {
                    lecturerResponses = lecturerResponses.Where(x => x.LecturerField.Equals(type)).ToList();
                }
                if (request.Schedules != null && request.StartDate != null && request.CourseId != null)
                {
                    List<ScheduleRequest> scheduleRequests = request.Schedules;
                    List<string> daysOfWeek = new List<string>();
                    foreach (ScheduleRequest scheduleRequest in scheduleRequests)
                    {
                        daysOfWeek.Add(scheduleRequest.DateOfWeek);
                    }
                    List<DayOfWeek> convertedDateOfWeek = new List<DayOfWeek>();
                    foreach (var dayOfWeek in daysOfWeek)
                    {
                        if (dayOfWeek.ToLower().Equals("sunday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Sunday);
                        }
                        if (dayOfWeek.ToLower().Equals("monday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Monday);
                        }
                        if (dayOfWeek.ToLower().Equals("tuesday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Tuesday);
                        }
                        if (dayOfWeek.ToLower().Equals("wednesday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Wednesday);
                        }
                        if (dayOfWeek.ToLower().Equals("thursday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Thursday);
                        }
                        if (dayOfWeek.ToLower().Equals("friday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Friday);
                        }
                        if (dayOfWeek.ToLower().Equals("saturday"))
                        {
                            convertedDateOfWeek.Add(DayOfWeek.Saturday);
                        }
                    }
                    var coursex = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.CourseId.ToString()));
                    if (coursex == null)
                    {
                        throw new BadHttpRequestException("không thấy lớp hợp lệ", StatusCodes.Status400BadRequest);
                    }
                    int numberOfSessions = coursex.NumberOfSession;
                    int scheduleAdded = 0;
                    DateTime startDatex = request.StartDate.Value;
                    while (scheduleAdded < numberOfSessions)
                    {
                        if (convertedDateOfWeek.Contains(startDatex.DayOfWeek))
                        {

                            scheduleAdded++;
                        }
                        startDatex = startDatex.AddDays(1);
                    }
                    var endDate = startDatex;
                    List<ScheduleRequest> schedules = request.Schedules;
                    List<ConvertScheduleRequest> convertSchedule = new List<ConvertScheduleRequest>();
                    foreach (var schedule in schedules)
                    {
                        var doW = 1;
                        if (schedule.DateOfWeek.ToLower().Equals("sunday"))
                        {
                            doW = 1;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("monday"))
                        {
                            doW = 2;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("tuesday"))
                        {
                            doW = 4;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("wednesday"))
                        {
                            doW = 8;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("thursday"))
                        {
                            doW = 16;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("friday"))
                        {
                            doW = 32;
                        }
                        if (schedule.DateOfWeek.ToLower().Equals("saturday"))
                        {
                            doW = 64;
                        }
                        convertSchedule.Add(new ConvertScheduleRequest
                        {
                            DateOfWeek = doW,
                            SlotId = schedule.SlotId,
                        });
                    }
                    var allSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync();
                    allSchedule = allSchedule.Where(x => (x.Date < endDate && x.Date >= request.StartDate)).ToList();
                    List<Schedule> result = new List<Schedule>();
                    foreach (var convert in convertSchedule)
                    {
                        var newFilter = allSchedule.Where(x => (x.DayOfWeek == convert.DateOfWeek && x.SlotId.ToString().Equals(convert.SlotId.ToString()))).ToList();
                        if (newFilter != null)
                        {
                            result.AddRange(newFilter);
                        }
                    }
                    List<Guid> classIds = new List<Guid>();
                    List<Guid> subLecturerIds = new List<Guid>();
                    if (result.Count > 0)
                    {
                        var groupByClass = result.GroupBy(x => x.ClassId);
                        classIds = groupByClass.Select(x => x.Key).ToList();
                        var groupBySubLecturer = result.Where(x => (x.SubLecturerId != null)).GroupBy(x => x.SubLecturerId.Value);
                        subLecturerIds = groupBySubLecturer.Select(x => x.Key).ToList();
                    }
                    List<Guid> LecturerIds = new List<Guid>();
                    foreach (var classId in classIds)
                    {
                        LecturerIds.Add(await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()), selector: x => x.LecturerId));
                    }
                    LecturerIds.AddRange(subLecturerIds);
                    List<LecturerResponse> final = new List<LecturerResponse>();
                    foreach (var res in lecturerResponses)
                    {
                        if (!LecturerIds.Contains(res.LectureId))
                        {
                            final.Add(res);
                        }
                    }
                    return final;
                }

            }
            return lecturerResponses;
        }

        public async Task<UserResponse> UpdateUserAsync(UserRequest request)
        {
            try
            {
                var id = GetUserIdFromJwt();
                var currentUser = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id.ToString()), include: x => x.Include(x => x.PersonalWallet));
                if (currentUser == null)
                {
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh Không Thể Xác Thực Người Dùng, Vui Lòng Đăng Nhập Lại Và Thực Hiện Lại Thao Tác", StatusCodes.Status400BadRequest);
                }
                if (request.FullName != null)
                {
                    await UpdateCurrentUserTransaction(request, currentUser);

                    currentUser.FullName = request.FullName!;
                }

                currentUser.DateOfBirth = request.DateOfBirth != default ? request.DateOfBirth : currentUser.DateOfBirth;
                currentUser.Gender = request.Gender ?? currentUser.Gender;
                currentUser.AvatarImage = request.AvatarImage ?? currentUser.AvatarImage;
                currentUser.Email = request.Email ?? currentUser.Email;
                currentUser.Address = request.Address;

                _unitOfWork.GetRepository<User>().UpdateAsync(currentUser);
                await _unitOfWork.CommitAsync();

                return _mapper.Map<UserResponse>(currentUser);

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex}]", StatusCodes.Status400BadRequest);
            }
        }

        private async Task UpdateCurrentUserTransaction(UserRequest request, User currentUser)
        {
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId == currentUser.Id);

            var oldTransactions = await _unitOfWork.GetRepository<WalletTransaction>()
               .GetListAsync(predicate: x => x.PersonalWalletId == personalWallet.Id);

            foreach (var trans in oldTransactions)
            {
                trans.CreateBy = request.FullName;
                trans.UpdateTime = DateTime.Now;
                trans.PersonalWalletId = personalWallet.Id;
                trans.PersonalWallet = personalWallet;
            }

            _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(oldTransactions);
        }

        public async Task<List<LectureScheduleResponse>> GetLectureScheduleAsync()
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId == GetUserIdFromJwt() && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Course!));

            foreach (var cls in classes)
            {
                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == cls.Id,
                include: x => x.Include(x => x.Slot!).Include(x => x.Room!));
            }

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Giáo Viên Không Có Lịch Dạy Hoặc Lớp Học Chưa Bắt Đầu", StatusCodes.Status400BadRequest);
            }

            var responses = new List<LectureScheduleResponse>();
            foreach (var cls in classes)
            {
                responses.AddRange(ScheduleCustomMapper.fromClassToListLectureScheduleResponse(cls));
            }

            return responses;
        }

        public async Task<List<AdminLecturerResponse>> GetAdminLecturerResponses(DateTime? startDate, DateTime? endDate, string? searchString, string? slotId)
        {
            var user = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role).Include(x => x.LecturerField));
            var lecturers = user.Where(x => x.Role.Name.ToLower().Equals("lecturer"));
            List<AdminLecturerResponse> adminLecturerResponses = new List<AdminLecturerResponse>();
            foreach (var lecturer in lecturers)
            {
                List<Schedule> mySchedule = new List<Schedule>();
                var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(include: x => x.Include(x => x.Class).Include(x => x.Room).Include(x => x.Slot));
                var filterSchedules2 = schedules.Where(x => x.Class.LecturerId.ToString().Equals(lecturer.Id.ToString()));
                mySchedule.AddRange(filterSchedules2);
                if (mySchedule.Count > 0)
                {
                    foreach (var schedule in mySchedule)
                    {
                        var adminResponse = new AdminLecturerResponse
                        {
                            Address = lecturer.Address,
                            AvatarImage = lecturer.AvatarImage,
                            ClassCode = schedule.Class.ClassCode,
                            ClassRoom = schedule.Room.Name,
                            Date = schedule.Date,
                            DateOfBirth = lecturer.DateOfBirth,
                            Email = lecturer.Email,
                            StartTime = schedule.Slot.StartTime,
                            EndTime = schedule.Slot.EndTime,
                            FullName = lecturer.FullName,
                            Gender = lecturer.Gender,
                            LecturerField = lecturer.LecturerField.Name,
                            Phone = lecturer.Phone
                        };
                        adminLecturerResponses.Add(adminResponse);

                    }
                }
            }
            if (adminLecturerResponses.Count > 0)
            {
                if (startDate != null)
                {
                    adminLecturerResponses = adminLecturerResponses.Where(x => x.Date >= startDate).ToList();
                }
                if (endDate != null)
                {
                    adminLecturerResponses = adminLecturerResponses.Where(x => x.Date <= endDate.Value.AddHours(23)).ToList();
                }
                if (searchString != null)
                {
                    adminLecturerResponses = adminLecturerResponses.Where(x => (x.LecturerField.ToLower().Trim().Contains(searchString.ToLower().Trim()) || x.FullName.Trim().ToLower().Contains(searchString.ToLower().Trim()) || x.Phone.Trim().ToLower().Contains(searchString.ToLower().Trim()))).ToList();
                }
                if (slotId != null)
                {
                    var startTime = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(slotId), selector: x => x.StartTime);
                    adminLecturerResponses = adminLecturerResponses.Where(x => x.StartTime.Equals(startTime)).ToList();
                }
                adminLecturerResponses = adminLecturerResponses.OrderByDescending(x => x.Date).ThenBy(x => x.FullName).ToList();
            }
            return adminLecturerResponses;

        }

        public async Task<UserResponse> GetUserFromPhone(string phone)
        {
            var checkphone = "+84" + phone.Substring(1);
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Equals(checkphone));
            if (user == null)
            {
                return new UserResponse();
            }
            return new UserResponse
            {
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                AvatarImage = user.AvatarImage,
                DateOfBirth = user.DateOfBirth.Value,
                FullName = user.FullName,
                Gender = user.Gender,
                Id = user.Id,
            };
        }

        public async Task<List<StudentResponse>> GetStudents(string classId, string phone)
        {
            var phone2 = "+84" + phone.Substring(1);
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Equals(phone) || x.Phone.Equals(phone2), include: x => x.Include(x => x.Students));
            if (user == null)
            {
                return new List<StudentResponse>();
            }
            var students = user.Students;
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId.ToString()), include: x => x.Include(x => x.Course).Include(x => x.Schedules));
            var classList = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId == classx.CourseId, selector: x => x.Id);

            var minAge = classx.Course.MinYearOldsStudent.Value;
            var maxAge = classx.Course.MaxYearOldsStudent.Value;
            var startdate = classx.StartDate;
            var enddate = classx.EndDate;
            var year = DateTime.Now.Year;
            List<StudentResponse> st = new List<StudentResponse>();
            foreach (var student in students)
            {
                var exist = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId == student.Id);
                var exist2 = exist.Where(x => classList.Any(p => p == x.ClassId));
                if (exist2.Count() > 0)
                {
                    st.Add(new StudentResponse
                    {
                        Age = year - student.DateOfBirth.Year,
                        AvatarImage = student.AvatarImage,
                        DateOfBirth = student.DateOfBirth,
                        Email = student.Email,
                        FullName = student.FullName,
                        Gender = student.Gender,
                        StudentId = student.Id,
                        CanRegistered = false,
                        ReasonCannotRegistered = $"Học sinh đã đăng ký khóa này trước đó",
                    });
                    continue;
                }
                if ((year - student.DateOfBirth.Year) < minAge || (year - student.DateOfBirth.Year) > maxAge)
                {
                    st.Add(new StudentResponse
                    {
                        Age = year - student.DateOfBirth.Year,
                        AvatarImage = student.AvatarImage,
                        DateOfBirth = student.DateOfBirth,
                        Email = student.Email,
                        FullName = student.FullName,
                        Gender = student.Gender,
                        StudentId = student.Id,
                        CanRegistered = false,
                        ReasonCannotRegistered = $"Độ tuổi của học sinh không thích hợp",
                    });
                    continue;
                }
                var attandances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId == student.Id, include: x => x.Include(x => x.Schedule));
                if (attandances == null)
                {
                    continue;
                }
                var schedules = attandances.Where(x => (x.Schedule.Date >= startdate && x.Schedule.Date.Date <= enddate)).Select(x => x.Schedule);
                var DaysOfWeek = classx.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
                bool flag = true;
                string existDateOfWeek = "";
                foreach (var day in DaysOfWeek)
                {
                    var isExist = schedules.Any(x => x.DayOfWeek == day.DayOfWeek && x.SlotId == day.SlotId);
                    if (isExist)
                    {
                        flag = false;
                        if (day.DayOfWeek == 1)
                        {
                            existDateOfWeek = "sunday";
                        }
                        if (day.DayOfWeek == 2)
                        {
                            existDateOfWeek = "monday";
                        }
                        if (day.DayOfWeek == 4)
                        {
                            existDateOfWeek = "tuesday";
                        }
                        if (day.DayOfWeek == 8)
                        {
                            existDateOfWeek = "wednesday";
                        }
                        if (day.DayOfWeek == 16)
                        {
                            existDateOfWeek = "thursday";
                        }
                        if (day.DayOfWeek == 32)
                        {
                            existDateOfWeek = "friday";
                        }
                        if (day.DayOfWeek == 64)
                        {
                            existDateOfWeek = "saturday";
                        }
                        break;
                    }
                }
                if (flag)
                {
                    st.Add(new StudentResponse
                    {
                        Age = year - student.DateOfBirth.Year,
                        AvatarImage = student.AvatarImage,
                        DateOfBirth = student.DateOfBirth,
                        Email = student.Email,
                        FullName = student.FullName,
                        Gender = student.Gender,
                        StudentId = student.Id,
                        CanRegistered = true,
                    });
                }
                else
                {
                    st.Add(new StudentResponse
                    {
                        Age = year - student.DateOfBirth.Year,
                        AvatarImage = student.AvatarImage,
                        DateOfBirth = student.DateOfBirth,
                        Email = student.Email,
                        FullName = student.FullName,
                        Gender = student.Gender,
                        StudentId = student.Id,
                        CanRegistered = false,
                        ReasonCannotRegistered = $"Học sinh tồn tại lịch vào {existDateOfWeek} trước đó",
                    });
                }
            }
            return st;
        }

        public async Task<List<UserResponse>> GetUserFromName(string name)
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(predicate: x => x.FullName.ToLower().Trim().Contains(name.ToLower().Trim()), include: x => x.Include(x => x.Role));
            var responses = new List<UserResponse>();
            foreach (var user in users)
            {
                if (user.Role.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()))
                {
                    var response = new UserResponse
                    {
                        Email = user.Email,
                        Phone = user.Phone,
                        Address = user.Address,
                        AvatarImage = user.AvatarImage,
                        DateOfBirth = user.DateOfBirth.Value,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        Id = user.Id,
                    };
                    responses.Add(response);
                }
            }
            return responses;
        }

        public async Task<List<StudentResultResponse>> GetFromNameAndBirthDate(string? name, DateTime? birthdate, string? id)
        {
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync(include: x => x.Include(x => x.User));
            if (name != null)
            {
                students = students.Where(x => x.FullName.ToLower().Trim().Contains(name.ToLower().Trim())).ToList();
            }
            if (birthdate != null)
            {
                students = students.Where(x => x.DateOfBirth.Date == birthdate.Value.Date).ToList();
            }
            if (id != null)
            {
                students = students.Where(x => x.Id.ToString().Equals(id)).ToList();
            }
            List<StudentResultResponse> responses = new List<StudentResultResponse>();
            foreach (var student in students)
            {
                var res = new StudentResultResponse
                {
                    StudentResponse = new StudentResponse
                    {
                        Age = (DateTime.Now.Year - student.DateOfBirth.Year),
                        AvatarImage = student.AvatarImage,
                        DateOfBirth = student.DateOfBirth,
                        FullName = student.FullName,
                        Email = student.Email,
                        Gender = student.Gender,
                        StudentId = student.Id,
                    },
                    Parent = new UserResponse
                    {
                        FullName = student.User.FullName,
                        Gender = student.User.Gender,
                        Email = student.User.Email,
                        DateOfBirth = student.User.DateOfBirth.Value,
                        Address = student.User.Address,
                        AvatarImage = student.User.AvatarImage,
                        Id = student.User.Id,
                        Phone = student.User.Phone
                    }
                };
                responses.Add(res);
            }
            return responses;
        }

        public async Task<ClassResultResponse> GetClassOfStudent(string studentId, string? status, string? searchString, DateTime? date)
        {
            var classxx = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), selector: x => x.ClassId);
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => classxx.Any(p => p == x.Id), include: x => x.Include(x => x.Schedules));
            if (classes.Count < 1)
            {
                return new ClassResultResponse();
            }
            List<MyClassResponse> result = new List<MyClassResponse>();
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            foreach (var c in classes)
            {
                var schedulex = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == c.Id)).FirstOrDefault();
                if (schedulex == null) { continue; }
                var room = (await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == schedulex.RoomId));
                if (room == null) { continue; }
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.LecturerId.ToString()));
                if (lecturer == null) { continue; }
                RoomResponse roomResponse = new RoomResponse
                {
                    Floor = room.Floor.Value,
                    Capacity = room.Capacity,
                    RoomId = room.Id,
                    Name = room.Name,
                    Status = room.Status,
                    LinkUrl = room.LinkURL,

                };
                LecturerResponse lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                };
                List<DailySchedule> schedules = new List<DailySchedule>();
                var DaysOfWeek = c.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
                foreach (var day in DaysOfWeek)
                {
                    var slot = slots.Where(x => x.Id.ToString().ToLower().Equals(day.SlotId.ToString().ToLower())).FirstOrDefault();
                    if (day.DayOfWeek == 1)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Sunday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 2)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Monday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 4)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Tuesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 8)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Wednesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 16)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Thursday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 32)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Friday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 64)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Saturday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                }
                Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                var studentList = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id);
                MyClassResponse myClassResponse = new MyClassResponse
                {
                    ClassId = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,
                    ClassCode = c.ClassCode,
                    LecturerName = lecturer.FullName,
                    CoursePrice = await GetDynamicPrice(course.Id, false),
                    EndDate = c.EndDate,
                    CourseId = c.CourseId,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = studentList.Count,
                    Schedules = schedules,
                    CourseName = course.Name,
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,
                    CreatedDate = c.AddedDate.Value,
                };
                var syllabusCode = "undefined";
                var syllabusName = "undefined";
                var syllabusType = "undefined";
                if (course.Syllabus != null)
                {
                    syllabusCode = course.Syllabus.SubjectCode;
                    syllabusName = course.Syllabus.Name;
                    syllabusType = course.Syllabus.SyllabusCategory.Name;
                }
                CustomCourseResponse customCourseResponse = new CustomCourseResponse
                {
                    Image = course.Image,
                    MainDescription = course.MainDescription,
                    MaxYearOldsStudent = course.MaxYearOldsStudent,
                    MinYearOldsStudent = course.MinYearOldsStudent,
                    Name = course.Name,
                    Price = await GetDynamicPrice(course.Id, false),
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = course.Status
                };
                myClassResponse.CourseResponse = customCourseResponse;
                myClassResponse.ClassScheduleResponses = await GetStudentSessionAsync(myClassResponse.ClassId.ToString(), studentId, date);
                var canChange = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.ClassId == myClassResponse.ClassId && x.StudentId.ToString().Equals(studentId));
                myClassResponse.CanChangeClass = canChange.CanChangeClass;
                result.Add(myClassResponse);
            }
            if (date != null)
            {
                var datex = date.Value.DayOfWeek.ToString();
                result = result.Where(x => (x.StartDate.Date <= date.Value.Date && x.EndDate >= date.Value.Date)).ToList();
                result = result.Where(x => x.Schedules.Select(x => x.DayOfWeek.ToLower()).Any(p => p.Equals(date.Value.DayOfWeek.ToString().ToLower()))).ToList();
            }
            var numberOfClasses = 0;
            if (status == null)
            {
                numberOfClasses = result.Count;
            }
            else
            {
                numberOfClasses = result.Where(x => x.Status.ToLower().Equals(status.ToLower())).Count();
            }
            //
            if (result.Count == 0)
            {
                return null;
            }
            if (result != null)
            {
                result = result.OrderByDescending(x => x.CreatedDate).ToList();
            }
            if (searchString == null && status == null)
            {
                return new ClassResultResponse
                {
                    MyClassResponses = result.OrderByDescending(x => x.CreatedDate).ToList(),
                    NumberOfClasses = result.Count,
                };
            }
            if (searchString == null)
            {
                return new ClassResultResponse
                {
                    MyClassResponses = (result.Where(x => x.Status.ToLower().Equals(status.ToLower())).OrderByDescending(x => x.CreatedDate).ToList()),
                    NumberOfClasses = (result.Where(x => x.Status.ToLower().Equals(status.ToLower())).OrderByDescending(x => x.CreatedDate).ToList()).Count,
                };
            }
            if (status == null)
            {
                List<MyClassResponse> res = new List<MyClassResponse>();
                var filter1 = result.Where(x => x.ClassCode.ToLower().Contains(searchString.ToLower()));
                if (filter1 != null)
                {
                    res.AddRange(filter1);
                }
                var filter2 = result.Where(x => x.CourseName.ToLower().Contains((searchString.ToLower())));
                if (filter2 != null)
                {
                    res.AddRange(filter2);
                }
                return new ClassResultResponse
                {
                    MyClassResponses = res.OrderByDescending(x => x.CreatedDate).ToList(),
                    NumberOfClasses = res.Count,
                };
            }
            return new ClassResultResponse
            {
                MyClassResponses = (result.Where(x => ((x.ClassCode.ToLower().Contains(searchString.ToLower()) || x.CourseName.ToLower().Contains(searchString.ToLower())) && x.Status.ToLower().Equals(status.ToLower())))).OrderByDescending(x => x.CreatedDate).ToList(),
                NumberOfClasses = (result.Where(x => ((x.ClassCode.ToLower().Contains(searchString.ToLower()) || x.CourseName.ToLower().Contains(searchString.ToLower())) && x.Status.ToLower().Equals(status.ToLower())))).Count(),
            };

        }

        public async Task<List<StudentScheduleResponse>> GetScheduleOfStudentInDate(string studentId, DateTime date)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId.ToString().Equals(studentId)),
                include: x => x.Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Slot)
                .Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Room!));

            if (!classes.Any())
            {
                return new List<StudentScheduleResponse>();
            }
            var listStudentSchedule = new List<StudentScheduleResponse>();
            foreach (var cls in classes)
            {
                var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(cls.LecturerId));

                var subject = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                       selector: x => x.SubjectName,
                       predicate: x => x.Id == cls.CourseId);

                var topics = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    selector: x => x.Topics,
                    predicate: x => x.CourseId == cls.CourseId,
                    include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                var identifySession = new List<(int, Guid, Guid)>();

                int sessionIndex = 0;
                foreach (var topic in topics!)
                {
                    foreach (var session in topic.Sessions!)
                    {
                        sessionIndex++;
                        identifySession.Add(new(sessionIndex, topic.Id, session.Id));
                    }
                }

                for (int i = 0; i < cls.Schedules.Count(); i++)
                {
                    var schedule = cls.Schedules.ToList()[i];
                    var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == schedule.Id && x.StudentId.ToString().ToLower() == studentId);
                    var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == schedule.Id && x.StudentId.ToString().ToLower() == studentId);
                    var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == cls.CourseId);
                    var studentSchedule = new StudentScheduleResponse
                    {
                        Address = cls.Street + " " + cls.District + " " + cls.City,
                        ClassName = cls.ClassCode!,
                        ClassId = cls.Id,
                        CourseId = cls.CourseId,
                        ClassSubject = subject!,
                        StudentName = student.FullName!,
                        Date = schedule.Date,
                        ClassCode = cls.ClassCode!,
                        TopicId = identifySession.Find(x => x.Item1 == i + 1).Item2,
                        SessionId = schedule.Id,
                        DayOfWeek = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                        EndTime = schedule.Slot!.EndTime,
                        StartTime = schedule.Slot.StartTime,
                        LinkURL = schedule.Room!.LinkURL,
                        Method = cls.Method,
                        RoomInFloor = schedule.Room.Floor,
                        RoomName = schedule.Room.Name,
                        AttendanceStatus = attendance != null ? attendance.IsPresent == true ? "Có Mặt" : "Vắng Mặt" : "Chưa Điểm Danh",
                        Note = attendance != null ? attendance.Note : null,
                        LecturerName = lecturerName,
                        EvaluateLevel = evaluate.Status == EvaluateStatusEnum.NORMAL.ToString() ? 2 : evaluate.Status == EvaluateStatusEnum.NOTGOOD.ToString() ? 1 : 3,
                        EvaluateDescription = evaluate.Status == EvaluateStatusEnum.NORMAL.ToString() ? "Bình Thường"
                        : evaluate.Status == EvaluateStatusEnum.NOTGOOD.ToString() ? "Không Tốt" : "Tốt",
                        EvaluateNote = evaluate.Note,
                        CourseName = course.Name,
                        SessionIdInDate = identifySession.Find(x => x.Item1 == i + 1).Item3,
                    };

                    listStudentSchedule.Add(studentSchedule);
                }
            }
            listStudentSchedule = listStudentSchedule.Where(x => x.Date.Date == date.Date).ToList();
            return listStudentSchedule;
        }

        public async Task<StudentSessionResponse> GetStudentSession(string scheduleId)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(scheduleId), include: x => x.Include(x => x.Slot));
            var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == schedule.ClassId);
            var studentId = (students.FirstOrDefault()).StudentId.ToString();
            var res = await GetScheduleOfStudentInDate(studentId, schedule.Date.Date);
            var response = res.Single(x => x.SessionId.ToString().Equals(scheduleId));
            var session = await _unitOfWork.GetRepository<Session>().SingleOrDefaultAsync(predicate: x => x.Id == response.SessionIdInDate, include: x => x.Include(x => x.SessionDescriptions).Include(x => x.Topic));
            List<SessionContentReponse> sessionContentReponses = new List<SessionContentReponse>();
            foreach (var ss in session.SessionDescriptions)
            {
                var content = ss.Content;
                var description = ss.Detail.Split("/r/n").ToList();
                sessionContentReponses.Add(new SessionContentReponse
                {
                    Content = content,
                    Details = description,
                });
            }
            var result = new StudentSessionResponse
            {
                CourseName = response.CourseName,
                ClassCode = response.ClassCode,
                Contents = sessionContentReponses,
                TopicName = session.Topic.Name,
                Index = session.NoSession,
                Date = schedule.Date.Date,
                StartTime = schedule.Slot.StartTime,
                EndTime = schedule.Slot.EndTime,
            };
            return result;
        }

        public async Task<List<ClassScheduleResponse>> GetStudentSessionAsync(string classId, string studentId, DateTime? date)
        {
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId), include: x => x.Include(x => x.Schedules).Include(x => x.Course));
            var schedules = classx.Schedules.OrderBy(x => x.Date).ToArray();
            var attendancesArray = new List<Attendance>();
            for (int i = 0; i < schedules.Length; i++)
            {
                var att = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == schedules[i].Id && x.StudentId.ToString().Equals(studentId));
                if (att != null)
                {
                    attendancesArray.Add(att);
                }
            }
            //var attendancesArray = attendances.OrderBy(x => x.Schedule.Date).ToArray();
            var syllabusId = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.CourseId == classx.CourseId, selector: x => x.Id);
            var topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(predicate: x => x.SyllabusId == syllabusId);
            var sessions = new List<Session>();
            foreach (var topic in topics)
            {
                var sessionx = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId == topic.Id, include: x => x.Include(x => x.SessionDescriptions).Include(x => x.Topic));
                sessions.AddRange(sessionx);
            }
            var sessionArray = sessions.OrderBy(x => x.NoSession).ToArray();
            List<ClassScheduleResponse> classScheduleResponses = new List<ClassScheduleResponse>();
            for (int i = 0; i < schedules.Length; i++)
            {
                var status = "";
                if (attendancesArray[i].IsPresent != null)
                {
                    if (attendancesArray[i].IsPresent.Value)
                    {
                        status = "Có mặt";
                    }
                    if (!attendancesArray[i].IsPresent.Value)
                    {
                        status = "Vắng";
                    }
                }
                if (attendancesArray[i].IsPresent == null)
                {
                    status = "Chưa diễn ra";
                }
                List<SessionContentReponse> sessionContentReponses = new List<SessionContentReponse>();
                foreach (var ss in sessionArray[i].SessionDescriptions)
                {
                    var content = ss.Content;
                    var description = ss.Detail.Split("/r/n").ToList();
                    sessionContentReponses.Add(new SessionContentReponse
                    {
                        Content = content,
                        Details = description,
                    });
                }
                var classSched = new ClassScheduleResponse
                {
                    Date = schedules[i].Date,
                    Index = i + 1,
                    Status = status,
                    TopicContent = new TopicContent
                    {
                        TopicName = sessionArray[i].Topic.Name,
                        TopicIndex = sessionArray[i].Topic.OrderNumber,
                        Contents = sessionContentReponses,
                    }
                };
                classScheduleResponses.Add(classSched);
            }
            if (date != null)
            {
                classScheduleResponses = classScheduleResponses.Where(r => r.Date.Date == date.Value.Date).ToList();
            }
            return classScheduleResponses;
        }
    }
}
