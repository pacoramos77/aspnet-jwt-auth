using Auth.Controllers;
using Auth.IdentityAuth;
using Auth.Models;

namespace test;

public class WeatherForecastControllerTest
{
    [Fact]
    public void Get_Returns_WeatherForecasts()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WeatherForecastController>>();
        var controller = new WeatherForecastController(mockLogger.Object);

        // Act
        var result = controller.Get();

        // Assert
        Assert.Equal(5, result.Count());
    }
}