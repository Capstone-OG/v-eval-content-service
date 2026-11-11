using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Domain.Entities;
using V_Eval_Content_Service.Infrastructure.Persistence;

namespace V_Eval_Content_Service.Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeder khởi tạo 12 Kỹ năng chuẩn ĐGNL ĐHQG-HCM và Đồ thị có hướng DAG tiên quyết (Core Flow 2)
/// </summary>
public static class SkillPrerequisiteSeeder
{
    // 4 Miền Năng Lực Chuẩn
    public static readonly Guid DomainLanguageId = Guid.Parse("b581ee4c-7277-4be8-a156-c5b20a0a59f6"); // Sử dụng Ngôn ngữ
    public static readonly Guid DomainMathLogicId = Guid.Parse("6f3765db-943e-4810-bf74-d6a8bdc215da"); // Toán học, Tư duy Logic & Phân tích số liệu
    public static readonly Guid DomainNaturalScienceId = Guid.Parse("77777777-7777-7777-7777-000000000001"); // Khoa học tự nhiên
    public static readonly Guid DomainSocialScienceId = Guid.Parse("77777777-7777-7777-7777-000000000002"); // Khoa học xã hội

    // 12 Kỹ Năng Chuẩn ĐGNL ĐHQG-HCM
    public static readonly Guid VietDocHieuId = Guid.Parse("a0000001-0000-0000-0000-000000000001"); // Đọc hiểu văn bản Tiếng Việt
    public static readonly Guid VietNguPhapId = Guid.Parse("a0000001-0000-0000-0000-000000000002"); // Ngữ pháp & Logic câu Tiếng Việt
    public static readonly Guid EngReadingId = Guid.Parse("a0000001-0000-0000-0000-000000000003"); // Đọc hiểu văn bản Tiếng Anh
    public static readonly Guid EngGrammarId = Guid.Parse("a0000001-0000-0000-0000-000000000004"); // Ngữ pháp & Từ vựng Tiếng Anh
    public static readonly Guid MathAlgebraId = Guid.Parse("a0000001-0000-0000-0000-000000000005"); // Đại số, Hàm số & Giải tích
    public static readonly Guid MathGeometryId = Guid.Parse("a0000001-0000-0000-0000-000000000006"); // Hình học & Lượng giác không gian
    public static readonly Guid LogicDeductionId = Guid.Parse("a0000001-0000-0000-0000-000000000007"); // Suy luận Logic & Mệnh đề
    public static readonly Guid DataAnalysisId = Guid.Parse("a0000001-0000-0000-0000-000000000008"); // Phân tích số liệu & Bảng biểu
    public static readonly Guid PhysMechanicsId = Guid.Parse("a0000001-0000-0000-0000-000000000009"); // Vật lý đại cương & Cơ nhiệt
    public static readonly Guid ChemReactionsId = Guid.Parse("a0000001-0000-0000-0000-000000000010"); // Hóa học vô cơ & Hữu cơ
    public static readonly Guid BioGeneticsId = Guid.Parse("a0000001-0000-0000-0000-000000000011"); // Sinh học di truyền & Sinh thái
    public static readonly Guid SocHistoryGeoId = Guid.Parse("a0000001-0000-0000-0000-000000000012"); // Tổng hợp Lịch sử & Địa lý VN

