using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Users;
using CryptoWallet.Application.Users.Dtos;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Models.DTOs.Wallets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CryptoWallet.UnitTests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _loggerMock = new Mock<ILogger<UserService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _userService = new UserService(
            _loggerMock.Object,
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenUserDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var user = new User(
            request.Email,
            "hashed_password",
            request.FirstName,
            request.LastName);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber = null,
            CountryCode = null,
            IsEmailConfirmed = false,
            CreatedAt = user.CreatedAt,
            LastLoginAt = null,
            IsActive = true,
            Wallets = new List<WalletDto>()
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        _passwordHasherMock
            .Setup(x => x.HashPassword(request.Password))
            .Returns("hashed_password");

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _userService.RegisterUserAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(request.Email);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u => u.Email == request.Email), It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenUserExists_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Existing",
            LastName = "User"
        };

        var existingUser = new User(
            request.Email,
            "existing_hash",
            request.FirstName,
            request.LastName);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.RegisterUserAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain($"User with email '{request.Email}' already exists.");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(
            "test@example.com",
            "hashed_password",
            "Test",
            "User");
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber = null,
            CountryCode = null,
            IsEmailConfirmed = false,
            CreatedAt = user.CreatedAt,
            LastLoginAt = null,
            IsActive = true,
            Wallets = new List<WalletDto>()
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"User with ID '{userId}' not found.");
    }
}
