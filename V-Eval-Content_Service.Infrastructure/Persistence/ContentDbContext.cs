using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Domain.Entities;
using V_Eval_Content_Service.Application.Common.Interfaces;

namespace V_Eval_Content_Service.Infrastructure.Persistence
{
    public class ContentDbContext : DbContext, IContentDbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
        {
        }

        public DbSet<CompetencyDomain> CompetencyDomains { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<Passage> Passages { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<MockExam> MockExams { get; set; } = null!;
        public DbSet<ExamQuestion> ExamQuestions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập Default Schema là "content"
            modelBuilder.HasDefaultSchema("content");

            // Cấu hình CompetencyDomain
            modelBuilder.Entity<CompetencyDomain>(entity =>
            {
                entity.ToTable("CompetencyDomains");
                entity.HasKey(e => e.DomainId);
                entity.Property(e => e.DomainId).HasColumnName("domain_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
            });

            // Cấu hình Skill
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.ToTable("Skills");
                entity.HasKey(e => e.SkillId);
                entity.Property(e => e.SkillId).HasColumnName("skill_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.DomainId).HasColumnName("domain_id").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
                entity.Property(e => e.Weight).HasColumnName("weight");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(d => d.Domain)
                    .WithMany(p => p.Skills)
                    .HasForeignKey(d => d.DomainId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Material
            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Materials");
                entity.HasKey(e => e.MaterialId);
                entity.Property(e => e.MaterialId).HasColumnName("material_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.SkillId).HasColumnName("skill_id").IsRequired();
                entity.Property(e => e.Title).HasColumnName("title").IsRequired();
                entity.Property(e => e.Content).HasColumnName("content").IsRequired();
                entity.Property(e => e.VideoUrl).HasColumnName("video_url");
                entity.Property(e => e.FileUrl).HasColumnName("file_url");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(d => d.Skill)
                    .WithMany(p => p.Materials)
                    .HasForeignKey(d => d.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Passage
            modelBuilder.Entity<Passage>(entity =>
            {
                entity.ToTable("Passages");
                entity.HasKey(e => e.PassageId);
                entity.Property(e => e.PassageId).HasColumnName("passage_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Content).HasColumnName("content").IsRequired();
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            });

            // Cấu hình Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Questions");
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.QuestionId).HasColumnName("question_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.SkillId).HasColumnName("skill_id").IsRequired();
                entity.Property(e => e.PassageId).HasColumnName("passage_id");
                entity.Property(e => e.DifficultyLevel).HasColumnName("difficulty_level").IsRequired();
                entity.Property(e => e.ContentLatex).HasColumnName("content_latex").IsRequired();
                entity.Property(e => e.OptionA).HasColumnName("option_a").IsRequired();
                entity.Property(e => e.OptionB).HasColumnName("option_b").IsRequired();
                entity.Property(e => e.OptionC).HasColumnName("option_c").IsRequired();
                entity.Property(e => e.OptionD).HasColumnName("option_d").IsRequired();
                entity.Property(e => e.CorrectOption).HasColumnName("correct_option").IsRequired();
                entity.Property(e => e.Explanation).HasColumnName("explanation");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(d => d.Skill)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.SkillId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Passage)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.PassageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình MockExam
            modelBuilder.Entity<MockExam>(entity =>
            {
                entity.ToTable("MockExams");
                entity.HasKey(e => e.ExamId);
                entity.Property(e => e.ExamId).HasColumnName("exam_id").HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Title).HasColumnName("title").IsRequired();
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
                entity.Property(e => e.TotalQuestions).HasColumnName("total_questions").IsRequired();
                entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
                entity.Property(e => e.ExamCategory).HasColumnName("exam_category").IsRequired().HasDefaultValue("FULL_MOCK");
                entity.Property(e => e.SubjectCode).HasColumnName("subject_code");
                entity.Property(e => e.DifficultyLevel).HasColumnName("difficulty_level");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            });

            // Cấu hình ExamQuestion
            modelBuilder.Entity<ExamQuestion>(entity =>
            {
                entity.ToTable("ExamQuestions");
                entity.HasKey(e => new { e.ExamId, e.QuestionId });
                entity.Property(e => e.ExamId).HasColumnName("exam_id");
                entity.Property(e => e.QuestionId).HasColumnName("question_id");
                entity.Property(e => e.QuestionOrder).HasColumnName("question_order").IsRequired();

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.ExamQuestions)
                    .HasForeignKey(d => d.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.ExamQuestions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
