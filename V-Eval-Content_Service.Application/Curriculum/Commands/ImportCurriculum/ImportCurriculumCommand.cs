using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.ImportCurriculum;

public record ImportCurriculumCommand(ImportCurriculumRequestDto Dto) : IRequest<Result<ImportCurriculumResponseDto>>;

public class ImportCurriculumCommandHandler : IRequestHandler<ImportCurriculumCommand, Result<ImportCurriculumResponseDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<ImportCurriculumCommandHandler> _logger;

    public ImportCurriculumCommandHandler(
        IContentDbContext context,
        ILogger<ImportCurriculumCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ImportCurriculumResponseDto>> Handle(
        ImportCurriculumCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.DomainId == Guid.Empty)
        {
            return Result<ImportCurriculumResponseDto>.Failure(
                Error.Validation("Curriculum.InvalidDomain", "DomainId không được để trống."));
        }

        try
        {
            _logger.LogInformation("Bắt đầu nạp tri thức phân cấp từ Giảng viên/AI cho Miền: {DomainName} ({DomainId})",
                dto.DomainName, dto.DomainId);

            // 1. Upsert Domain
            var domain = await _context.CompetencyDomains.FindAsync(new object[] { dto.DomainId }, cancellationToken);
            if (domain == null)
            {
                domain = new CompetencyDomain
                {
                    DomainId = dto.DomainId,
                    Name = !string.IsNullOrWhiteSpace(dto.DomainName) ? dto.DomainName : "Miền Năng Lực",
                    Description = dto.DomainDescription
                };
                await _context.CompetencyDomains.AddAsync(domain, cancellationToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.DomainName)) domain.Name = dto.DomainName;
                if (!string.IsNullOrWhiteSpace(dto.DomainDescription)) domain.Description = dto.DomainDescription;
            }
            await _context.SaveChangesAsync(cancellationToken);

            int chaptersCount = 0;
            int lessonsCount = 0;
            int patternsCount = 0;
            int materialsCount = 0;
            int questionsCount = 0;

            // 2. Lặp qua các Chương (Chapters)
            foreach (var chapDto in dto.Chapters ?? Enumerable.Empty<ImportChapterDto>())
            {
                var chapId = chapDto.ChapterId ?? Guid.NewGuid();
                string chapName = !string.IsNullOrWhiteSpace(chapDto.ChapterName) ? chapDto.ChapterName.Trim() : $"Chương {chaptersCount + 1}";
                var chapterSkill = await _context.Skills.FindAsync(new object[] { chapId }, cancellationToken);
                if (chapterSkill == null)
                {
                    chapterSkill = new Skill
                    {
                        SkillId = chapId,
                        DomainId = dto.DomainId,
                        Name = chapName,
                        ParentId = null,
                        Weight = chapDto.Weight ?? 0.25,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Skills.AddAsync(chapterSkill, cancellationToken);
                }
                else
                {
                    chapterSkill.Name = chapName;
                    chapterSkill.Weight = chapDto.Weight ?? chapterSkill.Weight;
                }
                chaptersCount++;

                // 3. Lặp qua các Bài học (Lessons)
                foreach (var lesDto in chapDto.Lessons ?? Enumerable.Empty<ImportLessonDto>())
                {
                    var lesId = lesDto.LessonId ?? Guid.NewGuid();
                    string lesName = !string.IsNullOrWhiteSpace(lesDto.LessonName) ? lesDto.LessonName.Trim() : $"Bài học {lessonsCount + 1}";
                    var lessonSkill = await _context.Skills.FindAsync(new object[] { lesId }, cancellationToken);
                    if (lessonSkill == null)
                    {
                        lessonSkill = new Skill
                        {
                            SkillId = lesId,
                            DomainId = dto.DomainId,
                            Name = lesName,
                            ParentId = chapId,
                            Weight = lesDto.Weight ?? 0.08,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Skills.AddAsync(lessonSkill, cancellationToken);
                    }
                    else
                    {
                        lessonSkill.Name = lesName;
                        lessonSkill.ParentId = chapId;
                        lessonSkill.Weight = lesDto.Weight ?? lessonSkill.Weight;
                    }
                    lessonsCount++;

                    // 4. Lặp qua các Dạng bài cụ thể (Patterns)
                    foreach (var patDto in lesDto.Patterns ?? Enumerable.Empty<ImportPatternDto>())
                    {
                        var patId = patDto.PatternId ?? Guid.NewGuid();
                        string patName = !string.IsNullOrWhiteSpace(patDto.PatternName) ? patDto.PatternName.Trim() : $"Dạng bài {patternsCount + 1}";
                        var patternSkill = await _context.Skills.FindAsync(new object[] { patId }, cancellationToken);
                        if (patternSkill == null)
                        {
                            patternSkill = new Skill
                            {
                                SkillId = patId,
                                DomainId = dto.DomainId,
                                Name = patName,
                                ParentId = lesId,
                                Weight = patDto.Weight ?? 0.04,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _context.Skills.AddAsync(patternSkill, cancellationToken);
                        }
                        else
                        {
                            patternSkill.Name = patName;
                            patternSkill.ParentId = lesId;
                            patternSkill.Weight = patDto.Weight ?? patternSkill.Weight;
                        }
                        patternsCount++;

                        // 5. Upsert Material cho Dạng bài (Khối 1 & 2 & 3)
                        var existingMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.SkillId == patId, cancellationToken);
                        string theory = !string.IsNullOrWhiteSpace(patDto.TheoryRecap) ? patDto.TheoryRecap.Trim() : "(Đang cập nhật lý thuyết SGK)";
                        string method = !string.IsNullOrWhiteSpace(patDto.SolutionMethod) ? patDto.SolutionMethod.Trim() : "(Đang cập nhật phương pháp giải)";
                        string formattedContent = $"### 1. Kiến thức nền tảng SGK GDPT 2018\n{theory}\n\n### 2. Phương pháp giải chi tiết từng bước\n{method}";

                        if (existingMaterial == null)
                        {
                            var mat = new Material
                            {
                                MaterialId = Guid.NewGuid(),
                                SkillId = patId,
                                Title = patName,
                                Content = formattedContent,
                                VideoUrl = patDto.VideoUrl,
                                DurationSeconds = patDto.DurationSeconds,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _context.Materials.AddAsync(mat, cancellationToken);
                        }
                        else
                        {
                            existingMaterial.Title = patName;
                            existingMaterial.Content = formattedContent;
                            if (!string.IsNullOrWhiteSpace(patDto.VideoUrl)) existingMaterial.VideoUrl = patDto.VideoUrl;
                            if (patDto.DurationSeconds > 0) existingMaterial.DurationSeconds = patDto.DurationSeconds;
                        }
                        materialsCount++;

                        // 6. Upsert Questions cho Dạng bài (Khối 4)
                        foreach (var qDto in patDto.Questions ?? Enumerable.Empty<ImportQuestionDto>())
                        {
                            var qId = qDto.QuestionId ?? Guid.NewGuid();
                            var existingQ = await _context.Questions.FindAsync(new object[] { qId }, cancellationToken);
                            char safeCorrect = char.ToUpperInvariant(qDto.CorrectOption);
                            if (safeCorrect < 'A' || safeCorrect > 'D') safeCorrect = 'A';

                            if (existingQ == null)
                            {
                                var q = new Question
                                {
                                    QuestionId = qId,
                                    SkillId = patId,
                                    DifficultyLevel = qDto.DifficultyLevel,
                                    ContentLatex = qDto.ContentLatex ?? string.Empty,
                                    OptionA = qDto.OptionA ?? string.Empty,
                                    OptionB = qDto.OptionB ?? string.Empty,
                                    OptionC = qDto.OptionC ?? string.Empty,
                                    OptionD = qDto.OptionD ?? string.Empty,
                                    CorrectOption = safeCorrect,
                                    Explanation = qDto.Explanation,
                                    CreatedAt = DateTime.UtcNow
                                };
                                await _context.Questions.AddAsync(q, cancellationToken);
                            }
                            else
                            {
                                existingQ.ContentLatex = qDto.ContentLatex ?? string.Empty;
                                existingQ.OptionA = qDto.OptionA ?? string.Empty;
                                existingQ.OptionB = qDto.OptionB ?? string.Empty;
                                existingQ.OptionC = qDto.OptionC ?? string.Empty;
                                existingQ.OptionD = qDto.OptionD ?? string.Empty;
                                existingQ.CorrectOption = safeCorrect;
                                existingQ.Explanation = qDto.Explanation;
                                existingQ.DifficultyLevel = qDto.DifficultyLevel;
                            }
                            questionsCount++;
                        }

                        // 7. Upsert SkillPrerequisites (DAG Tiên Quyết)
                        foreach (var prereqId in patDto.PrerequisiteSkillIds ?? Enumerable.Empty<Guid>())
                        {
                            var existsPrereq = await _context.SkillPrerequisites.AnyAsync(
                                sp => sp.SkillId == patId && sp.PrerequisiteId == prereqId, cancellationToken);

                            if (!existsPrereq)
                            {
                                await _context.SkillPrerequisites.AddAsync(new SkillPrerequisite
                                {
                                    SkillId = patId,
                                    PrerequisiteId = prereqId
                                }, cancellationToken);
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Nạp tri thức thành công: {Chap} chương, {Les} bài, {Pat} dạng, {Q} câu hỏi.",
                chaptersCount, lessonsCount, patternsCount, questionsCount);

            return Result<ImportCurriculumResponseDto>.Success(new ImportCurriculumResponseDto
            {
                ChaptersImported = chaptersCount,
                LessonsImported = lessonsCount,
                PatternsImported = patternsCount,
                MaterialsImported = materialsCount,
                QuestionsImported = questionsCount,
                Message = $"Đã nạp thành công cây tri thức cho miền {dto.DomainName}: {chaptersCount} chương, {lessonsCount} bài học, {patternsCount} dạng bài."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi nạp tri thức từ Giảng viên / AI.");
            return Result<ImportCurriculumResponseDto>.Failure(
                Error.Failure("Curriculum.ImportError", $"Lỗi khi nạp dữ liệu: {ex.Message}"));
        }
    }
}
