using System.ComponentModel.DataAnnotations;

namespace MAdvice.ParameterModels
{
    public class RatingModel
    {
        [Required]
        public int MovieId { get; set; }
        [Required]
        [Range(1, 10, ErrorMessage = "Girilen puan {1} ile {2} arasında olmalıdır.")]
        public short Vote { get; set; }
        public string Note { get; set; }

    }
}
