# Nhật Ký Cập Nhật (Update Log) - Content Service

## [23/10/2026] - Đồng Bộ CSDL Khung Năng Lực & Đề Thi Chuẩn Vào Master SQL.sql Và Capstone.drawio

- **Đồng Bộ CSDL Phân Hệ Content (`v_eval_content`)**:
  - Hợp nhất toàn bộ 8 bảng thực thể vào `docs/SQL/SQL.sql`: `CompetencyDomains`, `Skills`, `SkillPrerequisites` (DAG), `Materials`, `Passages`, `Questions`, `MockExams`, `ExamQuestions`.
  - Chuẩn hóa đầy đủ các trường phân loại đề thi (`exam_category`, `subject_code`, `difficulty_level`), độ dài video học liệu `duration_seconds` và các ràng buộc đồ thị tiên quyết.
- **Đồng Bộ ERD Trang 1 Trong `docs/SQL/Capstone.drawio`**:
  - Phân bổ vào 2 package: `3. Khung năng lực & Ngân hàng câu hỏi` (6 bảng) và `4. Thi thử & Giám sát liêm chính` (liên kết đề thi - câu hỏi).
