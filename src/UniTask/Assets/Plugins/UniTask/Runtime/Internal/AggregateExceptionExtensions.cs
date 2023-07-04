using System;

namespace Cysharp.Threading.Tasks.Internal
{
    internal static class AggregateExceptionExtensions
    {
        /// <summary>
        /// Unwraps an AggregateException to its first inner exception.
        /// </summary>
        public static Exception UnwrapToFirstInnerException(this AggregateException aggregateException)
        {
            if (aggregateException == null || aggregateException.InnerException == null)
                return null;

            var flattenedAggregate = aggregateException.Flatten();
            return flattenedAggregate?.InnerException;
        }
    }
}