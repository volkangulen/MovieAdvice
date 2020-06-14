using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MAdvice.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Decimal AverageVote { get; set; }
        public int VoteCount { get; set; }


        #region Navigation Properties
        
        [JsonIgnore]
        public virtual ICollection<MovieRating> MovieRatings { get; set; }

        #endregion
    }
}