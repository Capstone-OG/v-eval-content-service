# UPDATE - V-EVAL CONTENT SERVICE

## [27/11/2026] - Chọn Ngẫu Nhiên Đề Đánh Giá Đầu Vào Đã Xuất Bản Trong `GetDiagnosticTestQuery`

- **Đa Dạng Hóa Đề Kiểm Tra Đầu Vào ([GetDiagnosticTestQuery.cs](./V-Eval-Content_Service.Application/Diagnostic/Queries/GetDiagnosticTest/GetDiagnosticTestQuery.cs))**:
  - Lọc chặt chỉ lấy các đề thuộc danh mục `DIAGNOSTIC` đã xuất bản (`IsPublished == true`).
  - Chọn ngẫu nhiên (`Random.Shared.Next(...)`) trong danh sách đề đầu vào chưa hoàn thành thay vì luôn phát cố định đề đầu tiên.
- **Chuẩn Hóa Dữ Liệu Đề Đầu Vào & Bảng Số Liệu (Supabase `v_eval_content`)**:
  - Sửa phân loại domain cho 91 kỹ năng Ngôn ngữ bị gắn nhầm sang `DOM_MATH` và chuẩn hóa lại bộ 3 đề `DIAGNOSTIC` (90 câu hỏi độc nhất, đúng phân bổ 5 Việt + 5 Anh + 8 Toán + 6 KHTN + 6 KHXH).
  - Chuẩn hóa toàn bộ 19 câu hỏi chứa bảng dạng văn bản sang chuẩn Markdown Table.
