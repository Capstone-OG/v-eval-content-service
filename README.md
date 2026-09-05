# V-Eval-Content_Service

**V-Eval-Content_Service** là microservice quản lý nội dung học tập, ngân hàng đề thi thử (Mock Exams), ngân hàng câu hỏi (Questions), chùm bài đọc hiểu (Passages) và cây phân cấp kỹ năng (Skills Hierarchy) trong hệ thống luyện thi đánh giá năng lực **V-Eval**.

* **Nhánh theo dõi chính**: `main`
* **Repository Remote**: `https://github.com/Capstone-OG/v-eval-content-service.git`

---

## 🏗️ Kiến trúc Công nghệ

* **Framework**: .NET 9 Web API (ASP.NET Core Minimal APIs).
* **Mô hình**: Clean Architecture 4 tầng (`Domain`, `Application`, `Infrastructure`, `API`).
* **Mẫu thiết kế**: CQRS (Command Query Responsibility Segregation) kết hợp `MediatR` và `FluentValidation`.
* **Cơ sở dữ liệu**: PostgreSQL Cloud (Supabase), quản lý trong schema riêng biệt `content`.
* **ORM**: Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`).

---

## 🚀 Các Tính Năng Cốt Lõi

1. **Nhập Đề thi Tự động từ AI Engine (`POST /api/content/exams/import`)**:
   * Tiếp nhận payload JSON chuẩn bóc tách từ 120 câu hỏi PDF (gồm LaTeX, lựa chọn A-B-C-D, đáp án đúng, giải thích, đường dẫn hình ảnh `ImageUrl`).
   * Hỗ trợ lưu trữ hình ảnh cho cả câu hỏi đơn lẻ và chùm bài đọc hiểu: tự động nhúng cú pháp Markdown `![Hình minh họa](image_url)` vào `ContentLatex` để hiển thị đồng bộ trên mọi thiết bị học tập.
   * Tự động nhận diện và tạo kỹ năng mới (`skills`) theo lĩnh vực năng lực tương ứng nếu chưa tồn tại trong cơ sở dữ liệu.
   * Lưu trữ quan hệ phân cấp giữa đề thi, câu hỏi và chùm bài đọc hiểu (`passages`) với tính toàn vẹn giao dịch cao.
2. **Truy vấn Danh sách Đề thi (`GET /api/content/exams`)**:
   * Cung cấp danh sách tóm tắt đề thi (ID, tiêu đề, thời lượng, số câu hỏi, ngày tạo) phục vụ giao diện chọn đề.
3. **Lấy Chi tiết Đề thi Toàn vẹn (`GET /api/content/exams/{id}`)**:
   * Nạp đầy đủ cây dữ liệu gồm đề thi, câu hỏi đơn, chùm bài đọc, công thức LaTeX và kỹ năng phục vụ KaTeX Viewer.
4. **Xóa Đề thi An toàn (`DELETE /api/content/exams/{id}`)**:
   * Tự động xóa liên kết trong `exam_questions` và các câu hỏi, chùm bài đọc thuộc đề thi, tránh rác dữ liệu.

---

## 📚 Hệ Thống Tài Liệu Kỹ Thuật (Documentation)

Chi tiết cấu trúc và hướng dẫn kỹ thuật được lưu trữ trong thư mục [`docs/`](./docs):

* 📘 [**Kiến trúc Hệ thống & Cơ sở Dữ liệu** (`docs/content_service_architecture.md`)](./docs/content_service_architecture.md): Sơ đồ quan hệ thực thể ERD, thiết kế schema Supabase, luồng liên thông AI Engine.
* 📋 [**Đặc tả API Quản lý Đề thi** (`docs/exam_management_api.md`)](./docs/exam_management_api.md): Đặc tả chi tiết các endpoints, JSON payload mẫu, mã lỗi và cấu hình CORS.
* 📅 [**Nhật ký & Kế hoạch Phát triển** (`docs/daily_process_and_planning.md`)](./docs/daily_process_and_planning.md): Theo dõi tiến độ từng ngày, danh sách task hoàn thành và lộ trình phát triển.
* 📝 [**Lịch sử Cập nhật** (`UPDATE.md`)](./UPDATE.md): Bản ghi chi tiết các bản phát hành và cập nhật hệ thống.

---

## ⚙️ Hướng dẫn Khởi chạy Cục bộ (Local Development)

### 1. Yêu cầu Môi trường
* .NET SDK 9.0 trở lên.
* Kết nối Internet để truy cập cơ sở dữ liệu Supabase PostgreSQL.

### 2. Cấu hình Chuỗi Kết nối
Tạo tệp `V-Eval-Content_Service.API/appsettings.Development.json` (tệp này đã được cấu hình `.gitignore` để chống lộ lọt secret):

```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.<your-project-ref>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 3. Khởi chạy Dịch vụ
```powershell
cd "V-Eval-Content_Service.API"
dotnet run
```
Dịch vụ sẽ khởi động tại cổng mặc định `http://localhost:5249` (hoặc cổng cấu hình trong `Properties/launchSettings.json`).
Truy cập Swagger UI tại: `http://localhost:5249/swagger` (nếu đang ở chế độ Development).
