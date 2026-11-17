# Kế hoạch: bộ công cụ làm game nhanh (cập nhật 2026-11-13)

Khi có ý tưởng game mới, người làm game chỉ cần nghĩ gameplay. Phần xung quanh và hình ảnh được làm sẵn. Các game làm sau chủ yếu thuộc thể loại hybrid-casual, nên phần khung gần như không đổi giữa các game. Chi tiết kỹ thuật (spec) viết sau.

## Bộ công cụ làm việc thế nào

| Bước | Việc | Làm bằng |
|---|---|---|
| 1 | Nói ý tưởng game | |
| 2 | Chọn phong cách | Xưởng phong cách: AI vẽ 3-4 phong cách để chọn |
| 3 | Sinh toàn bộ hình | Hình 2D (nút, bảng, chữ, nền): AI vẽ. Đồ vật 3D: code Blender |
| 4 | Dựng các màn hình | Khung game làm sẵn, tự mặc phong cách đã chọn |
| 5 | Viết gameplay | Riêng cho từng game |
| 6 | Kiểm tra | Chụp màn hình so với ảnh mẫu đã duyệt, thử trên điện thoại |

Bộ skill là hướng dẫn cho trợ lý AI trong suốt quá trình này.

Khung dùng được cho cả game 2D lẫn 3D vì giao diện luôn là lớp 2D. Hình không gắn cứng vào khung, nên viết code khung trước với hình tạm rồi thay hình thật sau.

## Khung game gồm những gì

| Nền tảng và dữ liệu | Màn hình | Bảng thông báo |
|---|---|---|
| Lưu dữ liệu | Màn hình chính | Thắng |
| Vàng, tim, booster | Cửa hàng | Thua |
| Âm thanh | Điểm danh ngày | Hồi sinh |
| Rung | Nhiệm vụ ngày | Tạm dừng |
| Cài đặt | Bảng xếp hạng (Unity Leaderboards; adapter thật trong `com.indiegame.shell.leaderboards`) | Rời ván |
| Đa ngôn ngữ | Màn hình tải | |
| Vùng an toàn (tai thỏ) | | |
| Xem quảng cáo nhận thưởng (giao diện bất đồng bộ; bản giả cho test, bản thật AdMob trong package `com.indiegame.shell.admob`) | | |
| Mua hàng (giả) | | |
| Thông báo đẩy | | |
| Cấu hình từ xa | | |

Chưa đưa vào khung (mới thấy ở một game): hồ sơ người chơi, chọn booster trước ván, nhiệm vụ sự kiện, ưu đãi xoay vòng, Sky Jump. Sẽ cân nhắc khi lặp lại ở game thứ ba.

## Bộ skill
Skill là hướng dẫn ngắn cho trợ lý AI, chỉ được mở ra khi việc đang làm khớp. Sáu skill làm theo thứ tự, mỗi skill chỉ viết khi đã có bài học nguồn từ hai game trước:

| Skill | Làm gì | Viết khi nào |
|---|---|---|
| Làm việc với AI | Đầu phiên đọc gì, giao việc, canh việc chạy nền, ghi bài học ngay lúc gặp lỗi | Đầu tiên |
| Kiểm tra kết quả | Kiểm tra tự động, chụp màn hình so ảnh mẫu, thử điện thoại; chưa kiểm thì không báo xong | Đầu tiên |
| Phong cách và hình AI | Chọn, khóa phong cách; viết yêu cầu vẽ; cắt nền, đóng gói hình | Cùng bước C |
| Giao diện 2D | Bố cục, chữ, nút, bảng thông báo | Cùng bước C |
| Đồ vật 3D bằng Blender | Dựng từ ảnh đã duyệt, kiểm nhiều góc, màu kiểu hoạt hình, chạy mượt trên máy yếu | Cùng bước C |
| Dùng khung game | Cách dựng một game mới trên khung | Sau bước D |

Chưa viết, cần nghiên cứu thêm: gameplay hybrid-casual; cảm giác chơi, âm thanh, kinh tế; phát hành.

