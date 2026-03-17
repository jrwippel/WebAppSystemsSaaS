using System.Collections.Concurrent;

namespace ClockTrack.Services
{
    /// <summary>
    /// Serviço para controlar tentativas de login e prevenir ataques de força bruta
    /// </summary>
    public class LoginAttemptService
    {
        private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
        private const int MaxAttempts = 5;
        private const int LockoutMinutes = 15;

        public class LoginAttemptInfo
        {
            public int FailedAttempts { get; set; }
            public DateTime? LockoutUntil { get; set; }
            public DateTime LastAttempt { get; set; }
        }

        /// <summary>
        /// Verifica se o login está bloqueado
        /// </summary>
        public bool IsLockedOut(string login)
        {
            if (!_attempts.TryGetValue(login.ToLower(), out var info))
                return false;

            if (info.LockoutUntil.HasValue && info.LockoutUntil.Value > DateTime.UtcNow)
                return true;

            // Se o bloqueio expirou, limpa
            if (info.LockoutUntil.HasValue && info.LockoutUntil.Value <= DateTime.UtcNow)
            {
                _attempts.TryRemove(login.ToLower(), out _);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Retorna o tempo restante de bloqueio
        /// </summary>
        public TimeSpan? GetLockoutTimeRemaining(string login)
        {
            if (!_attempts.TryGetValue(login.ToLower(), out var info))
                return null;

            if (info.LockoutUntil.HasValue && info.LockoutUntil.Value > DateTime.UtcNow)
                return info.LockoutUntil.Value - DateTime.UtcNow;

            return null;
        }

        /// <summary>
        /// Registra uma tentativa de login falhada
        /// </summary>
        public void RecordFailedAttempt(string login)
        {
            var key = login.ToLower();
            var info = _attempts.GetOrAdd(key, _ => new LoginAttemptInfo());

            info.FailedAttempts++;
            info.LastAttempt = DateTime.UtcNow;

            if (info.FailedAttempts >= MaxAttempts)
            {
                info.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            }
        }

        /// <summary>
        /// Limpa as tentativas após login bem-sucedido
        /// </summary>
        public void ResetAttempts(string login)
        {
            _attempts.TryRemove(login.ToLower(), out _);
        }

        /// <summary>
        /// Retorna o número de tentativas restantes
        /// </summary>
        public int GetRemainingAttempts(string login)
        {
            if (!_attempts.TryGetValue(login.ToLower(), out var info))
                return MaxAttempts;

            return Math.Max(0, MaxAttempts - info.FailedAttempts);
        }

        /// <summary>
        /// Limpa tentativas antigas (executar periodicamente)
        /// </summary>
        public void CleanupOldAttempts()
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            var keysToRemove = _attempts
                .Where(kvp => kvp.Value.LastAttempt < cutoff && 
                             (!kvp.Value.LockoutUntil.HasValue || kvp.Value.LockoutUntil.Value < DateTime.UtcNow))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _attempts.TryRemove(key, out _);
            }
        }
    }
}
