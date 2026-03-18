using FluentValidation;

public class UserValidator : AbstractValidator<CreateUserCommand>
{
    public UserValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(command => command.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must be 3-20 characters long.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");
    }
}
