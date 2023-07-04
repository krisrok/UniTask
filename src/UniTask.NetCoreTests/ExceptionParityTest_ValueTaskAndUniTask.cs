using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace NetCoreTests
{
    public class ExceptionParityTest_ValueTaskAndUniTask
    {
        [Fact]
        public async Task Single()
        {
            await AssertEqualExceptionBehaviour(
                async () => await ThrowingValueTask(),
                async () => await ThrowingUniTask()
            );
        }

        private async Task AssertEqualExceptionBehaviour(Func<ValueTask> t, Func<UniTask> ut)
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

        protected async ValueTask ThrowingValueTask()
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
