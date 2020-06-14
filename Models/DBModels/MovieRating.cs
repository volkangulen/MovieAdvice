using System.ComponentModel.DataAnnotations;

namespace MAdvice.Models
{
    public class MovieRating
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }

        [Range(1, 10,ErrorMessage = "Girilen puan {1} ile {2} arasında olmalıdır.")]
        public short Vote { get; set; }
        public string Note { get; set; }

        public virtual User User { get; set; }
        public virtual Movie Movie { get; set; }
    }
}