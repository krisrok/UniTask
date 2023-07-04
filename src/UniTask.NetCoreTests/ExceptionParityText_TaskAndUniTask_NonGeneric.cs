using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace NetCoreTests
{
    public class ExceptionParityText_TaskAndUniTask_NonGeneric
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
            catch(Exception thrownException)
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

        protected async Task ThrowingTask()
        {
            await Task.Yield();
            throw new TestException();
        }

        protected async UniTask ThrowingUniTask()
        {
            await UniTask.Yield();
            throw new TestException();
        }

        private class TestException : Exception
        { }
    }
}
