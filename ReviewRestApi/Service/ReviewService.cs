using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using ReviewRestApi.Models;
using ReviewRestApi.Service.ServiceInterface;
using System.Text.RegularExpressions;

namespace ReviewRestApi.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IMongoCollection<Review> _reviews;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReviewService(IMongoClient client, IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            var database = client.GetDatabase("ArticleDb");
            _reviews = database.GetCollection<Review>("Reviews");
            _httpClient = new HttpClient();
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Review>> GetAsync()
        {
            if (!_cache.TryGetValue("GetAllReviews", out List<Review> reviews))
            {
                reviews = await _reviews.Find(review => true).ToListAsync();
                _cache.Set("GetAllReviews", reviews);
            }
            return reviews;
        }

        public async Task<Review> GetAsync(string id)
        {
            ValidateIdFormat(id, false);

            if (!_cache.TryGetValue($"GetReview_{id}", out Review review))
            {
                review = await _reviews.Find<Review>(review => review.Id == id).FirstOrDefaultAsync();
                if (review == null)
                {
                    throw new KeyNotFoundException($"Review with id {id} does not exist.");
                }
                else
                {
                    _cache.Set($"GetReview_{id}", review);
                }
            }
            return review;
        }

        public async Task CreateAsync(Review review)
        {
            ValidateIdFormat(review.Id, true);
            ValidateIdFormat(review.ArticleId, false);
            var articleExists = await ArticleExistsAsync(review.ArticleId);
            if (articleExists)
            {
                await _reviews.InsertOneAsync(review);
                _cache.Remove("GetAllReviews");
            }
            else
            {
                throw new HttpRequestException($"Article with id {review.ArticleId} does not exist");
            }
        }

        public async Task UpdateAsync(Review review)
        {
            ValidateIdFormat(review.Id, false);
            ValidateIdFormat(review.ArticleId, false);

            var existingReview = await GetAsync(review.Id);
            if (existingReview == null)
            {
                throw new KeyNotFoundException($"Review with id {review.Id} does not exist.");
            }

            var articleExists = await ArticleExistsAsync(review.ArticleId);
            if (!articleExists)
            {
                throw new HttpRequestException($"Article with id {review.ArticleId} does not exist");
            }
            await _reviews.ReplaceOneAsync(r => r.Id == review.Id, review);
            _cache.Remove($"GetReview_{review.Id}");
            _cache.Remove("GetAllReviews");
        }

        public async Task<string> RemoveAsync(string id)
        {
            ValidateIdFormat(id, false);

            var review = await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (review == null)
            {
                throw new KeyNotFoundException($"Review with id {id} does not exist.");
            }

            await _reviews.DeleteOneAsync(review => review.Id == id);
            _cache.Remove($"GetReview_{id}");
            _cache.Remove("GetAllReviews");
            return $"Review with id {id} has been successfully deleted.";
        }

        private async Task<bool> ArticleExistsAsync(string articleId)
        {
            var client = _httpClientFactory.CreateClient();
            //var response = await client.GetAsync($"http://localhost:5002/api/articles/{articleId}");
            var response = await client.GetAsync($"http://host.docker.internal:5002/api/v1/articles/{articleId}");
            return response.IsSuccessStatusCode;
        }

        private static void ValidateIdFormat(string id, bool allowEmpty)
        {
            if (string.IsNullOrEmpty(id) && allowEmpty)   //generates automatically
                return;
            if (id.Length != 24 || !Regex.IsMatch(id, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                throw new ArgumentException($"Invalid ID format. It must be a 24-digit hex string.");
            }
        }
    }
}
