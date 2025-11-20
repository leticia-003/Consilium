using Consilium.Domain.Models;

namespace Consilium.Tests.Models;

public class PhoneTests
{
    [Fact]
    public void Phone_DefaultConstructor_SetsDefaultValues()
    {
        // Arrange & Act
        var phone = new Phone();

        // Assert
        Assert.Equal(Guid.Empty, phone.ID);
        Assert.Equal(Guid.Empty, phone.UserID);
        Assert.Equal((short)351, phone.CountryCode);
        Assert.Equal(string.Empty, phone.Number);
        Assert.False(phone.IsMain);
        Assert.Null(phone.User);
    }

    [Fact]
    public void Phone_ID_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();
        var expectedId = Guid.NewGuid();

        // Act
        phone.ID = expectedId;

        // Assert
        Assert.Equal(expectedId, phone.ID);
    }

    [Fact]
    public void Phone_UserID_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();
        var expectedUserId = Guid.NewGuid();

        // Act
        phone.UserID = expectedUserId;

        // Assert
        Assert.Equal(expectedUserId, phone.UserID);
    }

    [Fact]
    public void Phone_CountryCode_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();
        short expectedCountryCode = 1;

        // Act
        phone.CountryCode = expectedCountryCode;

        // Assert
        Assert.Equal(expectedCountryCode, phone.CountryCode);
    }

    [Fact]
    public void Phone_CountryCode_DefaultsTo351()
    {
        // Arrange & Act
        var phone = new Phone();

        // Assert
        Assert.Equal((short)351, phone.CountryCode);
    }

    [Fact]
    public void Phone_Number_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();
        var expectedNumber = "912345678";

        // Act
        phone.Number = expectedNumber;

        // Assert
        Assert.Equal(expectedNumber, phone.Number);
    }

    [Fact]
    public void Phone_Number_DefaultsToEmptyString()
    {
        // Arrange & Act
        var phone = new Phone();

        // Assert
        Assert.Equal(string.Empty, phone.Number);
    }

    [Fact]
    public void Phone_IsMain_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();

        // Act
        phone.IsMain = true;

        // Assert
        Assert.True(phone.IsMain);
    }

    [Fact]
    public void Phone_IsMain_DefaultsToFalse()
    {
        // Arrange & Act
        var phone = new Phone();

        // Assert
        Assert.False(phone.IsMain);
    }

    [Fact]
    public void Phone_User_CanBeSetAndRetrieved()
    {
        // Arrange
        var phone = new Phone();
        var user = new User
        {
            ID = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            NIF = "123456789",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        // Act
        phone.User = user;

        // Assert
        Assert.NotNull(phone.User);
        Assert.Equal(user.ID, phone.User.ID);
        Assert.Equal(user.Name, phone.User.Name);
        Assert.Equal(user.Email, phone.User.Email);
    }

    [Fact]
    public void Phone_InitializeWithValues_AllPropertiesSetCorrectly()
    {
        // Arrange
        var phoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        short countryCode = 55;
        string number = "11987654321";
        bool isMain = true;
        var user = new User
        {
            ID = userId,
            Name = "John Doe",
            Email = "john@example.com",
            NIF = "987654321",
            PasswordHash = "hash123",
            IsActive = true
        };

        // Act
        var phone = new Phone
        {
            ID = phoneId,
            UserID = userId,
            CountryCode = countryCode,
            Number = number,
            IsMain = isMain,
            User = user
        };

        // Assert
        Assert.Equal(phoneId, phone.ID);
        Assert.Equal(userId, phone.UserID);
        Assert.Equal(countryCode, phone.CountryCode);
        Assert.Equal(number, phone.Number);
        Assert.Equal(isMain, phone.IsMain);
        Assert.NotNull(phone.User);
        Assert.Equal(user, phone.User);
    }

    [Fact]
    public void Phone_CountryCode_AcceptsVariousValidValues()
    {
        // Arrange
        var phone = new Phone();
        short[] countryCodes = { 1, 44, 55, 351, 32767 }; // Including max value for short

        foreach (var code in countryCodes)
        {
            // Act
            phone.CountryCode = code;

            // Assert
            Assert.Equal(code, phone.CountryCode);
        }
    }

    [Fact]
    public void Phone_Number_AcceptsNullAndEmptyStrings()
    {
        // Arrange
        var phone = new Phone();

        // Act & Assert - Empty string
        phone.Number = string.Empty;
        Assert.Equal(string.Empty, phone.Number);

        // Act & Assert - Non-empty string
        phone.Number = "123456789";
        Assert.Equal("123456789", phone.Number);

        // Act & Assert - Null (if allowed by nullable reference types configuration)
        phone.Number = null!;
        Assert.Null(phone.Number);
    }

    [Fact]
    public void Phone_UserRelationship_UserIDMatchesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phone = new Phone
        {
            UserID = userId,
            User = new User
            {
                ID = userId,
                Name = "Test User",
                Email = "test@test.com",
                NIF = "123456789",
                IsActive = true
            }
        };

        // Assert
        Assert.Equal(phone.UserID, phone.User.ID);
    }

    [Fact]
    public void Phone_MultipleInstances_AreIndependent()
    {
        // Arrange
        var phone1 = new Phone
        {
            ID = Guid.NewGuid(),
            Number = "111111111",
            IsMain = true
        };

        var phone2 = new Phone
        {
            ID = Guid.NewGuid(),
            Number = "222222222",
            IsMain = false
        };

        // Assert
        Assert.NotEqual(phone1.ID, phone2.ID);
        Assert.NotEqual(phone1.Number, phone2.Number);
        Assert.NotEqual(phone1.IsMain, phone2.IsMain);
    }
}
