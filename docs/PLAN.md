# Kế hoạch: bộ công cụ làm game nhanh (cập nhật 2026-11-11)

Bản đọc cho người. Chi tiết kỹ thuật (spec) viết sau; bản nháp kỹ thuật cũ nằm ở `SPEC_DRAFT.md`.

## 1. Mục tiêu
Khi có ý tưởng game mới, bạn chỉ cần nghĩ gameplay. Phần còn lại làm sẵn:
- **Khung game:** các thứ game nào cũng cần, lấy từ phần giống nhau của game-3 và game-5. Gồm: màn hình chính, cửa hàng, thưởng hằng ngày, cài đặt, tạm dừng, thắng, thua, hồi sinh, màn hình tải; lưu vàng / tim / booster; âm thanh và rung; ô cắm quảng cáo và mua hàng giả.
- **Xưởng phong cách:** nhập ý tưởng game, AI vẽ ra vài phong cách để bạn chọn. Chọn xong, AI sinh toàn bộ hình theo phong cách đó (nút, bảng, chữ, nền, biểu tượng, cả đồ vật 3D) và khung tự mặc phong cách đó.
- **Bộ skill:** hướng dẫn để Claude làm đúng quy trình, không lặp lỗi cũ (báo xong khi chưa so ảnh, quên canh việc đang chạy, tự bịa số...).

Khung phục vụ cả game 2D lẫn 3D: giao diện luôn là lớp 2D, gameplay bên dưới là gì cũng được.

## 2. Làm một game mới sẽ thế nào
1. Bạn nói ý tưởng game.
2. AI vẽ 3 đến 4 phong cách khác nhau; bạn chọn một.
3. Phong cách được khóa lại thành một "bảng quy định" (màu, kiểu nút, bo góc, chữ).
4. AI sinh hình theo bảng đó; bạn duyệt vài ảnh mẫu của từng màn hình.
5. Khung nhận hình, các màn hình dựng xong và chạy được.
6. Viết riêng phần gameplay.
7. Chụp từng màn hình, so với ảnh mẫu bạn đã duyệt, thử trên điện thoại.

## 3. Các giai đoạn xây
Người làm: **Claude** điều phối, đọc, kiểm; **Codex** viết mã, vẽ ảnh AI, chạy Unity; **bạn** chọn, duyệt, thử trên điện thoại.

| # | Việc | Xong khi nào | Ai |
|---|---|---|---|
| A | So sánh phần xung quanh của game-3 và game-5, từng chức năng một | Có bảng "chức năng nào hai game đều có / chỉ một game có"; Claude kiểm lại ngẫu nhiên vài mục | Claude |
| B | Chốt danh sách thứ đưa vào khung | Bạn duyệt danh sách | Bạn |
| C | Xưởng phong cách | Thử với một ý tưởng giả: ra 3-4 phong cách, bạn chọn một, ra đủ bộ hình đồng nhất và bạn thấy ổn | Codex vẽ, Claude viết yêu cầu, bạn chọn |
| D | Chức năng nền của khung (lưu, vàng / tim, cài đặt, âm, rung, quảng cáo / mua hàng giả) | Chạy đúng, có kiểm tra tự động, không lỗi; chạy thử trên điện thoại | Codex, Claude kiểm |
| E | Các màn hình của khung, mặc được phong cách | Mỗi màn hình đúng ảnh mẫu bạn duyệt, ở 2 kiểu màn hình điện thoại | Codex, bạn duyệt |
| F | Thử bằng một game mới nhỏ (3 màn) | Chạy được trên điện thoại; ghi số giờ thật để biết tiết kiệm bao nhiêu | Cả ba |
| G | Viết bộ skill | Mỗi skill qua thử nghiệm: tình huống nên dùng, không nên dùng, so với không có skill | Claude |

Thứ tự: A → B, rồi C và D làm song song (C là vẽ ảnh, D là mã, không đụng nhau) → E → F. Hai skill đầu (quy trình làm việc và kiểm tra kết quả) không cần chờ khung, viết ngay sau B, dùng thử vài phiên rồi mới viết tiếp. Mỗi bước xong là kiểm bằng số hoặc ảnh; không đạt thì dừng và báo bạn.

## 4. Cần bạn quyết
- [ ] Duyệt danh sách thứ đưa vào khung ở mục 1 (thêm / bớt màn nào?).
- [ ] Làm chức năng nền (D) trước rồi đo, mới làm tiếp màn hình (E)? Khuyến nghị: có, vì mới có 2 game để so.
- [ ] Bắt đầu bước A ngay?

## 5. Điều chưa biết
- **Tiết kiệm bao nhiêu giờ: chưa đo.** Chỉ biết sau bước F.
- **AI có vẽ ra bộ hình nhất quán không:** chưa thử trong công cụ này. Bước C để kiểm; hình AI hay lệch phong cách hoặc viền xấu khi cắt.
- Mới có 2 game để so, cùng thể loại và đều là bản clone. Game mới có thể khác dự kiến, nên chỉ đưa vào khung phần thật sự giống nhau.
- Hình của hai game cũ lấy từ game gốc nên **không dùng lại**. Chỉ dùng lại cách tổ chức và logic.
- Phần chỉ có ở một game (nhiệm vụ, ưu đãi xoay vòng, nhảy mây...) chưa đưa vào khung.

## 6. Ghi chú cho phiên làm việc (dành cho AI)
- Trạng thái: chưa viết khung hay skill nào. Việc kế: mục 4, rồi bước A.
- Đọc tiếp: `SKILL_INVENTORY.md` (182 bài học), `SKILL_AUTHORING_OFFICIAL.md` (quy tắc viết skill), `SPEC_DRAFT.md`.
- Chỉ đọc, không sửa `game-3`, `game-5`. Repo này được tự commit và push (đường dẫn cụ thể; không `reset --hard`, `pull --rebase`, force push). Vẫn hỏi cho nhánh thật: đổi hướng đã chốt, tiêu tiền, xóa việc của bạn, art / âm / cảm giác bạn phải duyệt.
- Plan viết cho người đọc: ngắn, không thuật ngữ; chi tiết để spec riêng.
