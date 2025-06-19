using System;

namespace WitiQ.MessageBroker.Pulsar.Core.Helpers
{
    public static class DateTimeExtensions
    {
        // Unix Epoch (1 enero 1970 UTC)
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Convierte DateTime a Unix timestamp en milisegundos (ulong)
        /// </summary>
        public static ulong ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();

            var timeSpan = dateTime - UnixEpoch;
            return (ulong)timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// Convierte Unix timestamp en milisegundos (ulong) a DateTime
        /// </summary>
        public static DateTime FromUnixTimeMilliseconds(this ulong unixTimeMilliseconds)
        {
            return UnixEpoch.AddMilliseconds(unixTimeMilliseconds);
        }

        /// <summary>
        /// Convierte DateTime a Unix timestamp en segundos (ulong)
        /// </summary>
        public static ulong ToUnixTimeSeconds(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();

            var timeSpan = dateTime - UnixEpoch;
            return (ulong)timeSpan.TotalSeconds;
        }

        /// <summary>
        /// Convierte Unix timestamp en segundos (ulong) a DateTime
        /// </summary>
        public static DateTime FromUnixTimeSeconds(this ulong unixTimeSeconds)
        {
            return UnixEpoch.AddSeconds(unixTimeSeconds);
        }
    }
}
