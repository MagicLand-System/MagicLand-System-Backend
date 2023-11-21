using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    public interface IClassController
    {
        Task<IActionResult> FilterClass([FromQuery] List<string>? keyWords, [FromQuery] double? minPrice, [FromQuery] double? maxPrice, [FromQuery] int? limitStudent);
        Task<IActionResult> GetClassByCourseId(Guid id);
        Task<IActionResult> GetClassById(Guid id);
        Task<IActionResult> GetClasses();
    }
}