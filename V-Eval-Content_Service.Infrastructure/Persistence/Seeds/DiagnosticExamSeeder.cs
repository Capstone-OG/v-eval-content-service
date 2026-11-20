using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Domain.Entities;
using V_Eval_Content_Service.Infrastructure.Persistence;

namespace V_Eval_Content_Service.Infrastructure.Persistence.Seeds;

public static class DiagnosticExamSeeder
{
    public static readonly Guid LegacyDiagnosticExamId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly (Guid ExamId, string Title)[] BalancedDiagnosticSuites =
    [
        (Guid.Parse("22222222-2222-2222-2222-000000000001"), "Đề Chẩn Đoán Năng Lực ĐGNL Số 1 (30 Câu — Chuẩn 9 Môn GDPT 2018)"),
        (Guid.Parse("22222222-2222-2222-2222-000000000002"), "Đề Chẩn Đoán Năng Lực ĐGNL Số 2 (30 Câu — Chuẩn 9 Môn GDPT 2018)"),
        (Guid.Parse("22222222-2222-2222-2222-000000000003"), "Đề Chẩn Đoán Năng Lực ĐGNL Số 3 (30 Câu — Chuẩn 9 Môn GDPT 2018)")
    ];

    // Exact V-ACT 30-question blueprint covering ALL 4 Domains & ALL 9 GDPT 2018 Subjects:
    // DOM_LANG (10): Ngữ văn 5 + Tiếng Anh 5
    // DOM_MATH (8): Toán học 8
    // DOM_NAT_SCI (6): Vật lí 2 + Hóa học 2 + Sinh học 2
    // DOM_SOC_SCI (6): Lịch sử 2 + Địa lí 2 + GDKT & Pháp luật 2
    private static readonly (string Subject, int Quota)[] SubjectBlueprint =
    [
        ("Ngữ văn", 5),
        ("Tiếng Anh", 5),
        ("Toán học", 8),
        ("Vật lí", 2),
        ("Hóa học", 2),
        ("Sinh học", 2),
        ("Lịch sử", 2),
        ("Địa lí", 2),
        ("GDKT & Pháp luật", 2)
    ];

