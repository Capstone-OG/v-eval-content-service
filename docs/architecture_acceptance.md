# BÁO CÁO NGHIỆM THU VÀ THẤU HIỂU KIẾN TRÚC (ARCHITECTURE ACCEPTANCE) - CONTENT SERVICE

## 1. TỔNG QUAN DỊCH VỤ
- **Tên Dịch Vụ**: V-Eval Content Service (Ngân hàng câu hỏi & Đề thi).
- **Cổng Dịch Vụ**: `5249`.
- **Kiến Trúc**: Clean Architecture / Microservice.

## 2. KIẾN TRÚC DỮ LIỆU & SCHEMA
- Quản lý Ngân hàng câu hỏi, Ma trận đề thi, và Các môn học/chủ đề theo schema PostgreSQL đồng bộ với `VACT_SCHEMA_V2_ERD`.
- Tích hợp API tạo và quản lý đề thi linh hoạt cho Giảng viên/Quản trị viên.

## 3. KẾT QUẢ NGHIỆM THU
- Docker Build: Multi-Stage Build .NET 9 hoạt động ổn định.
- Swagger UI API Documentation sẵn sàng trên cổng `5249`.
