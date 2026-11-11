using FluentValidation;

namespace V_Eval_Content_Service.Application.Materials.Queries.GetMaterialsBySkill;

public class GetMaterialsBySkillQueryValidator : AbstractValidator<GetMaterialsBySkillQuery>
{
    public GetMaterialsBySkillQueryValidator()
    {
        RuleFor(x => x.SkillId)
            .NotEmpty().WithMessage("SkillId không được để trống.");
    }
}
