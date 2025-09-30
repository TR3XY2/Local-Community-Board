using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Subscriptions")]
    public class Subscription
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Column("location_id")]
        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}