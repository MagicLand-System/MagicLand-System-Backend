using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class MaterialCustomMapper
    {
        public static List<MaterialResponse> fromMaterialsToMaterialResponse(ICollection<Material> materials)
        {
            if (materials == null)
            {
                return default!;
            }

            var responses = materials.Select(mat => new MaterialResponse
            {
                MaterialId = mat.Id,
                Url = mat.URL,
            }).ToList();


            return responses;
        }
    }
}
