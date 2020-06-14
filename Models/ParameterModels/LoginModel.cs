using Microsoft.AspNetCore.Components.Forms;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MAdvice.ParameterModels
{
    /// <summary>
    /// Login ve register işlemlerinde http request body'sinde kullanılacak parametreler için kullanılır.
    /// </summary>
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [CustomValidation(typeof(RegisterModel), "CheckPasswordFormat")]
        public string Password { get; set; }

        
    }
    public class RegisterModel : LoginModel
    {
        [Required]
        [CustomValidation(typeof(RegisterModel), "IsValidEmail")]
        public string Email { get; set; }

        //Şifre kontrolü.
        public static ValidationResult CheckPasswordFormat(string password, ValidationContext pValidationContext)
        {
            if(pValidationContext.ObjectType.Name == "LoginModel")
                return ValidationResult.Success;
            bool check = Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+|~\-=\{}[\]:<>\?\.\/])(?=.*\d).{8,14}$");

            if (!check)
                return new ValidationResult("Şifreniz minimum 8 karakter,maksimum 14 karakter olmalı. En az 1 büyük karakter, 1 küçük karakter , 1 rakam ve 1 özel karakter kombinasyonundan oluşmalıdır.");

            return ValidationResult.Success;
        }

        //Email verification. From: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        public static ValidationResult IsValidEmail(string email, ValidationContext pValidationContext)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult("Email boş girilemez.");

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return new ValidationResult("İşleminiz yapılırken zaman aşımına uğradı.");
            }
            catch (ArgumentException e)
            {
                return new ValidationResult("Argüman hatası");
            }

            try
            {
                bool success = Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                if(success)
                    return ValidationResult.Success;
                return new ValidationResult("Lütfen mail adresinizi kontrol ediniz.");
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationResult("İşleminiz yapılırken zaman aşımına uğradı.");
            }
        }
    }
    
}
