using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class MaterialCustomMapper
    {
        public static MaterialResponse fromMaterialsToMaterialResponse(ICollection<Material> materials)
        {
            if (materials == null)
            {
                return default!;
            }

            var response = new MaterialResponse()
            {
                MaterialInfor = materials.Select(x => new MaterialInforResponse
                {
                    MaterialId = x.Id,
                    Url = x.URL,

                }).ToList(),
            };

            return response;
        }
    }
}
