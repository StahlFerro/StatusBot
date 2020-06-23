/*
ReconnectService based on Foxbot's example
https://gist.github.com/foxbot/7d81edab4e36497c643828638af289b8
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;

namespace StatusBot.Services
{
    public class ReconnectService
    {
        // --- Begin Configuration Section ---
        // How long should we wait on the client to reconnect before resetting?
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        // Should we attempt to reset the client? Set this to false if your client is still locking up.
        private static readonly bool _attemptReset = true;

        // Change log levels if desired:
        //private static readonly LogSeverity _debug = LogSeverity.Debug;
        //private static readonly LogSeverity _info = LogSeverity.Info;
        //private static readonly LogSeverity _critical = LogSeverity.Critical;

        // --- End Configuration Section ---

        private readonly DiscordSocketClient _client;
        private CancellationTokenSource _cts;
        private readonly LogService LS;
        private readonly string label = "[ReconSrv]";
        private readonly ConsoleColor cc = ConsoleColor.Magenta;

        public ReconnectService(DiscordSocketClient client, LogService logservice, Func<LogMessage, Task> logger = null)
        {
            _cts = new CancellationTokenSource();
            _client = client;
            //_logger = logger ?? (_ => Task.CompletedTask);
            _client.Connected += ConnectedAsync;
            _client.Disconnected += DisconnectedAsync;
            LS = logservice;
            Console.WriteLine("ReconnectService initialized");
        }

        public Task ConnectedAsync()
        {
            // Cancel all previous state checks and reset the CancelToken - client is back online
            //_ = DebugAsync("[ReconnService] Client reconnected, resetting cancel tokens...");
            _ = LS.WriteAsync($"{label} Client reconnected, resetting cancel tokens...", cc);
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _ = LS.WriteAsync($"{label} Client reconnected, cancel tokens reset.", cc);
            return Task.CompletedTask;
        }

        public Task DisconnectedAsync(Exception _e)
        {
            // Check the state after <timeout> to see if we reconnected

            _ = LS.WriteAsync($"{label} Client disconnected, starting timeout task...", cc);
            _ = Task.Delay(_timeout, _cts.Token).ContinueWith(async _ => 
            {
                Task log_timeout_expire = LS.WriteAsync($"{label} Timeout expired, continuing to check client state...", cc);
                await CheckStateAsync();
                Task log_okay = LS.WriteAsync($"{label} State came back okay", cc);
            });
            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            // Client reconnected, no need to reset
            if (_client.ConnectionState == ConnectionState.Connected) return;
            if (_attemptReset)
            {
                Task logreset = LS.WriteAsync($"{label} Attempting to reset the client");

                var timeout = Task.Delay(_timeout);
                var connect = _client.StartAsync();
                var task = await Task.WhenAny(timeout, connect);

                if (task == timeout)
                {
                    Task logreset_timeout = LS.WriteAsync($"{label} Client reset timed out (task deadlocked?), killing process", cc);
                    FailFast();
                }
                else if (connect.IsFaulted)
                {
                    Task logreset_fault = LS.WriteAsync($"{label} Client reset faulted, killing process\n{connect.Exception}", cc);
                    FailFast();
                }
                else if (connect.IsCompletedSuccessfully)
                {
                    Task logreset_success = LS.WriteAsync($"{label} Client reset succesfully!", cc);
                }
                return;
            }
            Task logkill = LS.WriteAsync($"{label} Client did not reconnect in time, killing process and return with exit code 1", cc);
            FailFast();
        }

        private void FailFast()
            => Environment.Exit(1);

        // Logging Helpers
        //private const string LogSource = "Reconnector";
        //private Task DebugAsync(string message)
        //{
        //    message = TimeStamper(message);
        //    File.AppendAllTextAsync(logpath, message);
        //    return _logger.Invoke(new LogMessage(_debug, LogSource, message));
        //}
        //private Task InfoAsync(string message)
        //{
        //    message = TimeStamper(message);
        //    File.AppendAllTextAsync(logpath, message);
        //    return _logger.Invoke(new LogMessage(_info, LogSource, message));
        //}
        //private Task CriticalAsync(string message, Exception error = null)
        //{
        //    message = TimeStamper(message);
        //    File.AppendAllTextAsync(logpath, message);
        //    return _logger.Invoke(new LogMessage(_critical, LogSource, message, error));
        //}
    }
}
