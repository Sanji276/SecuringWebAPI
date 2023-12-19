using SecuredWebAPIBestPractices.Model.Domain;

namespace SecuredWebAPIBestPractices.Repositories.Abstract
{
    public interface IBlogsRepository
    {
        Task<Guid> CreateBlogAsync(Post model, string userid);
        Task<bool> DeleteBlogAsync(Guid blogId);
        Task<bool> UserOwnBlogAsync(Guid postid, string getUserId);
    }
}