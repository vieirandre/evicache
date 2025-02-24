using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;

namespace EviCache.Tests.Utils;

public static class LoggerMockExtensions
{
    public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel logLevel, string expectedMessage, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    Regex.IsMatch(v.ToString(), expectedMessage, RegexOptions.None)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
