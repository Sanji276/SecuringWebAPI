using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecuredWebAPIBestPractices.Extensions;
using SecuredWebAPIBestPractices.Model.Domain;
using SecuredWebAPIBestPractices.Repositories.Abstract;
using SecuringWebAPI.Model.DTO;

namespace SecuredWebAPIBestPractices.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogsRepository _blogsRepository;

        public BlogController(IBlogsRepository blogsRepository)
        {
            _blogsRepository = blogsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post postrequest)
        {
            
            string userid = HttpContext.GetUserId();
            var response = await _blogsRepository.CreateBlogAsync(postrequest, userid);
            if (string.IsNullOrEmpty(response.ToString()))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new Blog");
            }
            return CreatedAtAction(nameof(Create), "Blog has been created with id=" + new { id = response }, postrequest);
        }

        [HttpDelete("~/api/delete/{postid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid postid)
        {
            var userOwnBlog = await _blogsRepository.UserOwnBlogAsync(postid, HttpContext.GetUserId().ToString());
            if (!userOwnBlog)
            {
                return BadRequest(new {error = "You don't have rights to delete this."});
            }
            var response = await _blogsRepository.DeleteBlogAsync(postid);

            if (!response)
                return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new Blog");

            return NoContent();
        }
    }
}
