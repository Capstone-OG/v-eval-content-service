using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.UpdatePattern;

public record UpdatePatternCommand(Guid PatternId, UpdateCurriculumPatternDto Dto) : IRequest<Result<CurriculumPatternDto>>;

public class UpdatePatternCommandHandler : IRequestHandler<UpdatePatternCommand, Result<CurriculumPatternDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<UpdatePatternCommandHandler> _logger;

    public UpdatePatternCommandHandler(
        IContentDbContext context,
        ILogger<UpdatePatternCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<CurriculumPatternDto>> Handle(
        UpdatePatternCommand request,
        CancellationToken cancellationToken)
    {
        var patternId = request.PatternId;
        var dto = request.Dto;

        if (patternId == Guid.Empty)
        {
            return Result<CurriculumPatternDto>.Failure(
                Error.Validation("Curriculum.InvalidPatternId", "Mã dạng bài (PatternId) không hợp lệ."));
        }

        var skill = await _context.Skills
            .Include(s => s.Materials)
            .Include(s => s.Questions)
            .Include(s => s.Prerequisites)
            .FirstOrDefaultAsync(s => s.SkillId == patternId, cancellationToken);

        if (skill == null)
        {
            return Result<CurriculumPatternDto>.Failure(
                Error.NotFound("Curriculum.PatternNotFound", $"Không tìm thấy dạng bài có ID: {patternId}"));
        }

        try
        {
            _logger.LogInformation("Giảng viên tiến hành hiệu đính Dạng bài: {SkillName} ({SkillId})", skill.Name, skill.SkillId);

            // 1. Cập nhật thông tin Kỹ năng / Dạng bài
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                skill.Name = dto.Name.Trim();
            }
            if (dto.Weight.HasValue)
            {
                skill.Weight = dto.Weight.Value;
            }

            // 2. Cập nhật Tài liệu sư phạm (Khối 1: Nền tảng SGK, Khối 2: Phương pháp giải, Khối 3: Video)
            string formattedContent = $"### 1. Kiến thức nền tảng SGK GDPT 2018\n{dto.TheoryRecap?.Trim()}\n\n### 2. Phương pháp giải chi tiết từng bước\n{dto.SolutionMethod?.Trim()}";

            var material = skill.Materials.FirstOrDefault();
            if (material == null)
            {
                material = new Material
                {
                    MaterialId = Guid.NewGuid(),
                    SkillId = skill.SkillId,
                    Title = skill.Name,
                    Content = formattedContent,
                    VideoUrl = dto.VideoUrl,
                    DurationSeconds = dto.DurationSeconds,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Materials.AddAsync(material, cancellationToken);
            }
            else
            {
                material.Title = skill.Name;
                material.Content = formattedContent;
                material.VideoUrl = dto.VideoUrl;
                material.DurationSeconds = dto.DurationSeconds;
            }

            // 3. Cập nhật Đồ thị quan hệ tiên quyết (DAG Prerequisites)
            if (dto.PrerequisiteSkillIds != null)
            {
                var existingPrereqs = await _context.SkillPrerequisites
                    .Where(p => p.SkillId == skill.SkillId)
                    .ToListAsync(cancellationToken);

                _context.SkillPrerequisites.RemoveRange(existingPrereqs);

                foreach (var prereqId in dto.PrerequisiteSkillIds.Where(id => id != Guid.Empty && id != skill.SkillId).Distinct())
                {
                    await _context.SkillPrerequisites.AddAsync(new SkillPrerequisite
                    {
                        SkillId = skill.SkillId,
                        PrerequisiteId = prereqId
                    }, cancellationToken);
                }
            }

            // 4. Cập nhật Câu hỏi mẫu minh họa (Khối 4)
            if (dto.SampleQuestion != null && !string.IsNullOrWhiteSpace(dto.SampleQuestion.ContentLatex))
            {
                var sampleDto = dto.SampleQuestion;
                Question? targetQuestion = null;

                if (sampleDto.QuestionId.HasValue && sampleDto.QuestionId.Value != Guid.Empty)
                {
                    targetQuestion = skill.Questions.FirstOrDefault(q => q.QuestionId == sampleDto.QuestionId.Value);
                }

                if (targetQuestion == null)
                {
                    targetQuestion = skill.Questions.FirstOrDefault();
                }

                if (targetQuestion != null)
                {
                    targetQuestion.DifficultyLevel = sampleDto.DifficultyLevel;
                    targetQuestion.ContentLatex = sampleDto.ContentLatex;
                    targetQuestion.OptionA = sampleDto.OptionA;
                    targetQuestion.OptionB = sampleDto.OptionB;
                    targetQuestion.OptionC = sampleDto.OptionC;
                    targetQuestion.OptionD = sampleDto.OptionD;
                    targetQuestion.CorrectOption = sampleDto.CorrectOption;
                    targetQuestion.Explanation = sampleDto.Explanation;
                }
                else
                {
                    var newQ = new Question
                    {
                        QuestionId = sampleDto.QuestionId ?? Guid.NewGuid(),
                        SkillId = skill.SkillId,
                        DifficultyLevel = sampleDto.DifficultyLevel,
                        ContentLatex = sampleDto.ContentLatex,
                        OptionA = sampleDto.OptionA,
                        OptionB = sampleDto.OptionB,
                        OptionC = sampleDto.OptionC,
                        OptionD = sampleDto.OptionD,
                        CorrectOption = sampleDto.CorrectOption,
                        Explanation = sampleDto.Explanation,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Questions.AddAsync(newQ, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 5. Chuẩn hóa DTO phản hồi trả về
            var updatedPattern = new CurriculumPatternDto
            {
                PatternId = skill.SkillId.ToString(),
                Name = skill.Name,
                Code = $"PAT_{skill.SkillId.ToString()[..4].ToUpperInvariant()}",
                Level = "Vận dụng",
                Weight = $"{Math.Round((skill.Weight ?? 0.05) * 100)}%",
                TheoryRecap = dto.TheoryRecap ?? "",
                SolutionMethod = dto.SolutionMethod ?? "",
                KeyFormulas = new List<CurriculumFormulaDto>(),
                CrossModel = material.Title,
                CrossNote = "",
                PrerequisiteIds = dto.PrerequisiteSkillIds?.Select(id => id.ToString()).ToList() ?? skill.Prerequisites.Select(p => p.PrerequisiteId.ToString()).ToList(),
                VideoLesson = new CurriculumVideoLessonDto
                {
                    Title = material.Title,
                    Teacher = "Ban Chuyên Môn / Giảng Viên",
                    Duration = $"{Math.Max(5, material.DurationSeconds / 60)} phút",
                    DurationSeconds = material.DurationSeconds,
                    Views = "Cơ sở dữ liệu",
                    VideoUrl = material.VideoUrl
                },
                SampleQuestion = skill.Questions.FirstOrDefault() != null ? new CurriculumSampleQuestionDto
                {
                    QuestionId = skill.Questions.First().QuestionId.ToString(),
                    Content = skill.Questions.First().ContentLatex,
                    Options = new List<string>
                    {
                        $"A. {skill.Questions.First().OptionA}",
                        $"B. {skill.Questions.First().OptionB}",
                        $"C. {skill.Questions.First().OptionC}",
                        $"D. {skill.Questions.First().OptionD}"
                    },
                    Correct = skill.Questions.First().CorrectOption switch { 'A' => 0, 'B' => 1, 'C' => 2, 'D' => 3, _ => 0 },
                    Explanation = skill.Questions.First().Explanation ?? ""
                } : null,
                QuestionBankCount = skill.Questions.Count
            };

            return Result<CurriculumPatternDto>.Success(updatedPattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi giảng viên cập nhật dạng bài {PatternId}", patternId);
            return Result<CurriculumPatternDto>.Failure(
                Error.Failure("Curriculum.UpdateError", $"Không thể lưu thay đổi dạng bài: {ex.Message}"));
        }
    }
}
