using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MAdvice.Services.Background
{
    /// <summary>
    /// Background JOBlar için abstract class yapısı. Host tarafından yönetiliyor.
    /// </summary>
    public abstract class TaskService : IHostedService, IDisposable
    {
        private Task currentTask;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken cToken);
        
        //Startup'ta host tarafından çalıştırılıyor.
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            currentTask = ExecuteAsync(cancellationTokenSource.Token);
            
            if (currentTask.IsCompleted)
                return currentTask;

            return Task.CompletedTask;
        }
        
        //Cancellation token geldiği zaman taskları durdurmak için kullanılır.
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (currentTask == null)
                return;

            try
            {
                cancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(currentTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public virtual void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
