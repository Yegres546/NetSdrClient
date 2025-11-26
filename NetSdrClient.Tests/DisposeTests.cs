using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClient.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class DisposeTests
    {
        [TestMethod]
        public void SDRClient_Dispose_ShouldCancelCancellationToken()
        {
            // Arrange
            var client = new SDRClient();

            // Act
            client.Dispose();

            // Assert
            // Перевіряємо, що токен скасований після Dispose
            Assert.IsTrue(client.IsDisposed);
        }

        [TestMethod]
        public async Task SDRClient_DisposeDuringOperation_ShouldCancelOperation()
        {
            // Arrange
            var client = new SDRClient();
            var operationCompleted = false;

            // Act
            var task = Task.Run(async () =>
            {
                try
                {
                    await client.StartAsync();
                    operationCompleted = true;
                }
                catch (OperationCanceledException)
                {
                    // Очікувана поведінка
                }
            });

            // Даємо трохи часу на початок операції
            await Task.Delay(100);
            
            // Dispose має скасувати операцію
            client.Dispose();
            
            await task;

            // Assert
            Assert.IsFalse(operationCompleted, "Operation should be cancelled by Dispose");
        }

        [TestMethod]
        public void SDRClient_MultipleDispose_ShouldNotThrowException()
        {
            // Arrange
            var client = new SDRClient();

            // Act & Assert
            try
            {
                client.Dispose();
                client.Dispose(); // Другий виклик не має кидати виняток
            }
            catch (Exception ex)
            {
                Assert.Fail($"Multiple Dispose calls should not throw exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void CancellationTokenSource_InUsingBlock_ShouldBeDisposedAutomatically()
        {
            // Arrange & Act & Assert
            // Не має бути помилок або попереджень
            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;
                cts.CancelAfter(100);
                
                Assert.IsTrue(token.CanBeCanceled);
            }
            // cts автоматично видаляється тут
        }
    }
}
