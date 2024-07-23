using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ReviewRestApi.Controllers;
using ReviewRestApi.Service.ServiceInterface;
using ReviewRestApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq.Protected;
using System.Net;

namespace ReviewRestApi.Tests
{
    public class ReviewsControllerTests
    {
        private readonly ReviewController _controller;
        private readonly Mock<IReviewService> _mockService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ReviewController>> _mockLogger;

        public ReviewsControllerTests()
        {
            _mockService = new Mock<IReviewService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ReviewController>>();
            _controller = new ReviewController(_mockService.Object, _mockCache.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetReviews_ReturnsOkResultWithListOfReviews()
        {
            var reviews = new List<Review>
            {
                new Review { Id = "1", ReviewContent = "Test Review 1", ArticleId = "000000000000000000000001" },
                new Review { Id = "2", ReviewContent = "Test Review 2", ArticleId = "000000000000000000000002" }
            };

            _mockService.Setup(s => s.GetAsync()).ReturnsAsync(reviews);

            object cachedReviews = null;
            _mockCache.Setup(c => c.TryGetValue("reviews", out cachedReviews)).Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var result = await _controller.GetReviews();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReviews = Assert.IsType<List<Review>>(okResult.Value);
            Assert.Equal(reviews.Count, returnedReviews.Count);

        }

        [Fact]
        public async Task GetReview_ExistingId_ReturnsOkResult()
        {
            var reviewId = "review123";
            var review = new Review { Id = reviewId, Reviewer = "Reviewer 1", ReviewContent = "Great article!" };

            _mockService.Setup(s => s.GetAsync(reviewId)).ReturnsAsync(review);

            var result = await _controller.GetReview(reviewId);

            var actionResult = Assert.IsType<ActionResult<Review>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<Review>(okResult.Value);
            Assert.Equal(reviewId, returnValue.Id);
        }

        [Fact]
        public async Task CreateReview_ValidReview_ReturnsCreatedAtAction()
        {
            var review = new Review { Id = "1", ArticleId = "000000000000000000000001", ReviewContent = "Great article!" };

            var articlesResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(articlesResponseMessage);
            var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://article-api/")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockService.Setup(s => s.CreateAsync(review)).Returns(Task.CompletedTask);

            var result = await _controller.PostReview(review);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedReview = Assert.IsType<Review>(createdAtActionResult.Value);
            Assert.Equal(review.Id, returnedReview.Id);
            Assert.Equal(review.ArticleId, returnedReview.ArticleId);
            Assert.Equal(review.ReviewContent, returnedReview.ReviewContent);
        }

        [Fact]
        public async Task UpdateReview_ValidReview_ReturnsOkResult()
        {
            var reviewId = "valid-review-id";
            var review = new Review
            {
                Id = reviewId,
                ArticleId = "some-article-id",
                ReviewContent = "Updated review content"
            };

            _mockService.Setup(s => s.UpdateAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateReview(review);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedReview = Assert.IsType<Review>(okResult.Value);
            Assert.Equal(review.Id, returnedReview.Id);
            Assert.Equal(review.ReviewContent, returnedReview.ReviewContent);
        }

        [Fact]
        public async Task DeleteReview_ExistingId_ReturnsOk()
        {
            var reviewId = "existing-id";
            var expectedResult = $"Review with id {reviewId} has been successfully deleted.";

            _mockService.Setup(s => s.RemoveAsync(reviewId)).ReturnsAsync(expectedResult);
            _mockService.Setup(s => s.GetAsync(reviewId)).ReturnsAsync(new Review { Id = reviewId });

            var result = await _controller.DeleteReview(reviewId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessage = Assert.IsType<string>(okResult.Value);
            Assert.Equal(expectedResult, returnedMessage);
        }
    }
}
