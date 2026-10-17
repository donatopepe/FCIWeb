using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace FCIWeb.Web.Security
{
    /// <summary>Configurazione dei limiti di tentativi login.</summary>
    public sealed class LoginAbuseOptions
    {
        public int MaxFailures { get; set; } = 5;
        public TimeSpan FailureWindow { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Limiter in-process: conta i fallimenti per coppia account+IP, usa compare-and-update
    /// atomico e limita le chiavi conservate per ridurre abuso memoria.
    /// </summary>
    public sealed class LoginAbuseGuard
    {
        private const int MaxTrackedKeys = 10000;
        private readonly ConcurrentDictionary<string, FailureState> _states = new ConcurrentDictionary<string, FailureState>();
        private readonly LoginAbuseOptions _options;
        private readonly TimeProvider _timeProvider;

        private sealed class FailureState
        {
            public int Failures;
            public DateTimeOffset WindowStarted;
            public DateTimeOffset? LockedUntil;
        }

        public LoginAbuseGuard(IOptions<LoginAbuseOptions> options, TimeProvider timeProvider)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _timeProvider = timeProvider ?? TimeProvider.System;
            if (_options.MaxFailures < 1) throw new ArgumentOutOfRangeException(nameof(options), "MaxFailures must be positive.");
            if (_options.FailureWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options), "FailureWindow must be positive.");
            if (_options.LockoutDuration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options), "LockoutDuration must be positive.");
        }

        public bool IsLocked(string account, string remoteAddress)
        {
            var key = MakeKey(account, remoteAddress);
            if (!_states.TryGetValue(key, out var state)) return false;
            var now = _timeProvider.GetUtcNow();
            if (state.LockedUntil.HasValue && state.LockedUntil.Value > now) return true;
            if (state.LockedUntil.HasValue || now - state.WindowStarted >= _options.FailureWindow)
                _states.TryRemove(key, out _);
            return false;
        }

        public void RecordFailure(string account, string remoteAddress)
        {
            var key = MakeKey(account, remoteAddress);
            var now = _timeProvider.GetUtcNow();
            if (!_states.TryGetValue(key, out var state))
            {
                if (_states.Count >= MaxTrackedKeys) Prune(now);
                if (_states.Count >= MaxTrackedKeys) return; // fail closed for resource use; ASP.NET request limits remain active
                var created = new FailureState { Failures = 1, WindowStarted = now };
                _states.TryAdd(key, created);
                state = _states.GetOrAdd(key, created);
            }

            lock (state)
            {
                if (state.LockedUntil.HasValue && state.LockedUntil.Value > now) return;
                if (now - state.WindowStarted >= _options.FailureWindow)
                {
                    state.Failures = 0;
                    state.WindowStarted = now;
                    state.LockedUntil = null;
                }
                state.Failures++;
                if (state.Failures >= _options.MaxFailures)
                    state.LockedUntil = now.Add(_options.LockoutDuration);
            }
        }

        public void RecordSuccess(string account, string remoteAddress)
            => _states.TryRemove(MakeKey(account, remoteAddress), out _);

        private void Prune(DateTimeOffset now)
        {
            foreach (var pair in _states)
            {
                lock (pair.Value)
                {
                    if ((!pair.Value.LockedUntil.HasValue || pair.Value.LockedUntil.Value <= now) &&
                        now - pair.Value.WindowStarted >= _options.FailureWindow)
                        _states.TryRemove(pair.Key, out _);
                }
            }
        }

        private static string MakeKey(string account, string remoteAddress)
        {
            var normalized = (account ?? String.Empty).Trim().ToUpperInvariant() + "\n" + (remoteAddress ?? "unknown");
            var digest = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(digest);
        }
    }
}
