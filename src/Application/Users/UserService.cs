using Ardalis.Result;
using AutoMapper;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Users.Dtos;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Users;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Application.Users;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : BaseService, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        ILogger<UserService> logger,
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher passwordHasher)
        : base(logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> RegisterUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LogInformation("Starting user registration for email: {Email}", request.Email);

            // Check if user with the same email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                LogWarning("User with email {Email} already exists", request.Email);
                return Result.Conflict($"User with email '{request.Email}' already exists.");
            }

            // Hash the password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create new user
            var user = new User(
                email: request.Email,
                passwordHash: passwordHash,
                firstName: request.FirstName,
                lastName: request.LastName);

            // Set additional properties if needed
            user.UpdateInfo(request.FirstName, request.LastName);

            // Save user
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
            
            // Get the created user with all related data
            var createdUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);

            // Log information
            Logger.LogInformation("Successfully registered user with ID: {UserId}", createdUser.Id);

            // Map to DTO and return
            var result = _mapper.Map<UserDto>(createdUser);
            return Ardalis.Result.Result.Success(result);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error occurred while registering user with email: {Email}", request.Email);
            return Result.Error("An error occurred while registering the user. Please try again later.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LogInformation("Fetching user by ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                LogWarning("User with ID {UserId} not found", userId);
                return Result.NotFound($"User with ID '{userId}' not found.");
            }

            var result = _mapper.Map<UserDto>(user);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error occurred while fetching user with ID: {UserId}", userId);
            return Result.Error($"An error occurred while fetching user with ID '{userId}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Error("Email cannot be empty.");

            LogInformation("Fetching user by email: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                LogWarning("User with email {Email} not found", email);
                return Result.NotFound($"User with email '{email}' not found.");
            }

            var result = _mapper.Map<UserDto>(user);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error occurred while fetching user with email: {Email}", email);
            return Result.Error($"An error occurred while fetching user with email '{email}'.");
        }
    }
}
