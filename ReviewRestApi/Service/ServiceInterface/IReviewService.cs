using ReviewRestApi.Models;

namespace ReviewRestApi.Service.ServiceInterface
{
    public interface IReviewService
    {
        Task<List<Review>> GetAsync();
        Task<Review> GetAsync(string id);
        Task CreateAsync(Review review);
        Task UpdateAsync(Review review);
        Task<string> RemoveAsync(string id);
    }
}
