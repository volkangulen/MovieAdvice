using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAdvice.Extensions
{
    /// <summary>
    /// Yalnızca işlem sonucu ve mesaj döndürmek için kullanılır.
    /// </summary>
    public class TaskResult
    {
        public TaskResult(bool success)
        {
            this.Success = success;
        }

        public TaskResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public bool Success { get; set; }

        public string Message { get; set; }
    }
    /// <summary>
    /// TaskResult classı dönüş ögeleri için kullanılır. İstenilen işlemin başarı ile tamamlanıp tamamlanmadığı , hata alınması durumunda hata mesajı , döndürülmek için kullanılır.
    /// Genellikle başarılı işlemlerde T typeparam tipindeki veri Data parametresinde döndürülür.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskResult<T>
    {

        public TaskResult(bool success, T data)
        {
            this.Success = success;
            this.Data = data;
        }
        public TaskResult(bool success, T data, string message)
        {
            this.Success = success;
            this.Data = data;
            this.Message = message;
        }
       

        public bool Success { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }
}
