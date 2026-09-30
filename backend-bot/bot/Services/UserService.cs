using System.Collections.Generic;

namespace Autouchet_Bot.Services
{
    public static class UserService
    {
        private static readonly HashSet<long> _acceptedUsers = new HashSet<long>();

        public static bool HasAccepted(long userId)
        {
            lock (_acceptedUsers)
            {
                return _acceptedUsers.Contains(userId);
            }
        }

        public static void Accept(long userId)
        {
            lock (_acceptedUsers)
            {
                _acceptedUsers.Add(userId);
            }
        }
    }
}