using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Domain.Entities;
using V_Eval_Content_Service.Infrastructure.Persistence;

namespace V_Eval_Content_Service.Infrastructure.Persistence.Seeds;

public static class DiagnosticExamSeeder
{
    public static readonly Guid DiagnosticExamId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedDiagnosticExamAsync(ContentDbContext context, ILogger logger)
    {
        try
        {
            var existingDiagnostic = await context.MockExams
                .FirstOrDefaultAsync(e => e.ExamCategory == "DIAGNOSTIC" || e.ExamId == DiagnosticExamId);

            if (existingDiagnostic != null)
            {
                logger.LogInformation("Diagnostic Exam already seeded: {ExamId} ({Title})", existingDiagnostic.ExamId, existingDiagnostic.Title);
                return;
            }

            // Lấy 30 câu hỏi đầu tiên có sẵn trong ngân hàng câu hỏi
            var availableQuestions = await context.Questions
                .OrderBy(q => q.CreatedAt)
                .Take(30)
                .ToListAsync();

            if (availableQuestions.Count == 0)
            {
                logger.LogWarning("No questions found in database to seed Diagnostic Exam.");
                return;
            }

            var diagnosticExam = new MockExam
            {
                ExamId = DiagnosticExamId,
                Title = "Bài Khảo Sát Đánh Giá Năng Lực Đầu Vào (30 Câu Chẩn Đoán)",
                DurationMinutes = 45,
                TotalQuestions = availableQuestions.Count,
                IsPublished = true,
                ExamCategory = "DIAGNOSTIC",
                SubjectCode = "V-ACT",
                DifficultyLevel = 2,
                CreatedAt = DateTime.UtcNow
            };

            context.MockExams.Add(diagnosticExam);

            int order = 1;
            foreach (var question in availableQuestions)
            {
                diagnosticExam.ExamQuestions.Add(new ExamQuestion
                {
                    ExamId = diagnosticExam.ExamId,
                    QuestionId = question.QuestionId,
                    QuestionOrder = order++
                });
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded Diagnostic Exam with {Count} questions.", availableQuestions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while seeding Diagnostic Exam.");
        }
    }
}
