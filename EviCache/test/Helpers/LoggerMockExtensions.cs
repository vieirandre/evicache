using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;

namespace EviCache.Tests.Helpers;

public static class LoggerMockExtensions
{
    public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel logLevel, string expectedMessage, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    Regex.IsMatch(v.ToString(), expectedMessage.Replace("|", "\\|"), RegexOptions.None)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    public static void VerifyNoFailureLogsWereCalledInEviction(this Mock<ILogger> loggerMock)
    {
        loggerMock.VerifyLog(LogLevel.Error, "Eviction selector didn't return a candidate", Times.Never());
        loggerMock.VerifyLog(LogLevel.Error, "Eviction candidate (.*) wasn't found in the cache", Times.Never());
        loggerMock.VerifyLog(LogLevel.Warning, "Eviction failed. Item not added. Key: .*", Times.Never());
    }
}
