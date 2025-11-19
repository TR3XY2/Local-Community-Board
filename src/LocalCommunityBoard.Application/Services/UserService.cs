// <copyright file="UserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services
{
    using LocalCommunityBoard.Application.Interfaces;
    using LocalCommunityBoard.Application.Security;
    using LocalCommunityBoard.Domain.Entities;
    using LocalCommunityBoard.Domain.Enums;
    using LocalCommunityBoard.Domain.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides business logic for user management (registration, login, etc.).
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<UserService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">Repository for user data access.</param>
        /// <param name="logger">Logger for tracking user-related actions.</param>
        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            this.userRepository = userRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Registers a new user with a securely hashed password.
        /// </summary>
        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            if (await this.userRepository.EmailExistsAsync(email))
            {
                this.logger.LogWarning("Registration failed: email '{Email}' already in use", email);
                throw new InvalidOperationException("Email already in use");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Password = PasswordHasher.HashPassword(password),
                RoleId = 1,
                Status = UserStatus.Active,
            };

            await this.userRepository.AddAsync(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation("New user registered successfully: {Username} ({Email})", username, email);
            return user;
        }

        /// <summary>
        /// Authenticates a user by verifying email and password.
        /// </summary>
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await this.userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                this.logger.LogWarning("Login failed: user with email '{Email}' not found", email);
                return null; // невірний email
            }

            var isValid = PasswordHasher.VerifyPassword(password, user.Password);
            if (!isValid)
            {
                this.logger.LogWarning("Login failed: incorrect password for user '{Email}'", email);
                return null; // неправильний пароль
            }

            // user повертається навіть якщо Status == Blocked
            this.logger.LogInformation("User attempted login: {Email}, Status: {Status}", email, user.Status);
            return user;
        }

        public async Task<User?> GetByIdAsync(int id) => await this.userRepository.GetByIdAsync(id);

        public async Task<bool> UpdateAsync(User user)
        {
            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await this.userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            this.userRepository.Delete(user);
            await this.userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!PasswordHasher.VerifyPassword(oldPassword, user.Password))
            {
                throw new UnauthorizedAccessException("Old password is incorrect");
            }

            user.Password = PasswordHasher.HashPassword(newPassword);
            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation("User {Email} changed password successfully", user.Email);
            return true;
        }

        public async Task<bool> UpdatePersonalInfoAsync(int userId, string? newUsername, string? newEmail)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
            {
                if (await this.userRepository.EmailExistsAsync(newEmail))
                {
                    throw new InvalidOperationException("Email is already in use by another account.");
                }

                this.logger.LogInformation("User {OldEmail} changed email to {NewEmail}", user.Email, newEmail);
                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != user.Username)
            {
                this.logger.LogInformation("User {Email} changed username from {OldUsername} to {NewUsername}", user.Email, user.Username, newUsername);
                user.Username = newUsername;
            }

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<User?> LogoutAsync(int userId)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            this.logger.LogInformation("User logged out: {Email}", user.Email);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var all = await this.userRepository.GetAllAsync();
            return all;
        }

        /// <summary>
        /// Allows an admin to update another user's information, including username, email, role, and password.
        /// </summary>
        public async Task<bool> AdminUpdateUserAsync(int userId, string? newUsername, string? newEmail, string? newPassword)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update username if provided
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != user.Username)
            {
                this.logger.LogInformation(
                    "Admin updating username for user {UserId} from {OldUsername} to {NewUsername}",
                    userId,
                    user.Username,
                    newUsername);
                user.Username = newUsername;
            }

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
            {
                if (await this.userRepository.EmailExistsAsync(newEmail))
                {
                    throw new InvalidOperationException("Email is already in use by another account.");
                }

                this.logger.LogInformation(
                    "Admin updating email for user {UserId} from {OldEmail} to {NewEmail}",
                    userId,
                    user.Email,
                    newEmail);
                user.Email = newEmail;
            }

            // Update password if provided (admin can change without old password)
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < 6)
                {
                    throw new ArgumentException("Password must be at least 6 characters long.");
                }

                user.Password = PasswordHasher.HashPassword(newPassword);
                this.logger.LogInformation("Admin updated password for user {UserId}", userId);
            }

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation("Admin successfully updated user {UserId}", userId);
            return true;
        }

        public async Task<bool> BlockUserAsync(int userId)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                this.logger.LogWarning("AdminBlockUserAsync: User {UserId} not found", userId);
                return false;
            }

            // Prevent blocking of admin accounts
            if (user.RoleId == 2)
            {
                this.logger.LogWarning(
                    "AdminBlockUserAsync: Attempted to block admin user {UserId} ({Username})",
                    userId,
                    user.Username);

                throw new InvalidOperationException("Cannot block administrator accounts.");
            }

            user.Status = UserStatus.Blocked;

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation(
                "AdminBlockUserAsync: User {UserId} ({Username}, {Email}) blocked successfully",
                userId,
                user.Username,
                user.Email);

            return true;
        }

        public async Task<bool> UnblockUserAsync(int userId)
        {
            // розблок = повернути статус Active
            var result = await this.userRepository.SetStatusAsync(userId, UserStatus.Active);

            if (result)
            {
                this.logger.LogInformation("User {UserId} unblocked by admin.", userId);
            }
            else
            {
                this.logger.LogWarning("Failed to unblock user {UserId}. Possibly not found.", userId);
            }

            return result;
        }

        public async Task<bool> DeleteUserByAdminAsync(int userId)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                this.logger.LogWarning("AdminDeleteUserAsync: User {UserId} not found", userId);
                return false;
            }

            // Prevent deletion of admin accounts
            if (user.RoleId == 2)
            {
                this.logger.LogWarning(
                    "AdminDeleteUserAsync: Attempted to delete admin user {UserId} ({Username})",
                    userId,
                    user.Username);
                throw new InvalidOperationException("Cannot delete administrator accounts.");
            }

            this.userRepository.Delete(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation(
                "AdminDeleteUserAsync: User {UserId} ({Username}, {Email}) deleted successfully",
                userId,
                user.Username,
                user.Email);

            return true;
        }

        /// <summary>
        /// Allows a user to delete their own account (self-deletion).
        /// </summary>
        public async Task<bool> DeleteOwnAccountAsync(int userId, string password)
        {
            var user = await this.userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                this.logger.LogWarning("DeleteOwnAccountAsync: User {UserId} not found", userId);
                return false;
            }

            // Заборона самовидалення адміністраторів
            if (user.RoleId == 2)
            {
                this.logger.LogWarning(
                    "DeleteOwnAccountAsync: Admin user {UserId} attempted self-deletion",
                    userId);
                throw new InvalidOperationException("Administrators cannot delete their own accounts.");
            }

            // Підтвердження пароля (користувач має підтвердити дію)
            var isValidPassword = PasswordHasher.VerifyPassword(password, user.Password);
            if (!isValidPassword)
            {
                this.logger.LogWarning(
                    "DeleteOwnAccountAsync: Incorrect password for user {UserId}",
                    userId);
                throw new UnauthorizedAccessException("Password is incorrect.");
            }

            this.userRepository.Delete(user);
            await this.userRepository.SaveChangesAsync();

            this.logger.LogInformation(
                "DeleteOwnAccountAsync: User {UserId} ({Email}) deleted their account",
                userId,
                user.Email);

            return true;
        }
    }
}
