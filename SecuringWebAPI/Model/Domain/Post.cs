using SecuringWebAPI.Model.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecuredWebAPIBestPractices.Model.Domain
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }
        public string? PostName { get; set; }
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
