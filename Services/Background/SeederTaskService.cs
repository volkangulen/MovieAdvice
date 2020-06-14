using MAdvice.DAL;
using MAdvice.Models;
using MAdvice.ServiceModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MAdvice.Services.Background
{
    /// <summary>
    /// The Movie DB'den periyodik olarak film çekip veri tabanında kaydetmek için kullanılır.
    /// </summary>
    public class SeederTaskService : TaskService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        //TheMovieDb'den kaç sayfa veri çekileceği constant olarak belirtildi..
        private const int PAGE_SIZE = 10;

        public SeederTaskService(IServiceScopeFactory serviceScopeFactory,IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var  movie_db_api_key = _configuration["TMDB:ApiKey"];
            //IHosted Service default olarak calcallation token istiyor. CancellationRequest edilene kadar sonsuza dek çalışır.
            while (!stoppingToken.IsCancellationRequested)
            {
                //Scoped olarak çalışan Movie Context Singleton scope'unda ayağa kalkamadığı için scope factory'den scoped olarak çağırılır.
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    //Movie context scope'u oluşturulur.
                    var movieContext = scope.ServiceProvider.GetRequiredService<MovieContext>();
                    //Filmler db'den çekilir.
                    var movies = await movieContext.Movies.ToListAsync();
                    //Film ID'leri key olarak dictionary'de tutulur.
                    var movie_ids = movies.ToDictionary(key => (long)key.Id, value => true);
                    using (var httpClient = new HttpClient())
                    {
                        for (int i = 1; i <= PAGE_SIZE; i++)
                        {
                            //Her sayfa için ayrı bir call atılır.
                            var url = $"https://api.themoviedb.org/3/movie/popular?api_key={movie_db_api_key}&page={i}";
                            using (var response = await httpClient.GetAsync(url))
                            {
                                //Gelen sonuçlar MovieSearchResultModel'e parse deserialize edilir.
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                var data = JsonConvert.DeserializeObject<MovieSearchResultModel>(apiResponse);
                                foreach (var result in data.results)
                                {
                                    //Eğer gelen kayıtlar db'de kayıtlı değil ise yeni kayıt oluşturulur.
                                    if (!movie_ids.ContainsKey(result.id))
                                    {
                                        var releaseDate = DateTime.ParseExact(result.release_date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).Date;
                                        var movie = new Movie
                                        {
                                            ReleaseDate = releaseDate,
                                            Title = result.title,
                                            Id = result.id,
                                            Description = result.overview,
                                            AverageVote = result.vote_average,
                                            VoteCount = Int32.Parse(result.vote_count)
                                        };
                                        await movieContext.AddAsync(movie);
                                    }
                                }

                            }
                        }
                        await movieContext.SaveChangesAsync();
                    }
                }
                //1 saat beklenip aynı işlemler tekrar çalıştırılır.
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
