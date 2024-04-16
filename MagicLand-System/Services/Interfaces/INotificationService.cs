﻿using MagicLand_System.Config;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Notifications;

namespace MagicLand_System.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetCurrentUserNotificationsAsync();
        Task<List<NotificationResponse>> GetStaffNotificationsAsync();
        Task<string> UpdateNotificationAsync(List<Guid> ids);
        Task<string> DeleteNotificationAsync(List<Guid> ids);
        Task<PushNotificationResponse> SendNotification(NotificationModel notificationModel);
    }
}
