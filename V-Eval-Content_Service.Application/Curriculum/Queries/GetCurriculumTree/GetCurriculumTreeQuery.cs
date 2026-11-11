using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Queries.GetCurriculumTree;

public record GetCurriculumTreeQuery : IRequest<Result<List<CurriculumDomainDto>>>;

public class GetCurriculumTreeQueryHandler : IRequestHandler<GetCurriculumTreeQuery, Result<List<CurriculumDomainDto>>>
{
    private readonly IContentDbContext _context;

    public GetCurriculumTreeQueryHandler(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CurriculumDomainDto>>> Handle(
        GetCurriculumTreeQuery request,
        CancellationToken cancellationToken)
    {
        // Truy vấn 100% từ Database: CompetencyDomains kèm Skills, Materials, Questions, Prerequisites
        var domains = await _context.CompetencyDomains
            .AsNoTracking()
            .Include(d => d.Skills)
                .ThenInclude(s => s.Prerequisites)
            .Include(d => d.Skills)
                .ThenInclude(s => s.Materials)
            .Include(d => d.Skills)
                .ThenInclude(s => s.Questions)
            .OrderBy(d => d.DomainId)
            .ToListAsync(cancellationToken);

        var resultList = new List<CurriculumDomainDto>();

        foreach (var d in domains)
        {
            var (icon, gradient, activeBg) = ResolveDomainVisuals(d.Name);

            var domainDto = new CurriculumDomainDto
            {
                Id = $"domain-{d.DomainId}",
                DomainId = d.DomainId.ToString(),
                DomainCode = ResolveDomainCode(d.DomainId, d.Name),
                Title = d.Name,
                Subtitle = d.Description ?? "Chuyên đề khung năng lực",
                Icon = icon,
                Gradient = gradient,
                ActiveBg = activeBg,
                Chapters = new List<CurriculumChapterDto>(),
                Skills = new List<CurriculumSkillDto>()
            };

            var allSkills = d.Skills.OrderBy(s => s.CreatedAt).ThenBy(s => s.Name).ToList();

            // 1. Cây 3 cấp động: Chương (Skills có ParentId == null và có con hoặc tên Chương)
            var chapters = allSkills
                .Where(s => s.ParentId == null && (s.Name.StartsWith("Chương", StringComparison.OrdinalIgnoreCase) || allSkills.Any(c => c.ParentId == s.SkillId)))
                .ToList();

            if (chapters.Count > 0)
            {
                int chapOrder = 1;
                foreach (var chap in chapters)
                {
                    var chapterDto = new CurriculumChapterDto
                    {
                        ChapterId = chap.SkillId.ToString(),
                        Name = chap.Name,
                        Code = $"CH_{chapOrder++:D2}",
                        Description = chap.Materials.FirstOrDefault()?.Content ?? $"Hệ thống kiến thức: {chap.Name}",
                        Weight = chap.Weight ?? 0.25,
                        Lessons = new List<CurriculumLessonDto>()
                    };

                    var lessons = allSkills.Where(s => s.ParentId == chap.SkillId).ToList();
                    int lesOrder = 1;
                    foreach (var les in lessons)
                    {
                        var lessonDto = new CurriculumLessonDto
                        {
                            LessonId = les.SkillId.ToString(),
                            Name = les.Name,
                            Code = $"{chapterDto.Code}_L{lesOrder++:D2}",
                            Summary = les.Materials.FirstOrDefault()?.Content ?? $"Bài học: {les.Name}",
                            Weight = les.Weight ?? 0.08,
                            Patterns = new List<CurriculumPatternDto>()
                        };

                        var patterns = allSkills.Where(s => s.ParentId == les.SkillId).ToList();
                        int patOrder = 1;
                        foreach (var pat in patterns)
                        {
                            lessonDto.Patterns.Add(BuildPatternFromEntity(pat, $"{lessonDto.Code}_P{patOrder++:D2}"));
                        }

                        // Nếu bài học là lá (chưa có dạng bài con), lấy chính nó làm dạng bài
                        if (lessonDto.Patterns.Count == 0)
                        {
                            lessonDto.Patterns.Add(BuildPatternFromEntity(les, $"{lessonDto.Code}_P01"));
                        }

                        chapterDto.Lessons.Add(lessonDto);
                    }

                    domainDto.Chapters.Add(chapterDto);
                }
            }

            // 2. Kỹ năng phẳng (chưa lồng cây): Gom thành chuyên đề chuẩn để đảm bảo cấu trúc đồng nhất
            var flatSkills = allSkills.Where(s => s.ParentId == null && !chapters.Contains(s)).ToList();
            if (flatSkills.Count > 0)
            {
                var defaultChap = new CurriculumChapterDto
                {
                    ChapterId = $"chap-general-{d.DomainId}",
                    Name = $"Khung Năng Lực Chuẩn - {d.Name}",
                    Code = $"{domainDto.DomainCode}_CORE",
                    Description = d.Description ?? "Danh mục kỹ năng trọng tâm",
                    Weight = 1.0,
                    Lessons = new List<CurriculumLessonDto>()
                };

                int lesIdx = 1;
                foreach (var s in flatSkills)
                {
                    var lessonDto = new CurriculumLessonDto
                    {
                        LessonId = s.SkillId.ToString(),
                        Name = s.Name,
                        Code = $"{defaultChap.Code}_L{lesIdx++:D2}",
                        Summary = s.Materials.FirstOrDefault()?.Content ?? s.Name,
                        Weight = s.Weight ?? 0.08,
                        Patterns = new List<CurriculumPatternDto>
                        {
                            BuildPatternFromEntity(s, $"{defaultChap.Code}_P{lesIdx:D2}")
                        }
                    };
                    defaultChap.Lessons.Add(lessonDto);
                }

                domainDto.Chapters.Add(defaultChap);
            }

            // 3. Danh sách phẳng Skills (tương thích ngược)
            foreach (var s in allSkills)
            {
                var mat = s.Materials.FirstOrDefault();
                var q = s.Questions.FirstOrDefault();

                domainDto.Skills.Add(new CurriculumSkillDto
                {
                    SkillId = s.SkillId.ToString(),
                    Name = s.Name,
                    Code = $"SKILL_{s.SkillId.ToString()[..4].ToUpperInvariant()}",
                    Weight = $"{Math.Round((s.Weight ?? 0.08) * 100)}%",
                    Level = q != null ? $"Bloom L{q.DifficultyLevel}" : "Chuẩn",
                    CrossModel = mat?.Title ?? "",
                    CrossNote = mat?.Content ?? "",
                    Theory = mat?.Content ?? "Kiến thức chuẩn trong cơ sở dữ liệu.",
                    KeyFormulas = ExtractFormulasFromContent(mat?.Content),
                    PrerequisiteIds = s.Prerequisites.Select(p => p.PrerequisiteId.ToString()).ToList(),
                    VideoLesson = mat != null ? new CurriculumVideoLessonDto
                    {
                        Title = mat.Title,
                        Teacher = "Ban Chuyên Môn",
                        Duration = $"{Math.Max(10, mat.DurationSeconds / 60)} phút",
                        DurationSeconds = mat.DurationSeconds,
                        Views = "Cơ sở dữ liệu",
                        VideoUrl = mat.VideoUrl
                    } : null,
                    SampleQuestion = q != null ? new CurriculumSampleQuestionDto
                    {
                        QuestionId = q.QuestionId.ToString(),
                        Content = q.ContentLatex,
                        Options = new List<string> { $"A. {q.OptionA}", $"B. {q.OptionB}", $"C. {q.OptionC}", $"D. {q.OptionD}" },
                        Correct = q.CorrectOption switch { 'A' => 0, 'B' => 1, 'C' => 2, 'D' => 3, _ => 0 },
                        Explanation = q.Explanation ?? ""
                    } : null
                });
            }

            resultList.Add(domainDto);
        }

        return Result<List<CurriculumDomainDto>>.Success(resultList);
    }

    private static CurriculumPatternDto BuildPatternFromEntity(Skill s, string code)
    {
        var mat = s.Materials.FirstOrDefault();
        var q = s.Questions.FirstOrDefault();

        string theory = "Kiến thức nền tảng trong cơ sở dữ liệu.";
        string method = "Phương pháp giải chi tiết trong cơ sở dữ liệu.";

        if (mat != null && !string.IsNullOrWhiteSpace(mat.Content))
        {
            var content = mat.Content;
            if (content.Contains("### 2."))
            {
                var parts = Regex.Split(content, @"###\s*2\.\s*");
                if (parts.Length > 1)
                {
                    theory = parts[0].Trim();
                    method = parts[1].Trim();
                }
                else
                {
                    theory = content;
                }
            }
            else
            {
                theory = content;
            }

            // Dọn dẹp tiêu đề markdown để giảng viên và học sinh xem/hiệu đính tinh gọn
            if (theory.StartsWith("### 1."))
            {
                var newlineIdx = theory.IndexOf('\n');
                if (newlineIdx >= 0) theory = theory.Substring(newlineIdx + 1).Trim();
            }
            if (method.StartsWith("Phương pháp giải"))
            {
                var newlineIdx = method.IndexOf('\n');
                if (newlineIdx >= 0) method = method.Substring(newlineIdx + 1).Trim();
            }
        }

        return new CurriculumPatternDto
        {
            PatternId = s.SkillId.ToString(),
            Name = s.Name,
            Code = code,
            Level = q != null ? $"Bloom L{q.DifficultyLevel}" : "Vận dụng",
            Weight = $"{Math.Round((s.Weight ?? 0.05) * 100)}%",
            TheoryRecap = theory,
            SolutionMethod = method,
            KeyFormulas = ExtractFormulasFromContent(mat?.Content),
            CrossModel = mat?.Title ?? "",
            CrossNote = "",
            PrerequisiteIds = s.Prerequisites.Select(p => p.PrerequisiteId.ToString()).ToList(),
            VideoLesson = mat != null ? new CurriculumVideoLessonDto
            {
                Title = mat.Title,
                Teacher = "Ban Chuyên Môn",
                Duration = $"{Math.Max(10, mat.DurationSeconds / 60)} phút",
                DurationSeconds = mat.DurationSeconds,
                Views = "Cơ sở dữ liệu",
                VideoUrl = mat.VideoUrl
            } : null,
            SampleQuestion = q != null ? new CurriculumSampleQuestionDto
            {
                QuestionId = q.QuestionId.ToString(),
                Content = q.ContentLatex,
                Options = new List<string> { $"A. {q.OptionA}", $"B. {q.OptionB}", $"C. {q.OptionC}", $"D. {q.OptionD}" },
                Correct = q.CorrectOption switch { 'A' => 0, 'B' => 1, 'C' => 2, 'D' => 3, _ => 0 },
                Explanation = q.Explanation ?? ""
            } : null,
            QuestionBankCount = s.Questions.Count
        };
    }

    private static List<CurriculumFormulaDto> ExtractFormulasFromContent(string? content)
    {
        var list = new List<CurriculumFormulaDto>();
        if (string.IsNullOrWhiteSpace(content)) return list;

        // Trích xuất công thức LaTeX $$...$$ hoặc $...$ từ content nếu có
        var matches = Regex.Matches(content, @"\$\$(.+?)\$\$");
        int count = 1;
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                list.Add(new CurriculumFormulaDto
                {
                    Label = $"Công thức {count++}",
                    Latex = match.Groups[1].Value.Trim()
                });
            }
        }
        return list;
    }

    private static string ResolveDomainCode(Guid domainId, string domainName)
    {
        string name = domainName.ToLowerInvariant();
        if (name.Contains("toán") || name.Contains("logic")) return "MATH_LOGIC";
        if (name.Contains("ngôn ngữ") || name.Contains("tiếng")) return "LANGUAGE";
        if (name.Contains("tự nhiên")) return "NATURAL_SCIENCES";
        if (name.Contains("xã hội")) return "SOCIAL_SCIENCES";
        return "GENERAL";
    }

    private static (string Icon, string Gradient, string ActiveBg) ResolveDomainVisuals(string domainName)
    {
        string name = domainName.ToLowerInvariant();
        if (name.Contains("toán") || name.Contains("logic"))
        {
            return ("Cpu", "from-blue-600 via-indigo-600 to-cyan-500", "bg-blue-50 border-blue-200 text-blue-700");
        }
        if (name.Contains("ngôn ngữ") || name.Contains("tiếng"))
        {
            return ("BookOpen", "from-emerald-600 via-teal-600 to-green-500", "bg-emerald-50 border-emerald-200 text-emerald-700");
        }
        if (name.Contains("tự nhiên"))
        {
            return ("Atom", "from-amber-600 via-orange-600 to-rose-500", "bg-amber-50 border-amber-200 text-amber-700");
        }
        if (name.Contains("xã hội"))
        {
            return ("BarChart2", "from-violet-600 via-purple-600 to-pink-500", "bg-purple-50 border-purple-200 text-purple-700");
        }

        return ("Layers", "from-slate-700 to-slate-900", "bg-slate-50 border-slate-200 text-slate-700");
    }
}
