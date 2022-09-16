using FluentValidation;




namespace AUTH_TEST1;

public class UserRequestValidator: AbstractValidator<UserRequest>
{

    public UserRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Password).Length(8,24);



    }


}
