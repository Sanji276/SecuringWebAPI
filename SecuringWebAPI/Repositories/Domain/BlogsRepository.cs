using Microsoft.EntityFrameworkCore;
using SecuredWebAPIBestPractices.Model.Domain;
using SecuredWebAPIBestPractices.Repositories.Abstract;
using SecuringWebAPI.Data;

namespace SecuredWebAPIBestPractices.Repositories.Domain
{
    public class BlogsRepository : IBlogsRepository
    {
        private readonly SecuringWebAPIContext _dbContext;

        public BlogsRepository(SecuringWebAPIContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateBlogAsync(Post model,string userid)
        {
            var blog = new Post
            {
                PostName = model.PostName,
                UserId = userid.ToString()
            };
            var result = await _dbContext.Posts.AddAsync(blog);
            
            await _dbContext.SaveChangesAsync();
            return blog.Id;
        }

        public async Task<bool> DeleteBlogAsync(Guid blogId)
        {
            var blog = await _dbContext.Posts.FindAsync(blogId);
            if(blog == null)
            {
                return false;
            }

            var result = _dbContext.Posts.Remove(blog);            
            await _dbContext.SaveChangesAsync();
            return true;
            
        }

        public async Task<bool> UserOwnBlogAsync(Guid postid, string getUserId)
        {
            var post = await _dbContext.Posts.SingleOrDefaultAsync(x=>x.Id == postid);
            if(post == null)
            {
                return false;
            }
            if(post.UserId !=  getUserId)
            {
                return false;
            }
            return true;
        }
    }
}
