using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ArticleRestApi.Models;
using ArticleRestApi.Service.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleRestApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ArticlesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ArticlesController(IArticleService articleService, IMemoryCache cache, ILogger<ArticlesController> logger, IHttpClientFactory httpClientFactory)
        {
            _articleService = articleService;
            _cache = cache;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/v1/Articles
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            try
            {
                var cacheKey = "articles";
                if (!_cache.TryGetValue(cacheKey, out List<Article> articles))
                {
                    articles = await _articleService.GetAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                        SlidingExpiration = TimeSpan.FromMinutes(1)
                    };
                    _cache.Set(cacheKey, articles, cacheEntryOptions);
                }

                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting articles.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/v1/Articles/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> GetArticle(string id)
        {
            try
            {
                var article = await _articleService.GetAsync(id);
                if (article == null)
                {
                    return NotFound($"Article with ID {id} not found.");
                }

                return Ok(article);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid ID format: {id}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Article with ID {id} not found.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting article with ID {id}.");
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }

        // POST: api/v1/Articles
        [HttpPost]
        [ProducesResponseType(typeof(Article), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            try
            {
                await _articleService.CreateAsync(article);
                return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid article data.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an article.");
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }

        // PUT: api/v1/Articles/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateArticle(Article article)
        {
            try
            {
                await _articleService.UpdateAsync(article);
                return Ok(article);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Article with ID {article.Id} not found.");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid article data.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating article with ID {article.Id}.");
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }

        // DELETE: api/v1/Articles/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            try
            {
                var article = await _articleService.GetAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                var result = await _articleService.RemoveAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Article with ID {id} not found.");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid article ID.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting article with ID {id}.");
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }
        }
    }
}
