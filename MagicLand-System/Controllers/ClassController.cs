using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class ClassController : BaseController<ClassController>
    {
        public ClassController(ILogger<ClassController> logger) : base(logger)
        {
        }
    }
}
