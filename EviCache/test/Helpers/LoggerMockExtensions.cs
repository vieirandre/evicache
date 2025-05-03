using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;

namespace EviCache.Tests.Helpers;

public static partial class LoggerMockExtensions
{
    [GeneratedRegex(@"[\|\(\)]")]
    private static partial Regex Pattern();

    public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel logLevel, string expectedMessage, Times times)
    {
        string escapedMessage = Pattern().Replace(expectedMessage, m => "\\" + m.Value);

        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    Regex.IsMatch(v.ToString(), escapedMessage, RegexOptions.None)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    public static void VerifyNoFailureLogsWereCalledInEviction(this Mock<ILogger> loggerMock)
    {
        loggerMock.VerifyLog(LogLevel.Error, "Eviction selector did not return a candidate", Times.Never());
        loggerMock.VerifyLog(LogLevel.Error, "Eviction candidate (.*) was not found in the cache", Times.Never());
    }
}
