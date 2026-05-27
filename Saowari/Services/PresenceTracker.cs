using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Saowari.Services
{
    public class PresenceTracker
    {
        // Maps UserId to ConnectionCount
        private static readonly ConcurrentDictionary<int, int> OnlineUsers = new ConcurrentDictionary<int, int>();

        public void UserConnected(int userId)
        {
            OnlineUsers.AddOrUpdate(userId, 1, (key, count) => count + 1);
        }

        public void UserDisconnected(int userId)
        {
            if (OnlineUsers.TryGetValue(userId, out int count))
            {
                if (count <= 1)
                {
                    OnlineUsers.TryRemove(userId, out _);
                }
                else
                {
                    OnlineUsers.TryUpdate(userId, count - 1, count);
                }
            }
        }

        public bool IsUserOnline(int userId)
        {
            return OnlineUsers.ContainsKey(userId);
        }

        public List<int> GetOnlineUsers()
        {
            return OnlineUsers.Keys.ToList();
        }
    }
}
