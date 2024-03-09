using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Mappers.Custom
{
    public class UserCustomMapper
    {
        public static UserResponse fromUserToUserResponse(MagicLand_System.Domain.Models.User user)
        {
            if (user == null)
            {
                return default!;
            }

            var response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone!,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth!.Value,
                AvatarImage = string.IsNullOrEmpty(user.AvatarImage) ? ImageUrlConstant.DefaultAvatar() : user.AvatarImage,
                Address = user.Address,
            };

            return response;
        }
    }
}
