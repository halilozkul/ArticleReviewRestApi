using ArticleRestApi.Models;

namespace ArticleRestApi.Service.ServiceInterface
{
    public interface IArticleService
    {
        Task<List<Article>> GetAsync();
        Task<Article> GetAsync(string id);
        Task CreateAsync(Article article);
        Task UpdateAsync(Article article);
        Task<string> RemoveAsync(string id);
    }
}
