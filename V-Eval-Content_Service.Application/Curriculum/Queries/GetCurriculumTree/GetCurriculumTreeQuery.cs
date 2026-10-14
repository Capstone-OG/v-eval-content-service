using System;
using System.Collections.Generic;
using System.Linq;
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
        // 1. Lấy danh sách CompetencyDomains kèm Skills, Prerequisites, Materials, Questions từ Database
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
            var (domainCode, title, subtitle, icon, gradient, activeBg) = ResolveDomainMetadata(d.DomainId, d.Name);

            var domainDto = new CurriculumDomainDto
            {
                Id = $"domain-{domainCode.ToLowerInvariant().Replace("_", "-")}",
                DomainId = d.DomainId.ToString(),
                DomainCode = domainCode,
                Title = !string.IsNullOrWhiteSpace(d.Name) ? d.Name : title,
                Subtitle = subtitle,
                Icon = icon,
                Gradient = gradient,
                ActiveBg = activeBg,
                Skills = new List<CurriculumSkillDto>()
            };

            foreach (var s in d.Skills.OrderBy(s => s.CreatedAt).ThenBy(s => s.Name))
            {
                var skillMetadata = ResolveSkillMetadata(s.SkillId, s.Name, domainCode);

                // Sample Question: ưu tiên câu hỏi từ Database, nếu chưa có thì dùng câu hỏi chuẩn
                var dbQuestion = s.Questions.FirstOrDefault();
                CurriculumSampleQuestionDto sampleQ;
                if (dbQuestion != null)
                {
                    sampleQ = new CurriculumSampleQuestionDto
                    {
                        QuestionId = dbQuestion.QuestionId.ToString(),
                        Content = dbQuestion.ContentLatex,
                        Options = new List<string>
                        {
                            $"A. {dbQuestion.OptionA}",
                            $"B. {dbQuestion.OptionB}",
                            $"C. {dbQuestion.OptionC}",
                            $"D. {dbQuestion.OptionD}"
                        },
                        Correct = dbQuestion.CorrectOption switch
                        {
                            'A' => 0,
                            'B' => 1,
                            'C' => 2,
                            'D' => 3,
                            _ => 0
                        },
                        Explanation = dbQuestion.Explanation ?? "Lời giải chi tiết chuẩn hóa theo phương pháp tư duy ĐHQG-HCM."
                    };
                }
                else
                {
                    sampleQ = skillMetadata.SampleQuestion;
                }

                // Video Lesson: ưu tiên tài liệu bài giảng từ Database
                var dbMaterial = s.Materials.FirstOrDefault();
                CurriculumVideoLessonDto video;
                if (dbMaterial != null)
                {
                    video = new CurriculumVideoLessonDto
                    {
                        Title = dbMaterial.Title,
                        Teacher = "Thầy Cô Ban Chuyên Môn ĐGNL",
                        Duration = $"{Math.Max(15, dbMaterial.DurationSeconds / 60)} phút",
                        Views = "3.5k lượt học",
                        VideoUrl = dbMaterial.VideoUrl
                    };
                }
                else
                {
                    video = skillMetadata.VideoLesson;
                }

                var skillDto = new CurriculumSkillDto
                {
                    SkillId = s.SkillId.ToString(),
                    Name = s.Name,
                    Code = skillMetadata.Code,
                    Weight = $"{Math.Round((s.Weight ?? 0.08) * 100)}%",
                    Level = skillMetadata.Level,
                    CrossModel = skillMetadata.CrossModel,
                    CrossNote = skillMetadata.CrossNote,
                    Theory = skillMetadata.Theory,
                    KeyFormulas = skillMetadata.KeyFormulas,
                    PrerequisiteIds = s.Prerequisites.Select(p => p.PrerequisiteId.ToString()).ToList(),
                    VideoLesson = video,
                    SampleQuestion = sampleQ
                };

                domainDto.Skills.Add(skillDto);
            }

            resultList.Add(domainDto);
        }

        return Result<List<CurriculumDomainDto>>.Success(resultList);
    }

    private static (string DomainCode, string Title, string Subtitle, string Icon, string Gradient, string ActiveBg) ResolveDomainMetadata(Guid domainId, string domainName)
    {
        string idStr = domainId.ToString().ToLowerInvariant();
        if (idStr == "6f3765db-943e-4810-bf74-d6a8bdc215da" || domainName.Contains("Toán", StringComparison.OrdinalIgnoreCase))
        {
            return ("MATH_LOGIC", "Toán học & Tư duy định lượng", "30 câu • Trọng số 300/1200 điểm", "Cpu", "from-blue-600 via-indigo-600 to-cyan-500", "bg-blue-50 border-blue-200 text-blue-700");
        }
        if (idStr == "b581ee4c-7277-4be8-a156-c5b20a0a59f6" || domainName.Contains("Ngôn ngữ", StringComparison.OrdinalIgnoreCase))
        {
            return ("LANGUAGE", "Sử dụng Ngôn ngữ (Việt - Anh)", "40 câu • Trọng số 400/1200 điểm", "BookOpen", "from-emerald-600 via-teal-600 to-green-500", "bg-emerald-50 border-emerald-200 text-emerald-700");
        }
        if (idStr == "77777777-7777-7777-7777-000000000001" || domainName.Contains("tự nhiên", StringComparison.OrdinalIgnoreCase))
        {
            return ("NATURAL_SCIENCES", "Khoa học Tự nhiên", "30 câu • Trọng số 300/1200 điểm", "Atom", "from-amber-600 via-orange-600 to-rose-500", "bg-amber-50 border-amber-200 text-amber-700");
        }
        if (idStr == "77777777-7777-7777-7777-000000000002" || domainName.Contains("xã hội", StringComparison.OrdinalIgnoreCase))
        {
            return ("SOCIAL_SCIENCES", "Khoa học Xã hội & Lịch sử Địa lý", "20 câu • Trọng số 200/1200 điểm", "BarChart2", "from-violet-600 via-purple-600 to-pink-500", "bg-purple-50 border-purple-200 text-purple-700");
        }

        return ("GENERAL", domainName, "Chuyên đề khảo thí", "Layers", "from-slate-700 to-slate-900", "bg-slate-50 border-slate-200 text-slate-700");
    }

    private static (string Code, string Level, string CrossModel, string CrossNote, string Theory, List<CurriculumFormulaDto> KeyFormulas, CurriculumVideoLessonDto VideoLesson, CurriculumSampleQuestionDto SampleQuestion) ResolveSkillMetadata(Guid skillId, string skillName, string domainCode)
    {
        string nameLower = skillName.ToLowerInvariant();

        if (nameLower.Contains("đạo hàm") || nameLower.Contains("hàm số"))
        {
            return (
                "MATH_01",
                "Vận dụng",
                "Mô hình Tốc độ Biến thiên Tức thời (Instantaneous Rate of Change)",
                "Công thức f'(x) được kế thừa trực tiếp trong Vật lý (v = s'(t), a = v'(t)) và Hóa học (Tốc độ phản ứng tức thời v = -d[A]/dt).",
                "Đạo hàm của hàm số y = f(x) tại điểm x_0 biểu diễn hệ số góc của tiếp tuyến và tốc độ biến thiên tức thời của đại lượng theo thời gian.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Định nghĩa đạo hàm", Latex = "f'(x_0) = \\lim_{\\Delta x \\to 0} \\frac{f(x_0 + \\Delta x) - f(x_0)}{\\Delta x}" },
                    new() { Label = "Tiếp tuyến đồ thị", Latex = "y - y_0 = f'(x_0)(x - x_0)" },
                    new() { Label = "Đạo hàm hàm hợp", Latex = "(g(u(x)))' = g'(u) \\cdot u'(x)" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Kỹ thuật giải nhanh Cực trị & Tiệm cận hàm phân thức",
                    Teacher = "Thầy Phạm Duy • Ban Khảo thí ĐGNL",
                    Duration = "28 phút",
                    Views = "3.2k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Cho hàm số y = f(x) có đạo hàm f'(x) = (x - 1)^2 (x^2 - 4). Số điểm cực trị của hàm số đã cho là:",
                    Options = new List<string> { "A. 1", "B. 2", "C. 3", "D. 4" },
                    Correct = 1,
                    Explanation = "Ta có f'(x) = 0 \\iff x = 1 (nghiệm bội chẵn) hoặc x = \\pm 2 (nghiệm đơn). Đạo hàm chỉ đổi dấu khi qua các nghiệm đơn x = 2 và x = -2. Do đó hàm số có đúng 2 điểm cực trị."
                }
            );
        }

        if (nameLower.Contains("hình học") || nameLower.Contains("lượng giác") || nameLower.Contains("tích phân"))
        {
            return (
                "MATH_02",
                "Vận dụng",
                "Mô hình Tích lũy Liên tục (Continuous Accumulation)",
                "Tích phân Riemann là cơ sở tính Công cơ học A = \\int F ds trong Vật lý và Năng lượng hoạt hóa phản ứng.",
                "Tích phân xác định biểu thị diện tích hình thang cong giới hạn bởi đường cong y = f(x), trục hoành và hai đường thẳng x = a, x = b.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Công thức Newton-Leibniz", Latex = "\\int_a^b f(x)\\,dx = F(b) - F(a)" },
                    new() { Label = "Diện tích hình phẳng", Latex = "S = \\int_a^b |f(x) - g(x)|\\,dx" },
                    new() { Label = "Thể tích khối tròn xoay", Latex = "V = \\pi \\int_a^b [f(x)]^2\\,dx" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Bấm máy tính Casio tối ưu tích phân diện tích hình phẳng",
                    Teacher = "Cô Nguyễn Mai • Chuyên gia Toán ĐGNL",
                    Duration = "32 phút",
                    Views = "4.8k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Diện tích hình phẳng giới hạn bởi đồ thị y = x^2 - 4 và trục hoành y = 0 là:",
                    Options = new List<string> { "A. \\frac{32}{3}", "B. \\frac{16}{3}", "C. 8", "D. 16" },
                    Correct = 0,
                    Explanation = "Phương trình hoành độ giao điểm x^2 - 4 = 0 \\iff x = \\pm 2. Diện tích S = \\int_{-2}^2 |x^2 - 4|\\,dx = 2 \\int_0^2 (4 - x^2)\\,dx = \\frac{32}{3}."
                }
            );
        }

        if (nameLower.Contains("logic") || nameLower.Contains("mệnh đề"))
        {
            return (
                "LOGIC_01",
                "Thông hiểu & Vận dụng",
                "Mô hình Đại số Boole & Đồ thị Suy luận (Inference DAG)",
                "Quy tắc Modus Tollens và bảng chân trị được áp dụng xuyên suốt trong Câu hỏi đọc hiểu Ngôn ngữ và Phân tích quy trình Pháp luật/Khoa học xã hội.",
                "Suy luận logic mệnh đề tập trung vào tính tương đương của mệnh đề phản đảo (Contrapositive) và quy tắc loại trừ mâu thuẫn.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Mệnh đề kéo theo", Latex = "P \\Rightarrow Q \\equiv \\neg P \\lor Q" },
                    new() { Label = "Luật phản đảo (Modus Tollens)", Latex = "(P \\Rightarrow Q) \\equiv (\\neg Q \\Rightarrow \\neg P)" },
                    new() { Label = "Luật De Morgan", Latex = "\\neg (P \\land Q) \\equiv \\neg P \\lor \\neg Q" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Chiến thuật hóa giải 10 dạng bẫy logic ĐHQG-HCM",
                    Teacher = "Thầy Phạm Duy • Giảng viên Logic ĐGNL",
                    Duration = "45 phút",
                    Views = "7.1k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Cho phát biểu: 'Nếu A đạt trên 850 điểm ĐGNL thì A trúng tuyển vào ĐH Bách Khoa'. Phát biểu nào sau đây có giá trị logic TƯƠNG ĐƯƠNG?",
                    Options = new List<string>
                    {
                        "A. Nếu A trúng tuyển vào ĐH Bách Khoa thì A đạt trên 850 điểm",
                        "B. Nếu A không trúng tuyển vào ĐH Bách Khoa thì A không đạt trên 850 điểm",
                        "C. Nếu A không đạt trên 850 điểm thì A không trúng tuyển",
                        "D. A đạt dưới 850 điểm hoặc A không trúng tuyển"
                    },
                    Correct = 1,
                    Explanation = "Mệnh đề dạng P \\Rightarrow Q luôn tương đương với mệnh đề phản đảo \\neg Q \\Rightarrow \\neg P. Do đó câu B là phương án chính xác."
                }
            );
        }

        if (nameLower.Contains("số liệu") || nameLower.Contains("bảng biểu"))
        {
            return (
                "DATA_01",
                "Thông hiểu & Vận dụng",
                "Mô hình Tăng trưởng Tương đối & Chỉ số Chuẩn hóa",
                "Công thức phần trăm tăng trưởng được ứng dụng trực tiếp trong Kinh tế học, Địa lý kinh tế và Hóa sinh thực nghiệm.",
                "Kỹ thuật tính tốc độ tăng trưởng, tỷ trọng cơ cấu, giá trị tuyệt đối và tốc độ tăng trưởng bình quân hàng năm qua các mốc thời gian.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Tốc độ tăng trưởng", Latex = "g = \\frac{Y_t - Y_{t-1}}{Y_{t-1}} \\times 100\\%" },
                    new() { Label = "Tỷ trọng cơ cấu", Latex = "P_i = \\frac{X_i}{\\sum X_k} \\times 100\\%" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Mẹo tính nhẩm nhanh số liệu biểu đồ không cần bấm máy",
                    Teacher = "Thầy Trần Đức • Chuyên gia Phân tích Số liệu",
                    Duration = "20 phút",
                    Views = "4.1k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Năm 2023, doanh thu của công ty A là 50 tỷ đồng. Năm 2024, doanh thu đạt 65 tỷ đồng. Tốc độ tăng trưởng doanh thu là:",
                    Options = new List<string> { "A. 15%", "B. 25%", "C. 30%", "D. 35%" },
                    Correct = 2,
                    Explanation = "Áp dụng công thức tăng trưởng: g = (65 - 50) / 50 * 100% = 30%."
                }
            );
        }

        if (nameLower.Contains("tiếng việt"))
        {
            return (
                "VIET_01",
                "Thông hiểu",
                "Phân tích Ngữ nghĩa & Ngữ cảnh (Semantic Context Analysis)",
                "Liên hệ trực tiếp với kỹ năng đọc hiểu văn bản khoa học lịch sử, địa lý và đề xuất luận điểm logic.",
                "Hệ thống các biện pháp tu từ từ vựng và cú pháp trong các phong cách ngôn ngữ chức năng.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Cấu trúc ẩn dụ", Latex = "\\text{Vế A (ẩn)} \\longleftrightarrow \\text{Vế B (xuất hiện qua nét tương đồng)}" },
                    new() { Label = "Cấu trúc hoán dụ", Latex = "\\text{Vế A} \\longleftrightarrow \\text{Vế B (xuất hiện qua nét tương cận)}" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Phân biệt Ẩn dụ và Hoán dụ trong 3 giây không bao giờ nhầm",
                    Teacher = "Thạc sĩ Lê Thanh • Thủ khoa Ngữ văn",
                    Duration = "22 phút",
                    Views = "5.4k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Trong câu thơ 'Áo chàm đưa buổi phân ly / Cầm tay nhau biết nói gì hôm nay', cụm từ 'Áo chàm' sử dụng biện pháp tu từ nào?",
                    Options = new List<string> { "A. Ẩn dụ", "B. Hoán dụ", "C. So sánh", "D. Điệp ngữ" },
                    Correct = 1,
                    Explanation = "‘Áo chàm’ chỉ đồng bào Việt Bắc dựa trên mối quan hệ tương cận nên là Hoán dụ."
                }
            );
        }

        if (nameLower.Contains("tiếng anh"))
        {
            return (
                "ENG_01",
                "Vận dụng",
                "Mô hình Khớp Đồ thị Cú pháp (Syntactic Dependency Parsing)",
                "Cấu trúc liên từ logic (However, Therefore, Although) tương đương với toán tử logic mệnh đề trong Toán học.",
                "Kỹ năng Skimming & Scanning, nhận diện ý chính, thái độ tác giả và lỗi sai ngữ pháp đảo ngữ, câu điều kiện.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Inversion Structure", Latex = "\\text{Negative Adverb} + \\text{Auxiliary} + S + V" },
                    new() { Label = "Mixed Conditional", Latex = "\\text{If} + S + \\text{had} + V_3, S + \\text{would} + V_0" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Chiến thuật xử lý bài đọc hiểu tiếng Anh 500 từ trong 4 phút",
                    Teacher = "Cô Emily Vũ • 8.5 IELTS",
                    Duration = "25 phút",
                    Views = "6.9k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Choose the correct sentence: 'Not only ______ the exam with flying colors, but he also received a full scholarship.'",
                    Options = new List<string> { "A. he passed", "B. did he pass", "C. had he passed", "D. he had passed" },
                    Correct = 1,
                    Explanation = "Đứng đầu câu là cụm phủ định 'Not only', ta bắt buộc phải dùng cấu trúc đảo ngữ: did + he + pass."
                }
            );
        }

        if (nameLower.Contains("vật lý") || nameLower.Contains("hóa học"))
        {
            return (
                "SCI_01",
                "Vận dụng",
                "Mô hình Bảo toàn Năng lượng & Phương trình Vi phân Bậc 1",
                "GraphRAG kết nối: Phương trình dao động điều hòa x'' + \\omega^2 x = 0 tương đương dao động mạch LC và phương trình cân bằng hóa học.",
                "Quy luật biến thiên chu kỳ, bảo toàn cơ năng và phương trình tốc độ phản ứng Arrhenius.",
                new List<CurriculumFormulaDto>
                {
                    new() { Label = "Phương trình dao động", Latex = "x(t) = A \\cos(\\omega t + \\varphi)" },
                    new() { Label = "Bảo toàn cơ năng", Latex = "W = \\frac{1}{2}mv^2 + \\frac{1}{2}kx^2 = \\text{const}" }
                },
                new CurriculumVideoLessonDto
                {
                    Title = "Chuỗi 5 dạng bài liên môn Vật lý - Hóa học thường gặp trong ĐGNL",
                    Teacher = "Thầy Vũ Quốc • Thủ khoa KHTN",
                    Duration = "35 phút",
                    Views = "5.1k lượt học"
                },
                new CurriculumSampleQuestionDto
                {
                    QuestionId = skillId.ToString(),
                    Content = "Một con lắc lò xo có độ cứng k = 100 N/m, khối lượng vật m = 0.1 kg dao động điều hòa. Chu kỳ dao động T xấp xỉ bằng:",
                    Options = new List<string> { "A. 0.20 s", "B. 0.31 s", "C. 0.50 s", "D. 0.63 s" },
                    Correct = 0,
                    Explanation = "Công thức chu kỳ con lắc lò xo: T = 2\\pi \\sqrt{\\frac{m}{k}} \\approx 0.20\\text{ giây}."
                }
            );
        }

        // Mặc định
        return (
            $"SKILL_{skillId.ToString()[..4].ToUpperInvariant()}",
            "Vận dụng",
            "Mô hình Tri Thức Chuẩn Hóa V-ACT",
            "Đồ thị tri thức kết nối logic đa môn theo chuẩn bài thi ĐHQG-HCM.",
            "Nắm vững bản chất định lý, nguyên lý vận hành và ứng dụng thực tiễn giải quyết bài toán phức hợp.",
            new List<CurriculumFormulaDto>
            {
                new() { Label = "Định luật bảo toàn / Cân bằng", Latex = "\\sum X_i = \\text{const}" }
            },
            new CurriculumVideoLessonDto
            {
                Title = $"Bài giảng chuyên đề: {skillName}",
                Teacher = "Ban Học Thuật ĐGNL",
                Duration = "25 phút",
                Views = "2.8k lượt học"
            },
            new CurriculumSampleQuestionDto
            {
                QuestionId = skillId.ToString(),
                Content = $"Ứng dụng kỹ năng '{skillName}' để giải quyết bài toán khảo thí tổng hợp.",
                Options = new List<string> { "A. Phương án A", "B. Phương án B", "C. Phương án C", "D. Phương án D" },
                Correct = 0,
                Explanation = "Phân tích điều kiện khả vi và tính bảo toàn năng lượng tương ứng."
            }
        );
    }
}