    public static async Task SeedDiagnosticExamAsync(ContentDbContext context, ILogger logger)
    {
        try
        {
            var suiteIds = BalancedDiagnosticSuites.Select(s => s.ExamId).ToHashSet();
            var existingSuiteIds = await context.MockExams
                .AsNoTracking()
                .Where(e => suiteIds.Contains(e.ExamId))
                .Select(e => e.ExamId)
                .ToHashSetAsync();

            if (existingSuiteIds.Count == BalancedDiagnosticSuites.Length)
            {
                await DemoteLegacyDiagnosticIfNeededAsync(context, logger);
                logger.LogInformation("All {Count} balanced GDPT 2018 Diagnostic Exams already seeded.", BalancedDiagnosticSuites.Length);
                return;
            }

            // Load full curriculum tree to bind diagnostic questions strictly to real GDPT 2018 Patterns (ParentId != null)
            var allSkills = await context.Skills.AsNoTracking().Include(s => s.Domain).ToListAsync();
            var skillMap = allSkills.ToDictionary(s => s.SkillId);

            string ResolveSubject(Skill skill)
            {
                string chapterName = string.Empty;
                if (skill.ParentId.HasValue && skillMap.TryGetValue(skill.ParentId.Value, out var parent))
                {
                    if (parent.ParentId.HasValue && skillMap.TryGetValue(parent.ParentId.Value, out var gp))
                        chapterName = gp.Name;
                    else
                        chapterName = parent.Name;
                }

                var text = $"{chapterName} {skill.Name}".ToLowerInvariant();
                if (text.Contains("(toán") || text.Contains("toán học")) return "Toán học";
                if (text.Contains("(tiếng anh") || text.Contains("tiếng anh") || text.Contains("english")) return "Tiếng Anh";
                if (text.Contains("(ngữ văn") || text.Contains("ngữ văn") || text.Contains("tiếng việt")) return "Ngữ văn";
                if (text.Contains("(vật lí") || text.Contains("(vật lý") || text.Contains("vật lí") || text.Contains("vật lý")) return "Vật lí";
                if (text.Contains("(hóa học") || text.Contains("(hoá học") || text.Contains("hóa học") || text.Contains("hoá học")) return "Hóa học";
                if (text.Contains("(sinh học") || text.Contains("sinh học")) return "Sinh học";
                if (text.Contains("(lịch sử") || text.Contains("lịch sử")) return "Lịch sử";
                if (text.Contains("(địa lí") || text.Contains("(địa lý") || text.Contains("địa lí") || text.Contains("địa lý")) return "Địa lí";
                if (text.Contains("gdkt") || text.Contains("kinh tế và pháp luật") || text.Contains("pháp luật")) return "GDKT & Pháp luật";
                return "OTHER";
            }

            // Only pick real hierarchical curriculum questions (ParentId != null -> linked to real Chapter -> Lesson -> Pattern)
            var rawCurriculumQuestions = await context.Questions
                .AsNoTracking()
                .Include(q => q.Skill)
                .Include(q => q.Passage)
                .Where(q => q.Skill.ParentId != null && !string.IsNullOrEmpty(q.ContentLatex))
                .OrderBy(q => q.DifficultyLevel)
                .ThenBy(q => q.SkillId)
                .ThenBy(q => q.QuestionId)
                .ToListAsync();

            var curriculumQuestions = rawCurriculumQuestions
                .Where(q => !IsBrokenOrphanPassageQuestion(q))
                .ToList();

            if (curriculumQuestions.Count < 30)
            {
                logger.LogWarning("Not enough curriculum questions ({Count}) to seed balanced GDPT 2018 Diagnostic Exams.", curriculumQuestions.Count);
                return;
            }

            // Group by Subject, preserving distinct skills & Bloom diversity (interleave Bloom levels 1..4)
            var poolsBySubject = curriculumQuestions
                .GroupBy(q => ResolveSubject(q.Skill))
                .ToDictionary(
                    g => g.Key,
                    g => InterleaveByDifficultyAndSkill(g.ToList()));

            var globalUsedQuestionIds = new HashSet<Guid>();
            var baseTimestamp = DateTime.UtcNow;

            for (int suiteIndex = 0; suiteIndex < BalancedDiagnosticSuites.Length; suiteIndex++)
            {
                var (examId, title) = BalancedDiagnosticSuites[suiteIndex];
                if (existingSuiteIds.Contains(examId)) continue;

                var selected = new List<Question>();
                var selectedIds = new HashSet<Guid>();

                foreach (var (subject, quota) in SubjectBlueprint)
                {
                    if (!poolsBySubject.TryGetValue(subject, out var pool) || pool.Count == 0) continue;

                    int taken = 0;
                    // First pass: prefer questions not used in previous suites & distinct skills
                    var usedSkillsInExam = new HashSet<Guid>();
                    foreach (var q in pool)
                    {
                        if (taken >= quota) break;
                        if (globalUsedQuestionIds.Contains(q.QuestionId) || selectedIds.Contains(q.QuestionId)) continue;
                        if (!usedSkillsInExam.Add(q.SkillId)) continue;
                        selected.Add(q);
                        selectedIds.Add(q.QuestionId);
                        globalUsedQuestionIds.Add(q.QuestionId);
                        taken++;
                    }

                    // Second pass: allow unused questions from same subject even if skill repeats
                    foreach (var q in pool)
                    {
                        if (taken >= quota) break;
                        if (globalUsedQuestionIds.Contains(q.QuestionId) || selectedIds.Contains(q.QuestionId)) continue;
                        selected.Add(q);
                        selectedIds.Add(q.QuestionId);
                        globalUsedQuestionIds.Add(q.QuestionId);
                        taken++;
                    }

                    // Third pass (if subject pool < 3 * quota): wrap around safely without duplicating within the same exam
                    foreach (var q in pool)
                    {
                        if (taken >= quota) break;
                        if (selectedIds.Contains(q.QuestionId)) continue;
                        selected.Add(q);
                        selectedIds.Add(q.QuestionId);
                        taken++;
                    }
                }

                // Fill any remainder up to 30 from curriculumQuestions
                foreach (var q in curriculumQuestions)
                {
                    if (selected.Count >= 30) break;
                    if (selectedIds.Add(q.QuestionId))
                    {
                        selected.Add(q);
                    }
                }

                if (selected.Count < 10) continue;

                var exam = new MockExam
                {
                    ExamId = examId,
                    Title = title,
                    DurationMinutes = 45,
                    TotalQuestions = selected.Count,
                    IsPublished = true,
                    ExamCategory = "DIAGNOSTIC",
                    SubjectCode = "V-ACT",
                    DifficultyLevel = 2,
                    // Suite 1 gets newest timestamp so first-time students start with Đề Số 1
                    CreatedAt = baseTimestamp.AddSeconds(BalancedDiagnosticSuites.Length - suiteIndex)
                };

                context.MockExams.Add(exam);

                int order = 1;
                foreach (var q in selected)
                {
                    exam.ExamQuestions.Add(new ExamQuestion
                    {
                        ExamId = examId,
                        QuestionId = q.QuestionId,
                        QuestionOrder = order++
                    });
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Seeded balanced GDPT 2018 Diagnostic Exam {ExamId} ({Title}) with {Count} real curriculum questions.",
                    examId, title, selected.Count);
            }

            await DemoteLegacyDiagnosticIfNeededAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while seeding balanced GDPT 2018 Diagnostic Exams.");
        }
    }

