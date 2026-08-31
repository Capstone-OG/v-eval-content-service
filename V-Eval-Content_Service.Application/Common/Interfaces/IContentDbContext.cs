using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Common.Interfaces
{
    public interface IContentDbContext
    {
        DbSet<CompetencyDomain> CompetencyDomains { get; }
        DbSet<Skill> Skills { get; }
        DbSet<Material> Materials { get; }
        DbSet<Passage> Passages { get; }
        DbSet<Question> Questions { get; }
        DbSet<MockExam> MockExams { get; }
        DbSet<ExamQuestion> ExamQuestions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
