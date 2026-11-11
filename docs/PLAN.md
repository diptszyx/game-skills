# Kế hoạch: bộ công cụ làm game nhanh (cập nhật 2026-11-11)

Chi tiết kỹ thuật (spec) viết sau.

## 1. Mục tiêu
Khi có ý tưởng game mới, người làm game chỉ cần nghĩ gameplay. Phần xung quanh và hình ảnh được làm sẵn. Bộ công cụ có ba phần: khung game, xưởng phong cách, bộ skill.

## 2. Ba phần của bộ công cụ

### Khung game
Phần code dùng chung, lấy từ chỗ giống nhau của hai game đã làm:
- Màn hình chính
- Cửa hàng
- Thưởng hằng ngày
- Cài đặt
- Tạm dừng
- Thắng
- Thua
- Hồi sinh
- Màn hình tải
- Lưu vàng, tim, booster
- Âm thanh
- Rung
- Ô cắm quảng cáo (giả)
- Ô cắm mua hàng (giả)

Khung phục vụ cả game 2D lẫn 3D: giao diện luôn là lớp 2D, gameplay bên dưới là gì cũng được. Hình ảnh không gắn cứng vào khung mà thay được, nên có thể viết code khung trước với hình tạm, rồi thay hình thật khi phong cách đã chốt.

### Xưởng phong cách
Quy trình tạo toàn bộ hình ảnh cho các màn hình của khung và cho gameplay:
- Nhập ý tưởng game; AI vẽ 3 đến 4 phong cách khác nhau để chọn (khi đã chọn concept thì AI sinh phong cách theo concept đó).
- Chọn xong, phong cách được khóa thành "bảng quy định" (màu, kiểu nút, bo góc, chữ).
- AI sinh hình theo bảng: nút, bảng thông báo, chữ, nền, biểu tượng, cả đồ vật 3D.
- Khung nhận hình và tự mặc phong cách đó.

### Bộ skill
Hướng dẫn cho trợ lý AI, mỗi skill một việc:
1. **Làm việc với AI:** đầu phiên đọc gì, giao việc thế nào, canh việc chạy nền, ghi bài học ngay lúc gặp lỗi, khi nào được tự chạy không cần hỏi.
2. **Kiểm tra kết quả:** chạy kiểm tra tự động, chụp màn hình so với ảnh mẫu đã duyệt, thử trên điện thoại; chưa kiểm thì không báo xong.
3. **Phong cách và hình AI:** cách chọn, khóa phong cách và viết yêu cầu vẽ.
4. **Giao diện 2D:** bố cục, chữ, nút, bảng thông báo.
5. **Xử lý hình AI:** cắt nền, đóng gói, làm chuyển động.
6. **Hình 3D phong cách hoạt hình:** đồ vật, ánh sáng, chạy mượt trên máy yếu.
7. **Thiết kế màn chơi / câu đố:** luật, sinh màn, kiểm độ khó.
8. **Cảm giác chơi, âm thanh, kinh tế game.**
9. **Phát hành.**

## 3. Làm một game mới sẽ thế nào
1. Nói ý tưởng game.
2. AI vẽ 3 đến 4 phong cách khác nhau; chọn một.
3. Phong cách được khóa lại thành bảng quy định.
4. AI sinh hình theo bảng; duyệt vài ảnh mẫu của từng màn hình.
5. Khung nhận hình, các màn hình dựng xong và chạy được.
6. Viết riêng phần gameplay.
7. Chụp từng màn hình, so với ảnh mẫu đã duyệt, thử trên điện thoại.

## 4. Các giai đoạn xây
| # | Việc | Xong khi nào |
|---|---|---|
| A | So sánh phần xung quanh của hai game đã làm, từng chức năng một | Có bảng "chức năng nào cả hai game đều có / chỉ một game có", kiểm lại ngẫu nhiên vài mục |
| B | Chốt danh sách thứ đưa vào khung | Danh sách được duyệt |
| C | Xưởng phong cách | Thử với một ý tưởng giả: ra 3-4 phong cách, chọn được một, ra đủ bộ hình đồng nhất |
| D | Code khung (lưu, vàng / tim, cài đặt, âm, rung, quảng cáo / mua hàng giả, các màn hình dùng hình tạm) | Chạy đúng, có kiểm tra tự động, không lỗi; chạy thử trên điện thoại |
| E | Thay hình tạm bằng hình của xưởng phong cách | Mỗi màn hình đúng ảnh mẫu đã duyệt, ở 2 kiểu màn hình điện thoại |
| F | Thử bằng một game mới nhỏ (3 màn) | Chạy được trên điện thoại; ghi số giờ thật để biết tiết kiệm bao nhiêu |
| G | Viết bộ skill | Mỗi skill qua thử nghiệm: tình huống nên dùng, không nên dùng, so với không có skill |

Thứ tự: A → B, rồi C và D làm song song (C là vẽ ảnh, D là code, không đụng nhau) → E → F. Hai skill đầu (làm việc với AI, kiểm tra kết quả) không cần chờ khung, viết ngay sau B, dùng thử vài phiên rồi mới viết tiếp. Mỗi bước xong được kiểm bằng số hoặc ảnh; không đạt thì dừng lại.
