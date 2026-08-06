using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Users;

public class UserService(
    IAppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserInput> createValidator,
    IValidator<UpdateUserInput> updateValidator) : IUserService
{
    public async Task<Result<UserDto>> CreateAsync(CreateUserInput input, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<UserDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();

        var emailInUse = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailInUse)
        {
            return Result.Failure<UserDto>("A user with this email already exists.");
        }

        var passwordHash = passwordHasher.Hash(input.Password);

        var userResult = User.Create(input.Email, passwordHash, input.FullName, input.Role, DateTime.UtcNow);
        if (userResult.IsFailure)
        {
            return Result.Failure<UserDto>(userResult.Error!);
        }

        dbContext.Users.Add(userResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(userResult.Value.ToDto());
    }

    public async Task<Result<UserDto>> UpdateAsync(UpdateUserInput input, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<UserDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == input.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>("User not found.");
        }

        user.UpdateProfile(input.FullName);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }

    public async Task<Result> SetActiveStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        if (isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null
            ? Result.Failure<UserDto>("User not found.")
            : Result.Success(user.ToDto());
    }

    public async Task<Result<PagedResult<UserDto>>> GetListAsync(int page, int pageSize, UserRole? role, CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking().OrderBy(u => u.FullName).AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var paged = await query
            .Select(u => u)
            .ToPagedResultAsync(page, pageSize, cancellationToken);

        var dtoPage = new PagedResult<UserDto>(
            paged.Items.Select(u => u.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
