using FluentValidation;


namespace AUTH_TEST1;


public class ProfileRequestValidator : AbstractValidator<string>
{
    public ProfileRequestValidator()
    {
        RuleFor(x => x).NotEmpty();
        RuleFor(x => x).Length(5,20);

    }



}