    private static async Task DemoteLegacyDiagnosticIfNeededAsync(ContentDbContext context, ILogger logger)
    {
        var legacy = await context.MockExams
            .FirstOrDefaultAsync(e => e.ExamId == LegacyDiagnosticExamId && e.ExamCategory == "DIAGNOSTIC");

        if (legacy != null)
        {
            legacy.ExamCategory = "DIAGNOSTIC_LEGACY";
            await context.SaveChangesAsync();
            logger.LogInformation("Demoted legacy unbalanced Diagnostic Exam {ExamId} to DIAGNOSTIC_LEGACY (history preserved).", LegacyDiagnosticExamId);
        }
    }

    private static List<Question> InterleaveByDifficultyAndSkill(List<Question> pool)
    {
        // Round-robin across Bloom levels (1, 2, 3, 4+) so every exam gets a true IRT difficulty spread
        var buckets = pool
            .GroupBy(q => Math.Clamp(q.DifficultyLevel, 1, 4))
            .OrderBy(g => g.Key)
            .Select(g => new Queue<Question>(g))
            .ToList();

        var result = new List<Question>(pool.Count);
        bool progress = true;
        while (progress)
        {
            progress = false;
            foreach (var bucket in buckets)
            {
                if (bucket.Count > 0)
                {
                    result.Add(bucket.Dequeue());
                    progress = true;
                }
            }
        }
        return result;
    }

    private static bool IsBrokenOrphanPassageQuestion(Question q)
    {
        if (q.Passage != null && !string.IsNullOrWhiteSpace(q.Passage.Content))
        {
            return false;
        }

        var text = (q.ContentLatex ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text)) return false;

        return text.Contains("of the passage")
            || text.Contains("in the passage")
            || text.Contains("from the passage")
            || text.Contains("according to the passage")
            || text.Contains("the passage mainly")
            || text.Contains("title of the passage")
            || text.Contains("in paragraph")
            || text.Contains("đoạn văn trên")
            || text.Contains("đoạn trích trên")
            || text.Contains("theo đoạn văn")
            || text.Contains("dựa vào đoạn văn")
            || text.Contains("văn bản trên")
            || text.Contains("ngữ liệu trên");
    }
}