## Các bước xây (trạng thái đo được, 2026-11-13)

| Bước | Việc | Xong khi | Trạng thái |
|---|---|---|---|
| A, B | So sánh hai game, chốt danh sách khung | Danh sách được duyệt | Xong |
| D | Code khung (màn hình, popup, tab, top bar, vùng chơi) | Shell test xanh, chạy trên A07 | Gần xong: shell EditMode 76 + PlayMode 36, pilot EditMode 325 + PlayMode 9 (số do người làm game báo, chưa chạy lại ở phiên này), A07 chạy; âm thanh + rung đã nối (D1 xong, nghe thử trên máy chờ bạn). Font đã xong: Baloo 2 ExtraBold là mặc định cố định qua `ShellFont`, shader SDF cho mobile, cổng tương phản chữ `MinTextContrast` 4.5. Popup đã sửa: vùng bấm đóng thật, nút 400x76 căn giữa, tiêu đề căn giữa, nhãn ngoài popup mặc định dùng `backdropText` (commit fbb51c5, f101230, 4f1dcc1, 62aeceb, b80c71a). Thiếu: thông báo đẩy, lệnh chụp montage qua ShellApp thật, mẫu game mới + đo thời gian (F1) |
| C | Xưởng phong cách | Bộ hình đồng nhất, hết lỗi art | Một nửa: có `kit_v5`; còn ô trắng trong icon, nút xanh lá, khung panel đôi trong `panel_popup.png`, hình xấu (băng, cá, hải cẩu, bg_play). Cần bạn quyết có giao Codex vẽ lại không (font đã xong, xem bước D) |
| E | Thay hình tạm, so ảnh mẫu | Cổng so ảnh chụp với `reference_chunky.png` xanh ở 2 tỉ lệ | Cổng đã viết (`tools/compare_shot`, selftest qua). Chưa có ảnh chuẩn thật: mockup quá khác game nên không dùng làm chuẩn chặt; cần bạn duyệt một ảnh chụp thật làm chuẩn. Chưa có lệnh chụp montage qua ShellApp thật |
| F | Game mới nhỏ 3 màn dựng từ tool | Chạy trên A07, có số giờ thật | Đã thử hai lần (`docs/F1_REPORT.md`): F1 ~3 phút đến chạy trên A07, F2 ~2,3 phút đến gate PlayMode xanh (không phone, chưa project mới, chưa style mới). Còn thiếu: hàm chạm thật dùng chung cho test, test chạm thật chập chờn |
| G1, G2 | 6 skill | Mỗi skill qua 3 tình huống (gồm 1 không nên kích hoạt) + so với không skill | Đã rà tĩnh (`docs/skill_eval/`). Đã sửa 2026-11-13: using-game-shell (luật 6 qua `ShellApp.Boot`, số cổng 76/36) và building-3d-objects (viết lại theo blender-mcp). Chưa chạy thử nghiệm với agent thật (chưa kiểm hai bản sửa) |
| H | Cài hook / skill toàn cục | Bạn đồng ý | Chưa, cần bạn duyệt |

## Tiêu chí "tool xong" (đề xuất, chờ bạn duyệt)
1. Game mới 3 màn chạy trên A07 trong ≤ X giờ làm việc (X đo ở F1, chưa có).
2. Mọi màn của game đó qua cổng so ảnh E1 (ngưỡng ghi trong script).
3. Shell EditMode + PlayMode xanh, `error CS` = 0.
4. 6 skill qua kiểm G.

## Việc tách song song (2026-11-13)
Chỉ một tiến trình Unity cùng lúc. Agent chỉ viết mã / đọc dữ liệu, không chạy Unity; Claude chạy Unity một lần mỗi đợt.
- E1: script so ảnh (Python, không Unity).
- D1: dịch vụ âm thanh + rung trong shell (C#, file mới).
- G: rà 6 skill, viết kịch bản kiểm (chỉ đọc + viết docs).
- Việc phụ của game-2-v2 (hiệu ứng thắng, màn thử cơ chế) không nằm trong tiêu chí xong tool.
