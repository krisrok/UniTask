using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NetCoreTests
{
    public class ExceptionParityTest
    {
        [Fact]
        public async Task ThrownExceptionsMatch_Single()
        {
            await AssertThrownExceptionsMatch(() => ThrowingTask());
        }

        [Fact]
        public async Task ThrownExceptionsMatch_WhenAll()
        {
            await AssertThrownExceptionsMatch(() => Task.WhenAll(ThrowingTask(), ThrowingTask()));
        }

        [Fact]
        public async Task ThrownExceptionsMatch_WhenAny()
        {
            await AssertThrownExceptionsMatch(() => Task.WhenAny(ThrowingTask(), ThrowingTask()));
        }

        [Fact]
        public async Task ThrownExceptionsMatch_WhenAll_Nested()
        {
            await AssertThrownExceptionsMatch(() => Task.WhenAll(ThrowingTaskNested(), ThrowingTaskNested()));
        }

        [Fact]
        public async Task ThrownExceptionMatchesTaskException_Single()
        {
            await AssertUniTaskThrownExceptionMatchesTaskException(() => ThrowingTask());
        }

        [Fact]
        public async Task ThrownExceptionMatchesTaskException_WhenAll()
        {
            await AssertUniTaskThrownExceptionMatchesTaskException(() => Task.WhenAll(ThrowingTask(), ThrowingTask()));
        }

        [Fact]
        public async Task ThrownExceptionMatchesTaskException_WhenAny()
        {
            await AssertUniTaskThrownExceptionMatchesTaskException(() => Task.WhenAny(ThrowingTask(), ThrowingTask()));
        }

        [Fact]
        public async Task ThrownExceptionMatchesTaskException_WhenAll_Nested()
        {
            await AssertUniTaskThrownExceptionMatchesTaskException(() => Task.WhenAll(ThrowingTaskNested(), ThrowingTaskNested()));
        }

        /// <summary>
        /// Checks if UniTask's throw exception matches Task's thrown exception
        /// </summary>
        private static async Task AssertThrownExceptionsMatch(Func<Task> t)
        {
            var task = t();
            Exception ex = null;
            try
            {
                await task;
            }
            catch (Exception thrownException)
            {
                ex = thrownException;
            }

            await AssertTaskThrowsAndExceptionMatches(t, ex);
        }

        /// <summary>
        /// Checks if UniTask's throw exception matches the Task.Exception type
        /// </summary>
        private static async Task AssertUniTaskThrownExceptionMatchesTaskException(Func<Task> t)
        {
            var task = t();
            Exception ex = null;
            try
            {
                await task;
            }
            catch
            {
                ex = task.Exception;
            }

            await AssertTaskThrowsAndExceptionMatches(t, ex);
        }

        private static async Task AssertTaskThrowsAndExceptionMatches(Func<Task> t, Exception ex)
        {
            if(ex == null)
            {
                await AssertAsUniTaskDoesNotThrow(t);
                return;
            }

            await Assert.ThrowsAsync(ex.GetType(),
                async () => await t().AsUniTask());
        }

        private static async Task AssertAsUniTaskDoesNotThrow(Func<Task> t)
        {
            try
            {
                await t().AsUniTask();
            }
            catch(Exception ex)
            {
                throw new XunitException($"Expected: No exception thrown\nActual:   {ex}");
            }
        }

        private async Task ThrowingTask()
        {
            await Task.Delay(1);
            throw new TestException();
        }

        private async Task ThrowingTaskNested()
        {
            await Task.Delay(1);
            await Task.WhenAll(ThrowingTask(), ThrowingTask());
        }

        private class TestException : Exception
        { }
    }
}
