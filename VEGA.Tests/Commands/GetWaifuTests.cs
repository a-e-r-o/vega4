using Moq;
using Xunit;
using SlashCommands;
using NetCord.Services.ApplicationCommands;
using System.Threading.Tasks;

namespace VEGA.Tests.Commands;

public class GetWaifuTests
{
    private readonly Mock<ApplicationCommandContext> _mockContext;

    public GetWaifuTests()
    {
        _mockContext = new Mock<ApplicationCommandContext>();
    }

    [Fact]
    public async Task Execute_ShouldReturnDiceRollResults()
    {
        // Arrange
        var command = new GetWaifu();
        var diceFaces = 6;
        var rollCount = 3;

        // Act
        await command.Execute(diceFaces, rollCount);

        // Assert
        // Add assertions to verify the response sent to the context
        // Example: Verify the interaction response
        _mockContext.Verify(c => c.Interaction.SendResponseAsync(It.IsAny<object>()), Times.Once);
    }
}