# NHẬT KÝ CẬP NHẬT (UPDATE LOG) - CONTENT SERVICE

## [11/11/2026] - Chuyển Nhánh Main, Cấu Hình Kestrel ListenAnyIP & Đóng Gói Docker Container Cho Hệ Thống V-Eval

- **Cấu Hình Kestrel Lắng Nghe Đa Giao Diện Mạng ([`Program.cs`](./V-Eval-Content_Service.API/Program.cs))**:
  - Chuyển đổi từ cấu hình `options.ListenLocalhost(...)` sang `options.ListenAnyIP(...)`:
    - Cổng `5249`: Lắng nghe HTTP/1 phục vụ toàn bộ REST API quản lý cây dàn kiến thức (`Curriculum`), ngân hàng câu hỏi (`Questions`), đề thi mô phỏng (`MockExams`) và tài liệu học tập (`Materials`).
    - Cổng `5250`: Lắng nghe HTTP/2 phục vụ dịch vụ gRPC Server cho các microservice nội bộ kết nối trực tiếp.
  - Thiết lập dung lượng nhận dữ liệu tối đa 250MB (`MaxRequestBodySize = 262_144_000`) hỗ trợ nạp tài liệu và giáo trình SGK dung lượng lớn.
  - Cho phép Kestrel chấp nhận các kết nối liên container từ `veval_network` trong Docker Compose và kết nối từ bên ngoài máy chủ.

- **Chuẩn Hóa Đóng Gói Docker Production ([`Dockerfile`](./Dockerfile), [`docker-compose.yml`](../../docker-compose.yml))**:
  - Cập nhật chỉ thị `EXPOSE 5249 5250` và biến môi trường `ENV ASPNETCORE_URLS=http://+:5249;http://+:5250` trong Dockerfile.
  - Cập nhật cấu hình Docker Compose ánh xạ hai cổng `5249:5249` và `5250:5250`, đồng bộ URL gRPC với Practice Service và URL REST với Gateway cluster.

- **Kiểm Thử & Nghiệm Thu Trực Tuyến**:
  - `dotnet build`: Biên dịch thành công 100% với .NET 9 SDK (0 Error, 0 Warning).
  - Docker Image `capstone-v-eval-content-service:latest` build thành công trong 117.9 giây.
  - Container `v_eval_content_service` khởi chạy ở trạng thái `Up`, phục vụ dữ liệu trực tiếp:
    - Endpoint `https://api.pickic.io.vn/api/content/curriculum` phản hồi HTTP 200 kèm trọn bộ cây dàn kiến thức 4 miền năng lực chuẩn hóa.
