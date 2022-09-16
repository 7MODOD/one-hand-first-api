using FluentValidation;


namespace AUTH_TEST1
{
    public class ArticlesValidator:AbstractValidator<ArticlesRequest>
    {

        public ArticlesValidator()
        {
            RuleFor(x => x.title).NotEmpty();
            RuleFor(x => x.desc).NotEmpty();
            RuleFor(x => x.body).NotEmpty();
            RuleFor(x => x.tagList).NotEmpty();
            



        }


    }
}
