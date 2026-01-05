using Xunit;

namespace AICodeReviewDemo.Controllers;

public class LegacyUserControllerTests
{
    private readonly LegacyUserController _controller;

    public LegacyUserControllerTests()
    {
        // ISSUE (test smell): Gerçek controller kullanılıyor, DB bağımlılığı var
        _controller = new LegacyUserController();
    }

    // AI, tüm "Edge Case"leri (sınır durumları) yakaladı ve biz biraz daha ekledik:

    [Theory]
    [InlineData(17, 0, 0, false, 0)]        // 18 yaş altı, her durumda 0 risk
    [InlineData(18, 10000, 0, false, 0)]    // Tam sınır: 18 yaş
    [InlineData(19, 4000, 0, false, 10)]    // Düşük gelir
    [InlineData(25, 5000, 0, false, 10)]    // Tam sınır: gelir = 5000
    [InlineData(25, 6000, 6000, false, 20)] // Yüksek borç
    [InlineData(25, 6000, 2000, false, 50)] // Orta borç
    [InlineData(30, 10000, 500, false, 80)] // Düşük borç, kartsız
    [InlineData(30, 10000, 500, true, 100)] // Düşük borç, kartlı
    [InlineData(40, 6000, 999, true, 100)]  // Tam sınır: debt = 999, kartlı
    [InlineData(40, 6000, 1000, true, 50)]  // Tam sınır: debt = 1000
    public void CalculateRisk_ReturnsCorrectScore(
        int age,
        int income,
        int debt,
        bool hasCard,
        int expectedScore)
    {
        // Act
        int result = _controller.CalculateRisk(age, income, debt, hasCard);

        // Assert
        Assert.Equal(expectedScore, result);
    }

    // Test smell: DB'ye bağlı test – AI muhtemelen mock / interface önerir
    [Fact(Skip = "Gerçek veritabanı yok, bu test gösterim amaçlıdır")]
    public void Login_Should_NotThrow_For_Any_Input()
    {
        // Arrange
        string username = "testuser";
        string password = "P@ssw0rd!";

        // Act
        var response = _controller.Login(username, password);

        // Assert
        Assert.NotNull(response);
    }

    // Test smell: duplication – aynı Arrange iki testte tekrar edilmiş
    [Fact]
    public void CalculateRisk_For_Teenagers_IsAlwaysZero()
    {
        // Arrange
        int score1 = _controller.CalculateRisk(17, 1000, 500, false);
        int score2 = _controller.CalculateRisk(16, 7000, 0, true);

        // Assert
        Assert.Equal(0, score1);
        Assert.Equal(0, score2);
    }

    [Fact]
    public void CalculateRisk_For_HighDebt_IsLowScore()
    {
        // Arrange
        int score1 = _controller.CalculateRisk(30, 10000, 6000, false);
        int score2 = _controller.CalculateRisk(45, 9000, 9999, true);

        // Assert
        Assert.Equal(20, score1);
        Assert.Equal(20, score2);
    }

    [Theory]
    [InlineData(25, 0, 0, false)]
    [InlineData(25, 5000, 9999, true)]
    public void CalculateRisk_LowIncome_AtOrBelowThreshold_Returns10(
        int age,
        int income,
        int debt,
        bool hasCard)
    {
        int result = _controller.CalculateRisk(age, income, debt, hasCard);
        Assert.Equal(10, result);
    }

    [Fact]
    public void CalculateRisk_DebtAt1000_IgnoresCard_Returns50()
    {
        int withCard = _controller.CalculateRisk(30, 6000, 1000, true);
        int withoutCard = _controller.CalculateRisk(30, 6000, 1000, false);

        Assert.Equal(50, withCard);
        Assert.Equal(withCard, withoutCard);
    }

    [Fact]
    public void CalculateRisk_DebtAt5000_IgnoresCard_Returns20()
    {
        int withCard = _controller.CalculateRisk(30, 6000, 5000, true);
        int withoutCard = _controller.CalculateRisk(30, 6000, 5000, false);

        Assert.Equal(20, withCard);
        Assert.Equal(withCard, withoutCard);
    }

    [Fact]
    public void CalculateRisk_HasCard_OnlyMatters_WhenDebtBelow1000()
    {
        int noCardBelow = _controller.CalculateRisk(40, 7000, 999, false);
        int cardBelow = _controller.CalculateRisk(40, 7000, 999, true);

        Assert.Equal(80, noCardBelow);
        Assert.Equal(100, cardBelow);

        int noCardAt = _controller.CalculateRisk(40, 7000, 1000, false);
        int cardAt = _controller.CalculateRisk(40, 7000, 1000, true);

        Assert.Equal(noCardAt, cardAt);
        Assert.Equal(50, noCardAt);
    }

    [Fact]
    public void CalculateRisk_ExtremeValues_DoNotOverflow_AndFollowLogic()
    {
        int resultHigh = _controller.CalculateRisk(int.MaxValue, int.MaxValue, int.MaxValue, true);
        Assert.Equal(20, resultHigh);

        int resultLowDebtNoCard = _controller.CalculateRisk(100, 1000000, -1, false);
        int resultLowDebtCard = _controller.CalculateRisk(100, 1000000, -1, true);

        Assert.Equal(80, resultLowDebtNoCard);
        Assert.Equal(100, resultLowDebtCard);
    }
}
