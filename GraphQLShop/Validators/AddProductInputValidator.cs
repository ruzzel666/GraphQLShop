using FluentValidation;
using GraphQLShop.GraphQL.Inputs;

namespace GraphQLShop.Validators;

public class AddProductInputValidator : AbstractValidator<AddProductInput>
{
    public AddProductInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя не может быть пустым")
            .MinimumLength(3).WithMessage("Минимум 3 символа");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Цена должна быть больше нуля");
    }
}