using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class PromotionController : BaseController<PromotionController>
    {
        private readonly IPromotionService _promotionService;
        public PromotionController(ILogger<PromotionController> logger, IPromotionService promotionService) : base(logger)
        {
            _promotionService = promotionService;
        }


        #region document API get current user promotion
        /// <summary>
        ///   Get All Promotion Of Current User
        /// </summary>
        /// <response code="200">List of promotion</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.PromotionEnpoint.GetCurrent)]
        [ProducesResponseType(typeof(Promotion), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCurrentUserPromtion()
        {
            var promotion = await _promotionService.GetCurrentUserPromotion();
            return Ok(promotion);
        }
    }
}
