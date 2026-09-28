# Nhật Ký Cập Nhật (Update Log) - Content Service

## [28/09/2026] - Hợp Nhất Toàn Diện Core Flow 1 (Bloom 6 Cấp, Phê Duyệt Đề Thi & ExcludeExamId) & Core Flow 2 (SkillPrerequisites DAG & gRPC GetSkillsTree)

- **Hợp Nhất Toàn Diện Nhánh `develop` và Nhánh `ThinhTT/feat-diagnostic-exam-studio-bloom-flow`**:
  - Đồng bộ hóa hoàn hảo mã nguồn giữa hệ thống phân loại Bloom, xuất bản đề thi và đồ thị tri thức DAG.
- **Core Flow 2 (Giai Đoạn 1) — Thực Thể SkillPrerequisites, Seeding DAG 12 Kỹ Năng & RPC GetSkillsTree**:
  - **Thực Thể & CSDL SkillPrerequisites (Đồ Thị Tiên Quyết DAG)**:
    - Bổ sung entity [`SkillPrerequisite.cs`](./V-Eval-Content_Service.Domain/Entities/SkillPrerequisite.cs) với khóa chính phức hợp `(skill_id, prerequisite_id)` đại diện cho cung có hướng trong đồ thị DAG.
    - Cập nhật [`Skill.cs`](./V-Eval-Content_Service.Domain/Entities/Skill.cs) bổ sung 2 navigation collections `Prerequisites` (tiên quyết) và `DependentSkills` (phụ thuộc).
    - Cấu hình Fluent API trong [`ContentDbContext.cs`](./V-Eval-Content_Service.Infrastructure/Persistence/ContentDbContext.cs) và cập nhật [`IContentDbContext.cs`](./V-Eval-Content_Service.Application/Common/Interfaces/IContentDbContext.cs).
  - **Bộ Dữ Liệu Chuẩn & Seeding Tự Động (`SkillPrerequisiteSeeder.cs`)**:
    - Khởi tạo seeder [`SkillPrerequisiteSeeder.cs`](./V-Eval-Content_Service.Infrastructure/Persistence/Seeds/SkillPrerequisiteSeeder.cs) tự động xác thực và nạp 4 Miền Năng Lực, 12 Kỹ Năng Chuẩn ĐGNL ĐHQG-HCM kèm trọng số và 9 cặp quan hệ DAG không chu trình.
    - Kích hoạt seeder tự động trong [`Program.cs`](./V-Eval-Content_Service.API/Program.cs) khi khởi động service.
  - **Triển Khai RPC GetSkillsTree (`ContentGrpcService.cs`)**:
    - Hiện thực phương thức gRPC `GetSkillsTree` trong [`ContentGrpcService.cs`](./V-Eval-Content_Service.API/Services/ContentGrpcService.cs) theo hợp đồng `content.proto`, cho phép Practice Service truy xuất toàn bộ cây kỹ năng và danh sách `prerequisite_ids` phục vụ thuật toán Path Planning.
- **Core Flow 1 — Hỗ Trợ Đổi Đề Chẩn Đoán (Unhappy Case 2), Phê Duyệt Xuất Bản & Thang Đo Bloom 6 Cấp**:
  - **Hỗ Trợ Tham Số `excludeExamId` Cho Đề Thi Chẩn Đoán (`DiagnosticController.cs`)**:
    - Hỗ trợ tham số truy vấn `excludeExamId` trong API `GET /api/v1/content/diagnostic-test?excludeExamId=...` giúp học sinh lấy bộ đề khác khi đề cũ bị khóa do quá hạn 24 giờ.
  - **Bổ Sung Endpoint Phê Duyệt Đề Thi (`PATCH /api/v1/content/exams/{id}/publish`)**:
    - Tích hợp endpoint RESTful trong [`MockExamsController.cs`](./V-Eval-Content_Service.API/Controllers/MockExamsController.cs) tiếp nhận lệnh duyệt đề từ giáo viên, cập nhật cờ `is_published: true`.
  - **Chuẩn Hóa Thang Đo Tư Duy Bloom 6 Cấp Độ**:
    - Khởi tạo [`BloomTaxonomy.cs`](./V-Eval-Content_Service.Domain/Constants/BloomTaxonomy.cs) định nghĩa 6 mức độ nhận thức (Nhận biết $\rightarrow$ Sáng tạo).
  - **Chuẩn Hóa Múi Giờ Việt Nam (Asia/Ho_Chi_Minh UTC+7) Cho CSDL & DTOs**:
    - Bổ sung trường `createdAtVn` trong `MockExamSummaryDto` và `MockExamDetailDto` tự động định dạng chuẩn ngày giờ Việt Nam (`dd/MM/yyyy HH:mm:ss`).
- **Kiểm Thử Biên Dịch & Vận Hành**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).
  - Kiểm thử `PATCH /publish`, `GetSkillsTree` gRPC và đồng bộ CSDL Supabase PostgreSQL thành công.
