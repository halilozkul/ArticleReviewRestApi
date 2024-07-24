using ArticleRestApi.Models;
using ArticleRestApi.Service.ServiceInterface;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace ArticleRestApi.Service
{
    public class ArticleService : IArticleService
    {
        private readonly IMongoCollection<Article> _articles;
        private readonly IMemoryCache _cache;

        public ArticleService(IMongoClient client, IMemoryCache cache)
        {
            var database = client.GetDatabase("ArticleDb");
            _articles = database.GetCollection<Article>("Articles");
            _cache = cache;
        }

        public async Task<List<Article>> GetAsync()
        {
            if (!_cache.TryGetValue("GetAllArticles", out List<Article> articles))
            {
                articles = await _articles.Find(article => true).ToListAsync();
                _cache.Set("GetAllArticles", articles);
            }
            return articles;
        }

        public async Task<Article> GetAsync(string id)
        {
            ValidateIdFormat(id, false);
            if (!_cache.TryGetValue($"GetArticle_{id}", out Article article))
            {
                article = await _articles.Find<Article>(article => article.Id == id).FirstOrDefaultAsync();

                if (article == null)
                {
                    throw new KeyNotFoundException($"Article with id {id} does not exist.");
                }
                else
                {
                    _cache.Set($"GetArticle_{id}", article);
                }
            }
            return article;
        }

        public async Task CreateAsync(Article article)
        {
            ValidateIdFormat(article.Id, true);
            await _articles.InsertOneAsync(article);
            _cache.Remove("GetAllArticles");
        }

        public async Task UpdateAsync(Article article)
        {
            ValidateIdFormat(article.Id, false);

            var existingArticle = await _articles.Find(a => a.Id == article.Id).FirstOrDefaultAsync();
            if (existingArticle == null)
            {
                throw new KeyNotFoundException($"Article with ID {article.Id} does not exist.");
            }

            await _articles.ReplaceOneAsync(a => a.Id == article.Id, article);
            _cache.Remove($"GetArticle_{article.Id}");
            _cache.Remove("GetAllArticles");
        }

        public async Task<string> RemoveAsync(string id)
        {
            ValidateIdFormat(id, false);

            var article = await _articles.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (article == null)
            {
                throw new KeyNotFoundException($"Article with id {id} does not exist.");
            }

            await _articles.DeleteOneAsync(a => a.Id == id);
            _cache.Remove("GetAllArticles");
            _cache.Remove($"GetArticle_{article.Id}");

            return $"Article with id {id} has been successfully deleted.";
        }

        private static void ValidateIdFormat(string id, bool allowEmpty)
        {
            if (string.IsNullOrEmpty(id) && allowEmpty)   //generates automatically
                return;
            if (id.Length != 24 || !Regex.IsMatch(id, @"\A\b[0-9a-fA-F]+\b\Z"))
                throw new ArgumentException($"Invalid ID format. It must be a 24-digit hex string.");
        }
    }
}
