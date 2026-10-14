# Nhật Ký Cập Nhật (Update Log) - Content Service

## [14/10/2026] - Triển Khai REST API Dàn Kiến Thức & Khung Tri Thức Động (Curriculum & Knowledge Tree API)

- **Triển Khai REST API Dàn Kiến Thức (`GET /api/content/curriculum`)**:
  - Xây dựng `CurriculumController.cs` tiếp nhận truy vấn dàn kiến thức chuẩn ĐHQG-HCM từ Web Client qua Gateway.
  - Xây dựng `GetCurriculumTreeQuery.cs` và `GetCurriculumTreeQueryHandler.cs` trong tầng Application: Truy vấn trực tiếp từ cơ sở dữ liệu `CompetencyDomains`, `Skills`, `Materials`, `Questions` và `SkillPrerequisites`.
  - Loại bỏ hoàn toàn việc hardcode danh mục trên Frontend: Cung cấp đầy đủ cấu trúc 4 miền, 12 kỹ năng chuẩn, trọng số, chuẩn Bloom, công thức KaTeX và mô hình liên môn GraphRAG.
- **Biên Dịch & Kiểm Thử Vận Hành**:
  - `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error** trên .NET 9).
  - Kiểm thử trực tiếp qua Gateway `GET http://localhost:5212/api/content/curriculum` trả về 200 OK với đầy đủ 4 miền năng lực và 12 kỹ năng.
