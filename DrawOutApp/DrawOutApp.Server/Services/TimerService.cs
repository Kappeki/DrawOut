using DrawOutApp.Server.Hubs;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DrawOutApp.Server.Services
{
    public class TimerService : ITimerService
    {
        private readonly ConcurrentDictionary<string, GameTimers> _gameTimers = new ConcurrentDictionary<string, GameTimers>();

        public GameTimers GetOrAdd(string key)
        {
            return _gameTimers.GetOrAdd(key, _ => new GameTimers());
        }
        public bool TryGetValue(string key, out GameTimers gameTimers)
        {
            return _gameTimers.TryGetValue(key, out gameTimers!);
        }
    }

    public class GameTimers
    {
        private Timer? _wordSelectTimer;
        private Timer? _mainTimer;
        private Timer? _stealTimer;
        private int _remainingTime;
        private Timer? _countdownUpdateTimer;
        private bool _wasStopped;

        private TaskCompletionSource<bool>? _tcs;

        private readonly object _lock = new object();

        public async Task<bool> StartWordSelectTimer(string gameId, int duration, IHubClients clients)
        {
            StopAllTimers();

            lock (_lock)
            {
                _remainingTime = duration;
                _wasStopped = false;
                _tcs = new TaskCompletionSource<bool>();
            }

            await clients.Group(gameId).SendAsync("TimerStarted", "WordSelectTimer", _remainingTime);

           
            _wordSelectTimer = new Timer(_ =>
            {
                lock (_lock)
                {
                    _tcs?.TrySetResult(_wasStopped);
                }
                StopAllTimers();
            }, null, TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);

            StartCountdown(gameId, clients);

            return await _tcs.Task;
        }

        public async Task<bool> StartMainTimer(string gameId, int mainTimer, IHubClients clients)
        {
            StopAllTimers();

            lock (_lock)
            {
                _remainingTime = mainTimer;
                _wasStopped = false;
                _tcs = new TaskCompletionSource<bool>();
            }

            await clients.Group(gameId).SendAsync("TimerStarted", "MainTimer", _remainingTime);

            _mainTimer = new Timer(_ =>
            {
                lock (_lock)
                {
                    _tcs?.TrySetResult(_wasStopped);
                }
                StopAllTimers();
            }, null, TimeSpan.FromSeconds(mainTimer), Timeout.InfiniteTimeSpan);

            StartCountdown(gameId, clients);

            return await _tcs.Task;
        }

        public async Task<bool> StartStealTimer(string gameId, int stealTimer, IHubClients clients)
        {
            StopAllTimers();
            lock (_lock)
            {
                _remainingTime = stealTimer;
                _wasStopped = false;
                _tcs = new TaskCompletionSource<bool>();
            }
            await clients.Group(gameId).SendAsync("TimerStarted", "StealTimer", _remainingTime);


            _stealTimer = new Timer(_ =>
            {
                lock (_lock)
                {
                    _tcs?.TrySetResult(_wasStopped);
                }
                StopAllTimers();
            }, null, TimeSpan.FromSeconds(stealTimer), Timeout.InfiniteTimeSpan);

            StartCountdown(gameId, clients);

            return await _tcs.Task;
        }

        public async Task StopWordSelectTimer(string gameId, IHubClients clients)
        {
            lock (_lock)
            {
                if (_wordSelectTimer != null)
                {
                    _wasStopped = true;

                    _wordSelectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _wordSelectTimer.Dispose();
                    _wordSelectTimer = null;

                    _tcs?.TrySetResult(true);
                }
            }
            await clients.Group(gameId).SendAsync("TimerStopped", "WordSelectTimer");
            StopCountdown();
        }

        public async Task StopRunningTimer(string gameId, IHubClients clients)
        {
            lock (_lock)
            {
                if (_mainTimer != null)
                {
                    _wasStopped = true;
                    _mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _mainTimer.Dispose();
                    _mainTimer = null;
                }
                else if (_stealTimer != null)
                {
                    _wasStopped = true;
                    _stealTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _stealTimer.Dispose();
                    _stealTimer = null;
                    
                }
            }

            if (_wasStopped)
            {
                await clients.Group(gameId).SendAsync("TimerStopped", _mainTimer != null ? "MainTimer" : "StealTimer");
                _tcs?.TrySetResult(true);
            }

            StopCountdown();
        }

        private void StartCountdown(string gameId, IHubClients clients)
        {
            _countdownUpdateTimer = new Timer(async _ =>
            {
                _remainingTime--;
                if (_remainingTime > 0)
                {
                    await clients.Group(gameId).SendAsync("TimerUpdate", _remainingTime);
                }
                else
                {
                    StopCountdown();
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void StopCountdown()
        {
            _countdownUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _countdownUpdateTimer?.Dispose();
            _countdownUpdateTimer = null;
        }

        private void StopAllTimers()
        {
            _wasStopped = true;

            _wordSelectTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _mainTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _stealTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            _wordSelectTimer?.Dispose();
            _mainTimer?.Dispose();
            _stealTimer?.Dispose();

            _wordSelectTimer = null;
            _mainTimer = null;
            _stealTimer = null;

            StopCountdown();
        }
    }

}
