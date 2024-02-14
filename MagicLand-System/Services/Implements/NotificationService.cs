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
            var notifications = await _unitOfWork.GetRepository<Notification>().GetListAsync(predicate: x => x.UserId == GetUserIdFromJwt(), orderBy: x => x.OrderBy(x => x.CreatedAt));

            return notifications.Select(noti => _mapper.Map<NotificationResponse>(noti)).ToList();
        }

        public async Task<List<NotificationResponse>> GetStaffNotificationsAsync()
        {
            var notifications = await _unitOfWork.GetRepository<Notification>().GetListAsync(predicate: x => x.UserId == null, orderBy: x => x.OrderBy(x => x.CreatedAt));

            return notifications.Select(noti => _mapper.Map<NotificationResponse>(noti)).ToList();
        }

        public async Task<string> UpdateNotificationAsync(Guid id)
        {
            try
            {
                var notification = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(predicate: x => x.Id == id);
                notification.IsRead = true;

                _unitOfWork.GetRepository<Notification>().UpdateAsync(notification);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
            return "Cập Nhập Thành Công";
        }


        public async Task<string> DeleteNotificationAsync(Guid id)
        {
            try
            {
                var notification = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(predicate: x => x.Id == id);

                _unitOfWork.GetRepository<Notification>().DeleteAsync(notification);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
            return "Xóa Thông Báo Thành Công";

        }
    }
}