    public static async Task SeedSkillsAndPrerequisitesAsync(ContentDbContext context, ILogger logger)
    {
        try
        {
            // 1. Khởi tạo / cập nhật Competency Domains
            var domains = new List<CompetencyDomain>
            {
                new() { DomainId = DomainLanguageId, Name = "Sử dụng Ngôn ngữ", Description = "Miền 1: Sử dụng Ngôn ngữ Tiếng Việt và Tiếng Anh (Tối đa 450 điểm)" },
                new() { DomainId = DomainMathLogicId, Name = "Toán học & Tư duy định lượng", Description = "Miền 2: Toán học, Tư duy Logic & Phân tích số liệu (Tối đa 400 điểm)" },
                new() { DomainId = DomainNaturalScienceId, Name = "Khoa học tự nhiên", Description = "Miền 3: Vật lý, Hóa học và Sinh học (Tối đa 200 điểm)" },
                new() { DomainId = DomainSocialScienceId, Name = "Khoa học xã hội", Description = "Miền 4: Lịch sử, Địa lý và Kiến thức xã hội tổng hợp (Tối đa 150 điểm)" }
            };

            foreach (var domain in domains)
            {
                var existing = await context.CompetencyDomains.FindAsync(domain.DomainId);
                if (existing == null)
                {
                    context.CompetencyDomains.Add(domain);
                }
            }
            await context.SaveChangesAsync();

            // 2. Khởi tạo 12 Kỹ năng chuẩn
            var standardSkills = new List<Skill>
            {
                new() { SkillId = VietDocHieuId, DomainId = DomainLanguageId, Name = "Đọc hiểu văn bản Tiếng Việt", Weight = 0.15 },
                new() { SkillId = VietNguPhapId, DomainId = DomainLanguageId, Name = "Ngữ pháp & Logic câu Tiếng Việt", Weight = 0.10 },
                new() { SkillId = EngReadingId, DomainId = DomainLanguageId, Name = "Đọc hiểu văn bản Tiếng Anh", Weight = 0.10 },
                new() { SkillId = EngGrammarId, DomainId = DomainLanguageId, Name = "Ngữ pháp & Từ vựng Tiếng Anh", Weight = 0.10 },
                new() { SkillId = MathAlgebraId, DomainId = DomainMathLogicId, Name = "Đại số, Hàm số & Giải tích", Weight = 0.12 },
                new() { SkillId = MathGeometryId, DomainId = DomainMathLogicId, Name = "Hình học & Lượng giác không gian", Weight = 0.08 },
                new() { SkillId = LogicDeductionId, DomainId = DomainMathLogicId, Name = "Suy luận Logic & Mệnh đề", Weight = 0.10 },
                new() { SkillId = DataAnalysisId, DomainId = DomainMathLogicId, Name = "Phân tích số liệu & Bảng biểu", Weight = 0.10 },
                new() { SkillId = PhysMechanicsId, DomainId = DomainNaturalScienceId, Name = "Vật lý đại cương & Cơ nhiệt", Weight = 0.05 },
                new() { SkillId = ChemReactionsId, DomainId = DomainNaturalScienceId, Name = "Hóa học vô cơ & Hữu cơ", Weight = 0.04 },
                new() { SkillId = BioGeneticsId, DomainId = DomainNaturalScienceId, Name = "Sinh học di truyền & Sinh thái", Weight = 0.03 },
                new() { SkillId = SocHistoryGeoId, DomainId = DomainSocialScienceId, Name = "Tổng hợp Lịch sử & Địa lý VN", Weight = 0.03 }
            };

            foreach (var skill in standardSkills)
            {
                var existing = await context.Skills.FindAsync(skill.SkillId);
                if (existing == null)
                {
                    context.Skills.Add(skill);
                }
                else
                {
                    existing.Weight = skill.Weight;
                    existing.DomainId = skill.DomainId;
                }
            }
            await context.SaveChangesAsync();

            // 3. Khởi tạo 9 Cặp quan hệ tiên quyết (DAG Không Chu Trình)
            var prerequisites = new List<SkillPrerequisite>
            {
                // Ngôn ngữ Tiếng Việt: S1 -> S2
                new() { SkillId = VietNguPhapId, PrerequisiteId = VietDocHieuId },
                // Ngôn ngữ Tiếng Anh: S3 -> S4
                new() { SkillId = EngGrammarId, PrerequisiteId = EngReadingId },
                // Toán học & Logic: S5 -> S6, S5 -> S7, S5 -> S8, S7 -> S8
                new() { SkillId = MathGeometryId, PrerequisiteId = MathAlgebraId },
                new() { SkillId = LogicDeductionId, PrerequisiteId = MathAlgebraId },
                new() { SkillId = DataAnalysisId, PrerequisiteId = MathAlgebraId },
                new() { SkillId = DataAnalysisId, PrerequisiteId = LogicDeductionId },
                // Khoa học tự nhiên: S5 -> S9, S10 -> S11
                new() { SkillId = PhysMechanicsId, PrerequisiteId = MathAlgebraId },
                new() { SkillId = BioGeneticsId, PrerequisiteId = ChemReactionsId },
                // Khoa học xã hội: S1 -> S12
                new() { SkillId = SocHistoryGeoId, PrerequisiteId = VietDocHieuId }
            };

            foreach (var prereq in prerequisites)
            {
                var exists = await context.SkillPrerequisites.AnyAsync(sp =>
                    sp.SkillId == prereq.SkillId && sp.PrerequisiteId == prereq.PrerequisiteId);

                if (!exists)
                {
                    context.SkillPrerequisites.Add(prereq);
                }
            }
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully verified and seeded 12 Standard Skills and 9 DAG Prerequisite relations.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding 12 Standard Skills and DAG Prerequisites.");
        }
    }
}
