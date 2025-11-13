using EbayCloneBuyerService_CoreAPI.DTOs.Review;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// [GET] /api/reviews/{id} - Lấy 1 review theo ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            // TODO: Get currentUserId from JWT token
            var currentUserId = GetCurrentUserId();

            var review = await _reviewService.GetByIdAsync(id, currentUserId);
            if (review == null)
            {
                return NotFound(new { message = "Review not found" });
            }

            return Ok(review);
        }

        /// <summary>
        /// [GET] /api/reviews - Lấy danh sách reviews với filter, sort, pagination (OData-like)
        /// Query params: productId, reviewerId, rating, minRating, maxRating, searchKeyword, sortBy, page, pageSize
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetAll([FromQuery] ReviewQueryParams queryParams)
        {
            var currentUserId = GetCurrentUserId();

            var (reviews, totalCount, totalPages) = await _reviewService.GetAllAsync(queryParams, currentUserId);

            return Ok(new
            {
                data = reviews,
                pagination = new
                {
                    page = queryParams.Page,
                    pageSize = queryParams.PageSize,
                    totalCount,
                    totalPages
                }
            });
        }

        /// <summary>
        /// [GET] /api/reviews/product/{productId} - Lấy tất cả reviews của 1 sản phẩm
        /// </summary>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), 200)]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var currentUserId = GetCurrentUserId();
            var reviews = await _reviewService.GetByProductIdAsync(productId, currentUserId);
            return Ok(reviews);
        }

        /// <summary>
        /// [GET] /api/reviews/user/{userId} - Lấy tất cả reviews của 1 user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), 200)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var currentUserId = GetCurrentUserId();
            var reviews = await _reviewService.GetByUserIdAsync(userId, currentUserId);
            return Ok(reviews);
        }

        /// <summary>
        /// [GET] /api/reviews/product/{productId}/stats - Lấy statistics của sản phẩm
        /// </summary>
        [HttpGet("product/{productId}/stats")]
        [ProducesResponseType(typeof(ProductReviewStats), 200)]
        public async Task<IActionResult> GetProductStats(int productId)
        {
            var stats = await _reviewService.GetProductStatsAsync(productId);
            return Ok(stats);
        }

        /// <summary>
        /// [POST] /api/reviews - Tạo review mới
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReviewResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Get userId from JWT token
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var review = await _reviewService.CreateAsync(userId.Value, request);
                return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [PUT] /api/reviews/{id} - Update review
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                // TODO: Check if user is admin from JWT claims
                var isAdmin = IsAdmin();

                var review = await _reviewService.UpdateAsync(id, userId.Value, request, isAdmin);
                return Ok(review);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Review not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// [DELETE] /api/reviews/{id} - Xóa review
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var isAdmin = IsAdmin();
                var success = await _reviewService.DeleteAsync(id, userId.Value, isAdmin);

                if (!success)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // === HELPER METHODS ===

        /// <summary>
        /// Helper: Lấy userId từ JWT token (placeholder - cần implement JWT)
        /// </summary>
        private int? GetCurrentUserId()
        {
            // TODO: Implement JWT token parsing
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // return int.TryParse(userIdClaim, out var userId) ? userId : null;

            // TEMPORARY: Return mock userId for testing
            return 1;
        }

        /// <summary>
        /// Helper: Check xem user có phải admin không
        /// </summary>
        private bool IsAdmin()
        {
            // TODO: Implement role checking from JWT
            // return User.IsInRole("Admin");

            // TEMPORARY: Return false for testing
            return false;
        }
    }
}