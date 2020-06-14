using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MAdvice.Models
{
    public class User
    {
        //Login metodundan dönen user bilgisinde id gönderilmemesi için JsonIgnore attribute eklendi.
        [JsonIgnore]
        public int Id { get; set; }
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        //Login metodundan dönen user bilgisinde şifre gönderilmemesi için JsonIgnore attribute eklendi.
        [JsonIgnore]
        public string Password { get; set; }

        //DB'ye kaydedilmeyecek. NotMapped ile database mappinge eklenmeyecek.
        [NotMapped]
        public string Token { get; set; }
        [NotMapped]
        public DateTime TokenExpirationDate { get; set; }

        [JsonIgnore]
        public virtual ICollection<MovieRating> MovieRatings { get; set; }

        public User(string username,string password,string email)
        {
            this.Username = username;
            this.Password = password.ToSha1();
            this.Email = email;
        }
    }
}