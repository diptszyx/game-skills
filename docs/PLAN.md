# Kế hoạch: bộ công cụ làm game nhanh (cập nhật 2026-11-11)

Chi tiết kỹ thuật (spec) viết sau.

## 1. Mục tiêu
Khi có ý tưởng game mới, người làm game chỉ cần nghĩ gameplay. Phần còn lại làm sẵn:
- **Khung game:** các thứ game nào cũng cần, lấy từ phần giống nhau của game-3 và game-5. Gồm: màn hình chính, cửa hàng, thưởng hằng ngày, cài đặt, tạm dừng, thắng, thua, hồi sinh, màn hình tải; lưu vàng / tim / booster; âm thanh và rung; ô cắm quảng cáo và mua hàng giả.
- **Xưởng phong cách:** nhập ý tưởng game, AI vẽ ra vài phong cách để chọn. Chọn xong, AI sinh toàn bộ hình theo phong cách đó (nút, bảng, chữ, nền, biểu tượng, cả đồ vật 3D) và khung tự mặc phong cách đó.
- **Bộ skill:** hướng dẫn để trợ lý AI làm đúng quy trình, không lặp lỗi cũ (báo xong khi chưa so ảnh, quên canh việc đang chạy, tự bịa số...).

Khung phục vụ cả game 2D lẫn 3D: giao diện luôn là lớp 2D, gameplay bên dưới là gì cũng được.

## 2. Làm một game mới sẽ thế nào
1. Nói ý tưởng game.
2. AI vẽ 3 đến 4 phong cách khác nhau; chọn một.
3. Phong cách được khóa lại thành một "bảng quy định" (màu, kiểu nút, bo góc, chữ).
4. AI sinh hình theo bảng đó; duyệt vài ảnh mẫu của từng màn hình.
5. Khung nhận hình, các màn hình dựng xong và chạy được.
6. Viết riêng phần gameplay.
7. Chụp từng màn hình, so với ảnh mẫu đã duyệt, thử trên điện thoại.

## 3. Các giai đoạn xây
| # | Việc | Xong khi nào |
|---|---|---|
| A | So sánh phần xung quanh của hai game đã làm, từng chức năng một | Có bảng "chức năng nào cả hai game đều có / chỉ một game có"; kiểm lại ngẫu nhiên vài mục |
| B | Chốt danh sách thứ đưa vào khung | Danh sách được duyệt |
| C | Xưởng phong cách | Thử với một ý tưởng giả: ra 3-4 phong cách, chọn một, ra đủ bộ hình đồng nhất |
| D | Chức năng nền của khung (lưu, vàng / tim, cài đặt, âm, rung, quảng cáo / mua hàng giả) | Chạy đúng, có kiểm tra tự động, không lỗi; chạy thử trên điện thoại |
| E | Các màn hình của khung, mặc được phong cách | Mỗi màn hình đúng ảnh mẫu được duyệt, ở 2 kiểu màn hình điện thoại |
| F | Thử bằng một game mới nhỏ (3 màn) | Chạy được trên điện thoại; ghi số giờ thật để biết tiết kiệm bao nhiêu |
| G | Viết bộ skill | Mỗi skill qua thử nghiệm: tình huống nên dùng, không nên dùng, so với không có skill |

Thứ tự: A → B, rồi C và D làm song song (C là vẽ ảnh, D là mã, không đụng nhau) → E → F. Hai skill đầu (quy trình làm việc và kiểm tra kết quả) không cần chờ khung, viết ngay sau B, dùng thử vài phiên rồi mới viết tiếp. Mỗi bước xong là kiểm bằng số hoặc ảnh; không đạt thì dừng lại.
