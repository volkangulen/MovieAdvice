using MAdvice.Extensions;
using MAdvice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAdvice.Services {

    public interface IMovieService {
        Task<IEnumerable<Movie>> GetMoviesAsync(int page, int pageSize);
        Task<Movie> GetMovieAsync(int movieId);
        Task<TaskResult<Movie>> RateMovieAsync(int movieId,short rating, string note,int userId);
        Task<TaskResult<IEnumerable<Movie>>> RecommendMovieAsync(int movieId,string userEmail);
    }
}