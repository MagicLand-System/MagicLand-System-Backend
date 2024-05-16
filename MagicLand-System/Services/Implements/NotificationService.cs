using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Notifications;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class NotificationService : BaseService<NotificationService>, INotificationService
    {
        public NotificationService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<NotificationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<NotificationResponse>> GetCurrentUserNotificationsAsync()
        {
            var notifications = new List<Notification>();
            if (GetRoleFromJwt() == RoleEnum.STAFF.ToString())
            {
                notifications = (await _unitOfWork.GetRepository<Notification>().GetListAsync(predicate: x => x.UserId == null || x.UserId == default, orderBy: x => x.OrderBy(x => x.CreatedAt))).ToList();
            }
            else
            {
                notifications = (await _unitOfWork.GetRepository<Notification>().GetListAsync(predicate: x => x.UserId == GetUserIdFromJwt(), orderBy: x => x.OrderBy(x => x.CreatedAt))).ToList();
            }

            return notifications.Select(noti => _mapper.Map<NotificationResponse>(noti)).ToList();
        }

        public async Task<List<NotificationResponse>> GetStaffNotificationsAsync()
        {
            var notifications = await _unitOfWork.GetRepository<Notification>().GetListAsync(predicate: x => x.UserId == null, orderBy: x => x.OrderBy(x => x.CreatedAt));

            return notifications.Select(noti => _mapper.Map<NotificationResponse>(noti)).ToList();
        }

        public async Task<string> UpdateNotificationAsync(List<Guid> ids)
        {
            try
            {
                var notifications = new List<Notification>();
                foreach (var id in ids)
                {
                    var notification = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(predicate: x => x.Id == id);
                    if (notification == null)
                    {
                        throw new BadHttpRequestException($"Id [{id}]Của Thông Báo Không Tồn Tại", StatusCodes.Status400BadRequest);
                    }

                    notifications.Add(notification);
                }

                notifications.ForEach(noti => noti.IsRead = true);
                _unitOfWork.GetRepository<Notification>().UpdateRange(notifications);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status500InternalServerError);
            }
            return "Cập Nhập Thành Công";
        }


        public async Task<string> DeleteNotificationAsync(List<Guid> ids)
        {
            try
            {
                var notifications = new List<Notification>();
                foreach (var id in ids)
                {
                    var notification = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(predicate: x => x.Id == id);
                    if (notification == null)
                    {
                        throw new BadHttpRequestException($"Id [{id}]Của Thông Báo Không Tồn Tại", StatusCodes.Status400BadRequest);
                    }

                    notifications.Add(notification);
                }


                _unitOfWork.GetRepository<Notification>().DeleteRangeAsync(notifications);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status500InternalServerError);
            }
            return "Xóa Thông Báo Thành Công";

        }


    }
}

