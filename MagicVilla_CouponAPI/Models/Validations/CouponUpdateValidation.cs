using FluentValidation;

namespace MagicVilla_CouponAPI.Models.Validations;

public class CouponUpdatelidation : AbstractValidator<CouponUpdateDTO>
{
	public CouponUpdatelidation()
	{
		RuleFor(model => model.Id).NotEmpty().GreaterThan(0);
		RuleFor(model => model.Name).NotEmpty();
		RuleFor(model => model.Percent).InclusiveBetween(1, 100);
	}
}
