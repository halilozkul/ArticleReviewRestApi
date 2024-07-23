using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ReviewRestApi.Models;
using ReviewRestApi.Service.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace ReviewRestApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, IMemoryCache cache, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _cache = cache;
            _logger = logger;
        }

        // GET: api/v1/Reviews
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            try
            {
                if (_cache.TryGetValue("reviews", out List<Review> cachedReviews))
                {
                    _logger.LogInformation("Retrieved reviews from cache.");
                    return Ok(cachedReviews);
                }

                var reviews = await _reviewService.GetAsync();
                if (reviews == null || !reviews.Any())
                {
                    return NoContent();
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };
                _cache.Set("reviews", reviews, cacheEntryOptions);

                _logger.LogInformation("Retrieved reviews from service and cached them.");
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting reviews.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/v1/Reviews/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Review>> GetReview(string id)
        {
            try
            {
                var review = await _reviewService.GetAsync(id);
                return Ok(review);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the review.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/v1/Reviews
        [HttpPost]
        [ProducesResponseType(typeof(Review), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            try
            {
                await _reviewService.CreateAsync(review);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"An error occurred while checking if the article with ID {review.ArticleId} exists.");
                return StatusCode(400, ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "An argument exception occurred.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the review.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/v1/Reviews/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReview(Review review)
        {
            try
            {
                await _reviewService.UpdateAsync(review);
                return Ok(review);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the review.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/v1/Reviews/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReview(string id)
        {
            try
            {
                var review = await _reviewService.GetAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var result = await _reviewService.RemoveAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the review.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
