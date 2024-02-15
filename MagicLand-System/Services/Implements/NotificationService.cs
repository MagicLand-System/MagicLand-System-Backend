using AutoMapper;
using CorePush.Google;
using MagicLand_System.Config;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Notifications;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using static MagicLand_System.Config.GoogleNotification;

namespace MagicLand_System.Services.Implements
{
    public class NotificationService : BaseService<NotificationService>, INotificationService
    {
        private readonly FcmNotificationSetting _fcmNotificationSetting;
        public NotificationService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<NotificationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IOptions<FcmNotificationSetting> settings) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _fcmNotificationSetting = settings.Value;
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

        public async Task<PushNotificationResponse> SendNotification(NotificationModel notificationModel)
        {
            PushNotificationResponse response = new PushNotificationResponse();
            try
            {
                if (notificationModel.IsAndroiodDevice)
                {
                    /* FCM Sender (Android Device) */
                    FcmSettings settings = new FcmSettings()
                    {
                        SenderId = _fcmNotificationSetting.SenderId,
                        ServerKey = _fcmNotificationSetting.ServerKey
                    };
                    HttpClient httpClient = new HttpClient();

                    string authorizationKey = string.Format("keyy={0}", settings.ServerKey);
                    string deviceToken = notificationModel.DeviceId;

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                    httpClient.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    DataPayload dataPayload = new DataPayload();
                    dataPayload.Title = notificationModel.Title;
                    dataPayload.Body = notificationModel.Body;

                    GoogleNotification notification = new GoogleNotification();
                    notification.Data = dataPayload;
                    notification.Notification = dataPayload;

                    var fcm = new FcmSender(settings, httpClient);
                    var fcmSendResponse = await fcm.SendAsync(deviceToken, notification);

                    if (fcmSendResponse.IsSuccess())
                    {
                        response.IsSuccess = true;
                        response.Message = "Notification sent successfully";
                        return response;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = fcmSendResponse.Results[0].Error;
                        return response;
                    }
                }
                else
                {
                    /* Code here for APN Sender (iOS Device) */
                    //var apn = new ApnSender(apnSettings, httpClient);
                    //await apn.SendAsync(notification, deviceToken);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong";
                return response;
            }
        }
    }
}

