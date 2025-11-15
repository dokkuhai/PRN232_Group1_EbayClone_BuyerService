using EbayCloneBuyerService_CoreAPI.DTOs.Feedback;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EbayCloneBuyerService_CoreAPI.Controllers;

/// <summary>
/// FeedbackController - Buyer Service Only
/// Buyer leaves feedback after purchase & views other buyers' feedback about sellers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(
        IFeedbackService feedbackService,
        ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    // ============================================================
    // PUBLIC ENDPOINTS - View feedback about sellers (for buyer research)
    // ============================================================

    /// <summary>
    /// Get feedback statistics about a seller (PUBLIC - for buyer to research before buying)
    /// GET: api/feedback/about-seller/5
    /// </summary>
    [HttpGet("about-seller/{sellerId}")]
    [ProducesResponseType(typeof(FeedbackStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackStatsDto>> GetFeedbackAboutSeller(int sellerId)
    {
        try
        {
            // Buyer xem feedback của những buyers khác đã để về seller này
            var stats = await _feedbackService.GetSellerFeedbackStatsAsync(sellerId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedback about seller {SellerId}", sellerId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all feedback about a seller - paginated (PUBLIC - for buyer research)
    /// GET: api/feedback/about-seller/5/list?page=1&pageSize=20
    /// </summary>
    [HttpGet("about-seller/{sellerId}/list")]
    [ProducesResponseType(typeof(List<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FeedbackDto>>> GetFeedbackListAboutSeller(
        int sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // Buyer xem list feedbacks của những buyers khác về seller
            var feedbacks = await _feedbackService.GetSellerFeedbacksAsync(sellerId, page, pageSize);

            return Ok(new
            {
                page,
                pageSize,
                totalItems = feedbacks.Count,
                data = feedbacks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedback list about seller {SellerId}", sellerId);
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================================
    // BUYER ENDPOINTS - Manage MY OWN feedback (as a buyer)
    // ============================================================

    /// <summary>
    /// Leave feedback after purchase (BUYER ONLY)
    /// POST: api/feedback
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FeedbackDto>> LeaveFeedback([FromBody] CreateFeedbackDto dto)
    {
        try
        {
            var buyerId = GetCurrentUserId();
            if (buyerId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            var feedback = await _feedbackService.LeaveFeedbackAsync(buyerId, dto);

            _logger.LogInformation("Buyer {BuyerId} left feedback about seller {SellerId}, order {OrderId}",
                buyerId, dto.SellerId, dto.OrderId);

            return CreatedAtAction(
                nameof(GetFeedbackById),
                new { id = feedback.Id },
                feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving feedback");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get MY feedbacks (feedbacks I left as a buyer)
    /// GET: api/feedback/my-feedbacks
    /// </summary>
    [Authorize]
    [HttpGet("my-feedbacks")]
    [ProducesResponseType(typeof(List<FeedbackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<FeedbackDto>>> GetMyFeedbacks()
    {
        try
        {
            var buyerId = GetCurrentUserId();
            if (buyerId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            // Get feedbacks tôi đã để về các sellers
            var feedbacks = await _feedbackService.GetBuyerFeedbacksAsync(buyerId);

            return Ok(new
            {
                totalFeedbacks = feedbacks.Count,
                data = feedbacks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my feedbacks");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if I can leave feedback for an order
    /// GET: api/feedback/order/123/can-leave
    /// </summary>
    [Authorize]
    [HttpGet("order/{orderId}/can-leave")]
    [ProducesResponseType(typeof(CanLeaveFeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CanLeaveFeedbackDto>> CanLeaveFeedback(int orderId)
    {
        try
        {
            var buyerId = GetCurrentUserId();
            if (buyerId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _feedbackService.CanLeaveFeedbackAsync(buyerId, orderId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking can leave feedback for order {OrderId}", orderId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get feedback by ID (PUBLIC)
    /// GET: api/feedback/123
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackDto>> GetFeedbackById(int id)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return NotFound(new { message = "Feedback not found" });

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedback by ID {FeedbackId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update MY existing feedback (within 60 days)
    /// PUT: api/feedback/123
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeedbackDto>> UpdateMyFeedback(int id, [FromBody] UpdateFeedbackDto dto)
    {
        try
        {
            var buyerId = GetCurrentUserId();
            if (buyerId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            var feedback = await _feedbackService.UpdateFeedbackAsync(buyerId, id, dto);

            _logger.LogInformation("Buyer {BuyerId} updated their feedback {FeedbackId}", buyerId, id);

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feedback {FeedbackId}", id);

            if (ex.Message.Contains("only update your own"))
                return Forbid();

            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete MY feedback (within 60 days)
    /// DELETE: api/feedback/123
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyFeedback(int id)
    {
        try
        {
            var buyerId = GetCurrentUserId();
            if (buyerId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _feedbackService.DeleteFeedbackAsync(buyerId, id);

            if (!result)
                return NotFound(new { message = "Feedback not found" });

            _logger.LogInformation("Buyer {BuyerId} deleted their feedback {FeedbackId}", buyerId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feedback {FeedbackId}", id);

            if (ex.Message.Contains("only delete your own"))
                return Forbid();

            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Get current user ID from JWT token
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return 0;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}