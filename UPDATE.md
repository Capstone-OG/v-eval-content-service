# Nhật Ký Cập Nhật (Update Log) - Content Service

## [15/10/2026] - Triển Khai Kiến Trúc Nạp Tri Thức Động Kép (Giảng Viên & AI Ingestion), Bộ API Hiệu Đính Sư Phạm & Xóa Bỏ 100% Hardcode

- **Xóa Bỏ Toàn Bộ Hardcode C# & Dữ Liệu Tĩnh Trong Mã Nguồn**:
  - Loại bỏ hoàn toàn `CurriculumHierarchySeeder.cs` và các switch-case hardcode LaTeX/câu hỏi giả trong `GetCurriculumTreeQuery.cs`.
  - Toàn bộ dữ liệu được truy vấn 100% động từ cơ sở dữ liệu PostgreSQL (`v_eval_content`), sử dụng quan hệ điều hướng EF Core và biểu thức chính quy (Regex) trích xuất công thức KaTeX từ văn bản thực tế.
- **Triển Khai REST API Nhập Liệu Tri Thức Động (`POST /api/content/curriculum/import`)**:
  - Hỗ trợ kiến trúc 2 kênh bổ trợ lẫn nhau: Giảng viên/Ban Chuyên Môn chủ động quản lý nhập liệu và AI Engine tự động nạp phân tích từ file PDF Sách Giáo Khoa.
  - Triển khai `ImportCurriculumCommand` và `ImportCurriculumCommandHandler`: Tự động upsert toàn bộ cây phân cấp đa môn: `Miền Năng Lực` -> `Chương` -> `Bài học` -> `Dạng bài` -> `Materials` -> `Questions` -> `SkillPrerequisites`.
- **Triển Khai Trọn Bộ REST APIs Hiệu Đính Tri Thức Sư Phạm Dành Riêng Cho Giảng Viên**:
  - **`PUT /api/content/curriculum/patterns/{id}` (`UpdatePatternCommand`)**: Cho phép giảng viên toàn quyền hiệu đính 4 khối nội dung dạng bài: Tên dạng, trọng số, Khối 1 (Nền tảng SGK), Khối 2 (Phương pháp giải KaTeX từng bước), Khối 3 (Video URL & thời lượng), Khối 4 (Câu hỏi mẫu minh họa, đáp án đúng & lời giải chi tiết).
  - **`PUT /api/content/curriculum/nodes/{id}` (`UpdateNodeCommand`)**: Cập nhật tên, mô tả hoặc thứ bậc của Chương / Bài học.
  - **`POST /api/content/curriculum/nodes` (`CreateNodeCommand`)**: Cho phép giảng viên thêm mới Chương, Bài học hoặc Dạng bài vào Cây tri thức.
  - **`DELETE /api/content/curriculum/nodes/{id}` (`DeleteNodeCommand`)**: Xóa node an toàn và tự động dọn dẹp quan hệ tiên quyết DAG, tài liệu và câu hỏi liên quan.
- **Cấu Trúc Cây 3 Tầng Chuẩn Sư Phạm SGK Cho Mọi Môn Học**:
  - Trả về cấu trúc cây 3 tầng động chuẩn mực (`Chapters` -> `Lessons` -> `Patterns`) qua API `GET /api/content/curriculum`.
- **Biên Dịch & Kiểm Thử Vận Hành**:
  - `V-Eval-Content_Service.sln` biên dịch sạch 100% (**0 Warning, 0 Error** trên .NET 9 SDK).
