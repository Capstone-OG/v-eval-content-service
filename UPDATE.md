# Nhật Ký Cập Nhật (Update Log) - Content Service

## [15/09/2026] - Dockerize Content Service & Chuẩn Hóa Docker Compose
- **Dockerfile Multi-Stage .NET 9**:
  - Khởi tạo `Dockerfile` chuẩn cho Content Service (`V-Eval-Content_Service.API`) với cổng `5249`.
  - Kết nối chung mạng nội bộ `veval_network` trong `docker-compose.yml`.

## [14/09/2026] - Chuẩn Hóa Cấu Hình Production & Git Security
- **Khởi Tạo `appsettings.example.json`**:
  - Tạo file cấu hình mẫu chứa `ConnectionStrings` và `JwtSettings`.
- **Bảo Mật Git**:
  - Cập nhật `.gitignore` ẩn các file `appsettings.json` chứa Password thật.
