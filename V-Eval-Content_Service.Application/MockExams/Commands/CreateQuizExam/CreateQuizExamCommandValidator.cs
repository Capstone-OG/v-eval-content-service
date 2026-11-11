using FluentValidation;

namespace V_Eval_Content_Service.Application.MockExams.Commands.CreateQuizExam;

public class CreateQuizExamCommandValidator : AbstractValidator<CreateQuizExamCommand>
{
    public CreateQuizExamCommandValidator()
    {
        RuleFor(x => x.SkillIds)
            .NotEmpty().WithMessage("Danh sách mã kỹ năng (SkillIds) không được để trống.");

        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(1, 25).WithMessage("Số lượng câu hỏi đóng gói (QuestionCount) phải từ 1 đến 25 câu.");

        RuleFor(x => x.MinBloomLevel)
            .InclusiveBetween(1, 6).WithMessage("Mức độ Bloom tối thiểu (MinBloomLevel) phải từ 1 đến 6.");

        RuleFor(x => x.MaxBloomLevel)
            .InclusiveBetween(1, 6).WithMessage("Mức độ Bloom tối đa (MaxBloomLevel) phải từ 1 đến 6.")
            .GreaterThanOrEqualTo(x => x.MinBloomLevel).WithMessage("Mức độ Bloom tối đa phải lớn hơn hoặc bằng mức tối thiểu.");
    }
}
