using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace NetCoreTests
{
    public static class TaskExt
    {
        /// <summary>
        /// A workaround for getting all of AggregateException.InnerExceptions with try/await/catch
        /// </summary>
        // from https://stackoverflow.com/a/62607500
        public static Task WithAggregatedExceptions(this Task @this)
        {
            // using AggregateException.Flatten as a bonus
            return @this.ContinueWith(
                continuationFunction: anteTask =>
                    anteTask.IsFaulted &&
                    anteTask.Exception is AggregateException ex &&
                    (ex.InnerExceptions.Count > 1 || ex.InnerException is AggregateException) ?
                    Task.FromException(ex.Flatten()) : anteTask,
                cancellationToken: CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler: TaskScheduler.Default).Unwrap();
        }
    }

    public class ExceptionParityTest_TaskAndUniTask_Generic
    {
        [Fact]
        public async Task Single()
        {
            await AssertEqualExceptionBehaviour(
                async () => await ThrowingTask(),
                async () => await ThrowingUniTask()
            );
        }

        [Fact]
        public async Task WhenAll_SingleTask()
        {
            await AssertEqualExceptionBehaviour(
                async () => await Task.WhenAll(ThrowingTask()),
                async () => await UniTask.WhenAll(ThrowingUniTask())
            );
        }

        [Fact]
        public async Task WhenAll_MultipleTasks()
        {
            await AssertEqualExceptionBehaviour(
                async () => await Task.WhenAll(ThrowingTask(), ThrowingTask()),
                async () => await UniTask.WhenAll(ThrowingUniTask(), ThrowingUniTask())
            );
        }

        [Fact]
        public async Task WhenAll_Nested()
        {
            await AssertEqualExceptionBehaviour(
                async () => await ThrowingTaskNested(),
                async () => await ThrowingUniTaskNested()
            );
        }

        [Fact]
        public async Task WhenAny()
        {
            await AssertEqualExceptionBehaviour(
                async () => await Task.WhenAny(ThrowingTask(), ThrowingTask()),
                async () => await UniTask.WhenAny(ThrowingUniTask(), ThrowingUniTask())
            );
        }

        private async Task AssertEqualExceptionBehaviour(Func<Task> t, Func<UniTask> ut)
        {
            Exception taskEx = null;
            try
            {
                await t();
            }
            catch (Exception thrownException)
            {
                taskEx = thrownException;
            }

            Exception uniTaskEx = null;
            try
            {
                await ut();
            }
            catch (Exception thrownException)
            {
                uniTaskEx = thrownException;
            }

            if (taskEx == null && uniTaskEx != null)
            {
                throw new XunitException($"UniTask is not expected to throw an exception.\nExpected: null\nActual:   {uniTaskEx}");
            }

            Assert.IsType(taskEx.GetType(), uniTaskEx);
        }

        private async Task<bool> ThrowingTask()
        {
            await Task.Yield();
            throw new TestException();
        }

        private async UniTask<bool> ThrowingUniTask()
        {
            await UniTask.Yield();
            throw new TestException();
        }

        private async Task ThrowingTaskNested()
        {
            await Task.Yield();
            await Task.WhenAll(ThrowingTask(), ThrowingTask());
        }

        private async UniTask ThrowingUniTaskNested()
        {
            await UniTask.Yield();
            await UniTask.WhenAll(ThrowingUniTask(), ThrowingUniTask());
        }

        private class TestException : Exception
        { }
    }
}
