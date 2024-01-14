using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.Mappers.Custom
{
    public class CourseDescriptionCustomMapper
    {

        public static SubDescriptionTitleResponse fromSubDesTileToSubDesTitleResponse(SubDescriptionTitle subDescriptionTitle)
        {
            if (subDescriptionTitle == null)
            {
                return new SubDescriptionTitleResponse();
            }

            SubDescriptionTitleResponse response = new SubDescriptionTitleResponse
            {
                Title = subDescriptionTitle.Title,
                Contents = subDescriptionTitle.SubDescriptionContents
                .Select(sdc => fromSubDescriptionContentToSubDescriptionContentResponse(sdc)).ToList(),
            };

            return response;
        }
        public static SubDescriptionContentResponse fromSubDescriptionContentToSubDescriptionContentResponse(SubDescriptionContent subDescriptionContent)
        {
            if (subDescriptionContent == null)
            {
                return new SubDescriptionContentResponse();
            }

            SubDescriptionContentResponse response = new SubDescriptionContentResponse
            {
                Content = subDescriptionContent.Content,
                Description = subDescriptionContent.Description,
            };

            return response;
        }
    }
}
