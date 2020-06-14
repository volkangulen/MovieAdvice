using Microsoft.AspNetCore.Mvc;
using MAdvice.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using MAdvice.ParameterModels;
using System.Net.Mail;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MAdvice.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
   
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("getmovies")]
        public async Task<IActionResult> GetMoviesAsync([Required]int page,[Required] int pageSize)
        {
            try
            {
                var movies = await _movieService.GetMoviesAsync(page, pageSize);
                return Ok(movies);
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpGet("getmovie/{movieId}")]
        public async Task<IActionResult> GetMovieAsync(int movieId)
        {
            var movie = await _movieService.GetMovieAsync(movieId);
            if (movie == null)
                return NoContent();
            return Ok(movie);
        }
        [HttpPost("rate")]
        public async Task<IActionResult> RateMovieAsync([FromBody] RatingModel model)
        {
            try
            {
                var rateTask = await _movieService.RateMovieAsync(model.MovieId, model.Vote, model.Note, this.HttpContext.GetUserId());
                if (rateTask.Success)
                    return Ok(rateTask);
                else
                    return NotFound(rateTask);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("recommend/{movieId}")]
        public async Task<IActionResult> RecommendMovieAsync(int movieId)
        {
            try
            {
                var result = await _movieService.RecommendMovieAsync(movieId,this.HttpContext.GetUserEmail());
                return Ok(result);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}