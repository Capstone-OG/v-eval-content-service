# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [11/11/2026] - Nâng Cấp Khả Năng Xóa Toàn Diện Miền Năng Lực & Bảo Vệ Toàn Vẹn Danh Mục Khung Tri Thức Chuẩn V-ACT

- **Bổ Sung Tính Năng Xóa Toàn Diện Miền Năng Lực ([`DeleteNodeCommand.cs`](./V-Eval-Content_Service.Application/Curriculum/Commands/DeleteNode/DeleteNodeCommand.cs))**:
  - Hỗ trợ cú pháp xóa toàn bộ Miền năng lực thông qua tiền tố `domain-{domainId}` hoặc trực tiếp bằng `domainGuid`.
  - Tự động thu hồi và dọn dẹp phân cấp toàn diện: Xóa toàn bộ DAG Prerequisites, bài học, dạng bài, câu hỏi liên kết, học liệu (Materials) và xóa vĩnh viễn thực thể `CompetencyDomain` khỏi cơ sở dữ liệu.
  - Cho phép người dùng và giảng viên một chạm xóa bỏ các miền dư thừa hoặc phân loại nhầm (như miền "Quan hệ song song trong không gian").

- **Bảo Vệ Tính Toàn Vẹn Của Danh Mục Miền Chuẩn ([`ImportCurriculumCommand.cs`](./V-Eval-Content_Service.Application/Curriculum/Commands/ImportCurriculum/ImportCurriculumCommand.cs))**:
  - Ngăn chặn triệt để tình trạng ghi đè tên miền hiện có bằng các tiêu đề chuyên đề hoặc nội dung do AI trích xuất (chỉ cập nhật tên khi miền vừa tạo mới hoặc đang mang tên mặc định tạm "Miền Năng Lực").
  - Giữ vững cấu trúc 5 miền năng lực khảo thí chuẩn hóa của đề thi V-ACT (Toán học, Ngữ văn, KHTN, KHXH, Tiếng Anh).

- **Biên Dịch & Kiểm Thử Vận Hành**:
  - Biên dịch toàn bộ Content Service đạt 0 lỗi (`0 Error(s)`).
  - Toàn bộ 3/3 bài kiểm thử unit tests vượt qua thành công 100%.
