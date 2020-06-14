using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MAdvice.DAL;
using MAdvice.Extensions;
using MAdvice.Models;
using MAdvice.ServiceModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MAdvice.Services
{
    public class MovieService : IMovieService
    {
        private readonly MovieContext _movieContext;
        private readonly IConfiguration _configuration;

        public MovieService(IConfiguration configuration,MovieContext movieContext)
        {
            _movieContext = movieContext;
            _configuration = configuration;
        }
        /// <summary>
        /// Filmleri listelemek için kullanılır. Listelenen sayfa sayısı ve sayfa başına kayıt sayısı parametre olarak verilir.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns><see cref="IEnumerable{Movie}"/></returns>
        public async Task<IEnumerable<Movie>> GetMoviesAsync(int page, int pageSize)
        {
            var movies = await _movieContext.Movies.OrderBy(q => q.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return movies;
        }
        /// <summary>
        /// Id'si verilen film detayını görüntülemek için kullanılır.
        /// </summary>
        /// <param name="movieId"></param>
        /// <returns><see cref="Movie"/></returns>
        public async Task<Movie> GetMovieAsync(int movieId)
        {
            var movie = await _movieContext.Movies.SingleOrDefaultAsync(q => q.Id == movieId);
            return movie;

        }
        /// <summary>
        /// Filme yorum ve not vermek için kullanılır. Aynı kullanıcı ile daha önce yorum yapılmış ise üzerine yazılır.
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="vote"></param>
        /// <param name="note"></param>
        /// <param name="userId"></param>
        /// <returns><see cref="TaskResult{Movie}"/></returns>
        public async Task<TaskResult<Movie>> RateMovieAsync(int movieId, short vote, string note, int userId)
        {
            try
            {
                var movie = await _movieContext.Movies.FirstOrDefaultAsync(q => q.Id == movieId);
                if (movie == null)
                    return new TaskResult<Movie>(false, null, "Aradığınız film bulunamadı.");

                //Kullanıcının daha önce değerlendirmesi getirilir.
                var movieRating = await _movieContext.MovieRatings.FirstOrDefaultAsync(q => q.MovieId == movieId && q.UserId == userId);
                
                //Değerlendirmesi yok ise yeni yaratılır.
                if (movieRating == null)
                {
                    movieRating = new MovieRating
                    {
                        MovieId = movieId,
                        Note = note,
                        Vote = vote,
                        UserId = userId
                    };

                    //İlgili filmin değerlendirme detayları güncellenir.
                    movie.AverageVote = (movie.VoteCount * movie.AverageVote + vote) / (movie.VoteCount + 1);
                    movie.VoteCount += 1;


                    await _movieContext.MovieRatings.AddAsync(movieRating);
                }
                //Değerlendirmesi var ise
                else
                {
                    movieRating.Note = note;
                    //İlgili filmin değerlendirme detayları eski değerlendirmenin farkı ile güncellenir.
                    movie.AverageVote = (movie.AverageVote * movie.VoteCount + (vote - movieRating.Vote)) / movie.VoteCount;
                    movieRating.Vote = vote;

                }
                await _movieContext.SaveChangesAsync();


                return new TaskResult<Movie>(true, movie);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// Film tavsiyesi için kullanılır.
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<TaskResult<IEnumerable<Movie>>> RecommendMovieAsync(int movieId, string userEmail)
        {
            var movie_db_api_key = _configuration["TMDB:ApiKey"];
            var movieList = new List<Movie>();
            using (var httpClient = new HttpClient())
            {
                var url = $"https://api.themoviedb.org/3/movie/{movieId}/recommendations?api_key={movie_db_api_key}&page=1";

                //Mail atılacak body için html başlangıcı.
                var html = new StringBuilder("<html><table><thead><tr><th>Id</th><th>Title</th><th>Description</th><th>ReleaseDate</th></tr></thead><tbody>");

                //The movie Db Api call atılır.
                using (var response = await httpClient.GetAsync(url))
                {
                    
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    //Response MovieSearchResultModel olarak deserialize edilir.
                    var data = JsonConvert.DeserializeObject<MovieSearchResultModel>(apiResponse);

                    foreach (var result in data.results)
                    {
                        var release_date = result.release_date;
                        var date = DateTime.ParseExact(release_date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).Date;
                        //Mail içeriğine tavsiye edilen kayıt atılır.
                        html.Append($"<tr><td>{result.id}</td><td>{result.original_title}</td><td>{result.overview}</td><td>{date}</td></tr>");

                        //Eğer tavsiye edilen film db'de kayıtlı değil ise yeni kayıt oluşturulur.
                        if (!(await _movieContext.Movies.AnyAsync(q => q.Id == result.id)))
                        {

                            var movie = new Movie
                            {
                                ReleaseDate = date,
                                Title = result.title,
                                Id = result.id,
                                Description = result.overview,
                                AverageVote = result.vote_average,
                                VoteCount = Int32.Parse(result.vote_count)
                            };
                            movieList.Add(movie);

                        }
                    }

                    //Mail içeriği için table ,html tagleri kapatılır.
                    html.Append("</tbody></table></html>");

                }
                await _movieContext.AddRangeAsync(movieList);
                await _movieContext.SaveChangesAsync();

                //Mail içeriği hazırlanır.
                var adress = _configuration["MailClient:MailAdress"];
                var displayName = _configuration["MailClient:DisplayName"];
                var from = new MailAddress(adress, displayName);
                var mailMessage = new MailMessage()
                {
                    From = from,
                    To = { userEmail },
                    Subject = "Movie Recommendation",
                    Body = html.ToString(),
                    IsBodyHtml = true
                };
                try
                {
                    //Mail atılır.
                    await SendMailAsync(mailMessage);
                }
                catch
                {
                    //Herhangi bir problemle karşılaşıldığında ilgili hata mesajı ile dönülür.
                    return new TaskResult<IEnumerable<Movie>>(false, null, "Mail atılırken bir sorun oluştu.");
                }
                finally
                {
                    mailMessage.Dispose();
                }
            }
            return new TaskResult<IEnumerable<Movie>>(true, movieList);
        }
        /// <summary>
        /// Parametre olarak gönderilen mail içeriğini atmak için kullanılır. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private Task SendMailAsync(MailMessage email)
        {
            var host = _configuration["MailClient:SMTPHost"];
            Int32.TryParse(_configuration["MailClient:SMTPPort"],out int port);
            var password = _configuration["MailClient:Password"];
            var username = _configuration["MailClient:Username"];

            var client = new SmtpClient(host, port);
            try
            {
                var taskCompletionSource = new TaskCompletionSource<object>();
                //Credentiallar girilir.
                client.Credentials = new NetworkCredential(username, password);
                //SSL encryption için true seçildi.
                client.EnableSsl = true;

                //Mail gönderilimi tamamlanınca yapılacaklar
                client.SendCompleted += (o, e) =>
                {
                    client.Dispose();
                    email.Dispose();
                    if (e.Error != null)
                    {
                        taskCompletionSource.TrySetException(e.Error);
                    }
                    else if (e.Cancelled)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        taskCompletionSource.TrySetResult(null);
                    }
                };
                //10sn timeout süresi set edilir.
                client.Timeout = 10000;
                //Mail gönderilir.
                client.SendAsync(email, null);
                return taskCompletionSource.Task;
            }
            catch
            {
                client.Dispose();
                email.Dispose();
                throw;
            }
        }

    }
}