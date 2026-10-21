using FluentValidation;

namespace V_Eval_Content_Service.Application.Questions.Commands.CreateQuestion;

public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.SkillId)
            .NotEmpty().WithMessage("Mã kỹ năng (SkillId) không được để trống.");

        RuleFor(x => x.DifficultyLevel)
            .InclusiveBetween(1, 6).WithMessage("Mức độ tư duy Bloom (DifficultyLevel) phải nằm trong khoảng 1 đến 6.");

        RuleFor(x => x.ContentLatex)
            .NotEmpty().WithMessage("Nội dung câu hỏi LaTeX không được để trống.")
            .MaximumLength(10000).WithMessage("Nội dung câu hỏi không được vượt quá 10,000 ký tự.");

        RuleFor(x => x.OptionA)
            .NotEmpty().WithMessage("Đáp án A không được để trống.");

        RuleFor(x => x.OptionB)
            .NotEmpty().WithMessage("Đáp án B không được để trống.");

        RuleFor(x => x.OptionC)
            .NotEmpty().WithMessage("Đáp án C không được để trống.");

        RuleFor(x => x.OptionD)
            .NotEmpty().WithMessage("Đáp án D không được để trống.");

        RuleFor(x => x.CorrectOption)
            .Must(c => c == 'A' || c == 'B' || c == 'C' || c == 'D')
            .WithMessage("Đáp án đúng (CorrectOption) phải là một trong bốn phương án: 'A', 'B', 'C', hoặc 'D'.");
    }
}
