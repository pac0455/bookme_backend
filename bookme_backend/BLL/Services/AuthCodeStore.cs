namespace bookme_backend.BLL.Services
{
    using System.Collections.Concurrent;

    public class AuthCodeStore
    {
        private readonly ConcurrentDictionary<string, (string Code, DateTime Expiration)> _codes = new();

        public void SaveCode(string userId, string code, TimeSpan ttl)
        {
            var expiration = DateTime.UtcNow.Add(ttl);
            _codes[userId] = (code, expiration);
        }

        public bool VerifyCode(string userId, string inputCode)
        {
            if (_codes.TryGetValue(userId, out var stored) &&
                stored.Expiration > DateTime.UtcNow &&
                stored.Code == inputCode)
            {
                _codes.TryRemove(userId, out _);
                return true;
            }

            return false;
        }
    }   
}
