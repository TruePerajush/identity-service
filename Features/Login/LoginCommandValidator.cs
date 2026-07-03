using FluentValidation;

namespace IdentityService.Features.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty(); // Вот тут хз, мб добавить проверку длины
    }
}