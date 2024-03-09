using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
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
            var date = DateTime.Now;
            var parts = loginRequest.Phone.Split("_");
            if (parts.Length == 1) 
            {
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: u => u.Phone.Trim().Equals(loginRequest.Phone.Trim()), include: u => u.Include(u => u.Role));
                string Role = user.Role.Name;
                Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
                var token = JwtUtil.GenerateJwtToken(user,null, guidClaim);
                LoginResponse loginResponse = new LoginResponse
                {
                    Role = Role,
                    AccessToken = token,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    FullName = user.FullName,
                    Gender = user.Gender,
                    Phone = user.Phone,
                };
                return loginResponse;

            }
            if (parts.Length == 2)
            {
                var parentPhone = parts[0];
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate : u => u.Phone.Trim().Equals(parentPhone));
                var students = (await _unitOfWork.GetRepository<Student>().GetListAsync(predicate : x => x.ParentId.ToString().Equals(user.Id.ToString()),include : x => x.Include(x => x.User))).ToArray();
                if(students != null && students.Length > 0)
                { 
                   try
                    {
                        students = students.OrderBy(x => x.AddedTime).ToArray();
                        var order = int.Parse(parts[1]);
                        var student = students[order - 1];
                        string Role = RoleEnum.STUDENT.ToString();
                        Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", student.Id);
                        var token = JwtUtil.GenerateJwtToken(null,student, guidClaim);
                        LoginResponse loginResponse = new LoginResponse
                        {
                            Role = Role,
                            AccessToken = token,
                            DateOfBirth = student.DateOfBirth,
                            Email = student.Email,
                            FullName = student.FullName,
                            Gender = student.Gender,
                            Phone = parts[0] ,
                        };
                        return loginResponse;
                    } 
                    catch(Exception ex) { }
                    {
                        return null;
                    }
                }
                return null;
            }
            return null;
        }

        public async Task<UserExistRespone> CheckUserExistByPhone(string phone)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phone.Trim()), include: x => x.Include(x => x.Role));
            if (user == null)
            {
                var parts = phone.Split('_');
                if (parts.Length == 2) 
                {
                    var phoneInput = parts[0];
                    var userFound = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phoneInput.Trim()), include: x => x.Include(x => x.Role).Include(x => x.Students));
                    try
                    {
                        var count = int.Parse(parts[1].Trim());
                        if (count - 1  > userFound.Students.Count)
                        {
                            return new UserExistRespone
                            {
                                IsExist = false,
                            };
                        } else
                        {
                            return new UserExistRespone
                            {
                                IsExist = true,
                                Role = "student",
                            };
                        }

                    }
                    catch (Exception ex)
                    {
                        return new UserExistRespone
                        {
                            IsExist = false,
                        };
                    }
                }
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

        public async Task<(User?,Student?)> GetCurrentUser()
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
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate : x => x.Id == Guid.Parse(userId),include : x => x.Include(x => x.User));
            Tuple<string, Guid> guidClaim = null;
            if (student != null)
            {
                guidClaim = new Tuple<string, Guid>("userId", student.Id);
            }
            if(user != null)
            {
                guidClaim = new Tuple<string,Guid>("userId", user.Id);    
            }
            var token = JwtUtil.GenerateJwtToken(user,student, guidClaim);
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
            var lecturers = users.Where(x => x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
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
                    return lecturerResponses;
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
                var id = (await GetUserFromJwt()).Item1.Id;
                var currentUser = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id.ToString()), include: x => x.Include(x => x.PersonalWallet));
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
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!)
               .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room!)
               .Include(x => x.Course!));

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Giáo Viên Không Có Lịch Dạy Hoặc Lớp Học Chưa Bắt Đầu", StatusCodes.Status400BadRequest);
            }

            var responses = new List<LectureScheduleResponse>();
            foreach(var cls in classes)
            {
                responses.AddRange(ScheduleCustomMapper.fromClassToListLectureScheduleResponse(cls));
            }

            return responses;
        }
    }
}
