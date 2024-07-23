using ArticleRestApi.Controllers;
using ArticleRestApi.Models;
using ArticleRestApi.Service.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace ArticleRestApi.Tests
{
    public class ArticlesControllerTests
    {
        private readonly ArticlesController _controller;
        private readonly Mock<IArticleService> _mockService;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ArticlesController>> _mockLogger;

        public ArticlesControllerTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockService = new Mock<IArticleService>();
            _mockLogger = new Mock<ILogger<ArticlesController>>();
            _controller = new ArticlesController(_mockService.Object, _mockCache.Object, _mockLogger.Object, null);
        }

        [Fact]
        public async Task GetArticles_ReturnsOkResultWithListOfArticles()
        {
            var articles = new List<Article>
            {
                new Article { Id = "1", Title = "Test Article 1" },
                new Article { Id = "2", Title = "Test Article 2" }
            };

            _mockService.Setup(s => s.GetAsync()).ReturnsAsync(articles);

            object cachedArticles;
            _mockCache.Setup(x => x.TryGetValue("articles", out cachedArticles)).Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var result = await _controller.GetArticles();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedArticles = Assert.IsType<List<Article>>(okResult.Value);
            Assert.Equal(articles.Count, returnedArticles.Count);
        }

        [Fact]
        public async Task GetArticle_ExistingId_ReturnsOkResult()
        {
            var articleId = "article123";
            var article = new Article { Id = articleId, Title = "Title 1" };

            _mockService.Setup(s => s.GetAsync(articleId)).ReturnsAsync(article);

            var result = await _controller.GetArticle(articleId);

            var actionResult = Assert.IsType<ActionResult<Article>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<Article>(okResult.Value);
            Assert.Equal(articleId, returnValue.Id);
        }

        [Fact]
        public async Task CreateArticle_ValidArticle_ReturnsCreatedAtAction()
        {
            var article = new Article { Id = "article123", Title = "Sample Article" };
            var result = await _controller.PostArticle(article);

            var actionResult = Assert.IsType<ActionResult<Article>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);

            var returnedArticle = Assert.IsType<Article>(createdAtActionResult.Value);
            Assert.Equal(article.Id, returnedArticle.Id);
            Assert.Equal(article.Title, returnedArticle.Title);
        }

        [Fact]
        public async Task UpdateArticle_ValidArticle_ReturnsOk()
        {
            // Arrange
            var articleId = "valid-id";
            var updatedArticle = new Article { Id = articleId, Title = "Updated Title" };

            // Mock the service method to simulate successful update
            _mockService.Setup(s => s.UpdateAsync(updatedArticle))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateArticle(updatedArticle);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedArticle = Assert.IsType<Article>(okResult.Value);
            Assert.Equal(articleId, returnedArticle.Id);
        }

        [Fact]
        public async Task DeleteArticle_ExistingId_ReturnsOk()
        {
            var articleId = "existing-id";
            var successMessage = $"Article with id {articleId} has been successfully deleted.";

            _mockService.Setup(s => s.GetAsync(articleId))
                        .ReturnsAsync(new Article { Id = articleId });
            _mockService.Setup(s => s.RemoveAsync(articleId))
                        .ReturnsAsync(successMessage);

            var result = await _controller.DeleteArticle(articleId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(successMessage, okResult.Value);
        }
    }
}
