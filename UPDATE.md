# Nhật Ký Cập Nhật (Update Log) - Content Service

## [30/09/2026] - Triển Khai Hoàn Thiện APIs 20, 21: Quản Trị Bài Giảng Video Lý Thuyết & Phân Công Nhiệm Vụ APIs 16-19 Cho ThinhTT

- **API 20: Tạo Mới Bài Giảng Video Lý Thuyết Chuẩn Theo Kỹ Năng (`POST /api/v1/content/materials`)**:
  - Khởi tạo DTOs [`MaterialDtos.cs`](./V-Eval-Content_Service.Application/Materials/DTOs/MaterialDtos.cs): `CreateMaterialRequestDto`, `MaterialDetailDto`, `MaterialItemDto`.
  - Xây dựng FluentValidation `CreateMaterialCommandValidator` kiểm tra tính toàn vẹn dữ liệu.
  - Bổ sung trường `DurationSeconds` vào thực thể [`Material.cs`](./V-Eval-Content_Service.Domain/Entities/Material.cs) và ánh xạ cột `duration_seconds` trong `ContentDbContext.cs`.
  - Triển khai `CreateMaterialCommand` và [`CreateMaterialCommandHandler.cs`](./V-Eval-Content_Service.Application/Materials/Commands/CreateMaterial/CreateMaterialCommandHandler.cs): xác thực kỹ năng `SkillId` tồn tại trong đồ thị DAG (`404 Not Found` `SkillNotFound`), khởi tạo bài giảng video lý thuyết chuẩn và trả về `201 Created`.
- **API 21: Tra Cứu Danh Sách Bài Giảng Video Theo Kỹ Năng (`GET /api/v1/content/materials/by-skill/{skillId}`)**:
  - Khởi tạo `GetMaterialsBySkillQuery`, `GetMaterialsBySkillQueryValidator` và [`GetMaterialsBySkillQueryHandler.cs`](./V-Eval-Content_Service.Application/Materials/Queries/GetMaterialsBySkill/GetMaterialsBySkillQueryHandler.cs).
  - Nghiệp vụ: Xác thực `skillId` tồn tại (`404 Not Found`), cung cấp toàn bộ danh sách bài giảng lý thuyết, link video và tài liệu đính kèm để phục vụ học sinh xem bài giảng trước khi mở khóa Quiz chặng học trong Practice Service.
  - Bổ sung endpoint tra cứu chi tiết bài giảng: `GET /api/v1/content/materials/{id}`.
- **Chuẩn Hóa Đồng Bộ Route API (`api/content/...`) Khớp Với API Gateway**:
  - Gỡ bỏ hoàn toàn tiền tố `v1` khỏi toàn bộ các Controller trong Content Service ([`DiagnosticController.cs`](./V-Eval-Content_Service.API/Controllers/DiagnosticController.cs), [`MockExamsController.cs`](./V-Eval-Content_Service.API/Controllers/MockExamsController.cs), [`MaterialsController.cs`](./V-Eval-Content_Service.API/Controllers/MaterialsController.cs)).
  - Đồng bộ 100% với cấu hình YARP Reverse Proxy của API Gateway (`/api/content/{**catch-all}`) và loại bỏ hoàn toàn các route duplicate card trên Swagger UI.
- **Tầng API Controller ([`MaterialsController.cs`](./V-Eval-Content_Service.API/Controllers/MaterialsController.cs))**:
  - Khởi tạo 3 endpoints: `[HttpPost]`, `[HttpGet("by-skill/{skillId:guid}")]`, `[HttpGet("{id:guid}")]`.
- **Phân Công Triển Khai Cho ThinhTT (Giai Đoạn 4)**:
  - **API 16 (`POST /api/v1/content/questions`)**: Thêm mới câu hỏi trắc nghiệm vào ngân hàng câu hỏi gốc. *Người phụ trách: ThinhTT*.
  - **API 17 (`PUT /api/v1/content/questions/{questionId}`)**: Hiệu đính nội dung câu hỏi trong ngân hàng đề. *Người phụ trách: ThinhTT*.
  - **API 18 (`DELETE /api/v1/content/questions/{questionId}`)**: Xóa mềm hoặc vô hiệu hóa câu hỏi. *Người phụ trách: ThinhTT*.
  - **API 19 (`POST /api/v1/content/exams/quiz`)**: Đóng gói và phát hành bộ đề Quiz củng cố chuyên đề chuẩn hóa. *Người phụ trách: ThinhTT*.
- **Kiểm Thử Vận Hành Trực Tiếp (Live End-to-End Test)**:
  - Solution `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error**).
  - Kịch bản API 20: Tạo bài giảng chuyên đề Đại số cho Skill `a0000001-0000-0000-0000-000000000005` -> `201 Created` trả về ID `21d4b371...`, `durationSeconds: 1800`.
  - Kịch bản API 20 (Unhappy Case): Thử với `skillId` không tồn tại -> `404 Not Found` (`SkillNotFound`). Thử với `videoUrl` sai format -> `400 Bad Request`.
  - Kịch bản API 21: Tra cứu bài giảng theo kỹ năng `a0000001...` -> `200 OK`, trả về danh sách 1 bài giảng đầy đủ link video, thời lượng chuẩn và tóm tắt công thức.
  - Kịch bản API chi tiết: Tra cứu `GET /materials/21d4b371...` -> `200 OK` đầy đủ thông tin kỹ năng và miền năng lực liên kết.
