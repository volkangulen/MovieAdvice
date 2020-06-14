using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MAdvice.ServiceModels
{
    public class MovieSearchResultModel
    {
        public int page { get; set; }
        public int total_pages { get; set; }
        public int total_results { get; set; }
        public List<MovieResult> results { get; set; }
    }
    public class MovieResult
    {
        public string poster_path { get; set; }
        public bool adult { get; set; }
        public string overview { get; set; }
        public string release_date { get; set; }
        public List<int> genre_ids { get; set; }
        public int id { get; set; }
        public string original_title { get; set; }
        public string original_language { get; set; }
        public string title { get; set; }
        public string backdrop_path { get; set; }
        public decimal popularity { get; set; }
        public string vote_count { get; set; }
        public bool video { get; set; }
        public decimal vote_average { get; set; }
    }
}
