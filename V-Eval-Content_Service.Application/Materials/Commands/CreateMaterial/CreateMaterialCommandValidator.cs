using System;
using FluentValidation;

namespace V_Eval_Content_Service.Application.Materials.Commands.CreateMaterial;

public class CreateMaterialCommandValidator : AbstractValidator<CreateMaterialCommand>
{
    public CreateMaterialCommandValidator()
    {
        RuleFor(x => x.SkillId)
            .NotEmpty().WithMessage("SkillId không được để trống.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài giảng không được để trống.")
            .MaximumLength(255).WithMessage("Tiêu đề bài giảng không được vượt quá 255 ký tự.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung tóm tắt lý thuyết không được để trống.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Thời lượng bài giảng (DurationSeconds) phải lớn hơn hoặc bằng 0.");

        When(x => !string.IsNullOrWhiteSpace(x.VideoUrl), () =>
        {
            RuleFor(x => x.VideoUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                             (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                .WithMessage("VideoUrl phải là một đường link URL hợp lệ (bắt đầu bằng http hoặc https).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.FileUrl), () =>
        {
            RuleFor(x => x.FileUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                             (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                .WithMessage("FileUrl phải là một đường link URL hợp lệ (bắt đầu bằng http hoặc https).");
        });
    }
}
