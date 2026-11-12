# Cổng kiểm "hình có bị AI quá không" (bản nháp để review, 2026-11-12)

Mục đích: trước khi đưa hình AI vẽ cho người làm game xem hoặc đưa vào game, một cổng tự động cho biết hình có mang "cảm giác AI" (nhìn là biết ảnh AI, không giống game thật) hay không. Cổng chỉ lọc lỗi; người làm game vẫn là người quyết cuối.

## Trạng thái thật
- Chưa có thước đo khách quan cho "cảm giác AI". Các công cụ phát hiện AI đo nguồn gốc ảnh, không đo cảm giác.
- Bản 1 (người chấm là mô hình thị giác, chấm từng ảnh riêng, 5 tiêu chí) đo trên 17 ảnh mù (5 màn game thật Colony Flow / Hole It, 12 màn AI của dự án): **độ phân biệt (AUC) 0,68, chưa đạt**. Chín trên 12 màn AI bị chấm như game thật. Phân biệt từng tiêu chí: chất liệu 0,63; độ bóng 0,60; thứ bậc 0,53; "chung chung" 0,38 (ngược); "bối cảnh ghép" 0,30 (ngược).
- Số đo thô "mật độ cạnh" từng được dùng làm mục tiêu, nhưng bị bác: bảng được ChatGPT chấm ít AI nhất lại có mật độ cạnh cao nhất.
- Mẫu nhỏ, một người chấm, ảnh AI và ảnh thật khác tỉ lệ khung (đã đệm cho bằng nhau, vẫn nhận ra được).

## Cách cổng chạy (bản 2, chưa chạy)
1. Ảnh cần kiểm được cắt theo từng màn (home, gameplay, popup...). Mỗi màn được so với 3 màn game thật **cùng loại** (ảnh thật chụp trên điện thoại).
2. Người chấm không biết ảnh nào là ảnh nào (mù) và xem từng cặp: ảnh cần kiểm cạnh một ảnh thật.
3. Hai người chấm độc lập (Claude và một mô hình khác); lấy điểm trung bình, hai người lệch nhau quá 2 điểm thì ghi "không chắc".
4. Mỗi tiêu chí chấm riêng 1 đến 5 (5 là kém nhất), kèm một câu bằng chứng chỉ vào chỗ cụ thể trên ảnh. Không có một điểm gộp duy nhất.
5. Cổng hợp lệ khi: trên bộ ảnh đã biết nguồn, độ phân biệt (AUC) ≥ 0,85, và có một phần ảnh thật giữ riêng để kiểm chéo, không dùng làm ảnh chuẩn.

## Tiêu chí (nguồn: bài ChatGPT, bảy luật trong `knowledge/08-mau-va-anh-sang.md`, bài tìm web, số đo đã có)
| # | Tiêu chí | Hỏi gì | Có dấu hiệu AI khi |
|---|---|---|---|
| 1 | Chất liệu đồng bộ | Logo, nhân vật, nút, nền, thanh dưới có cùng chất liệu và kiểu viền không | Mỗi phần một kiểu (logo viền hoạt hình, nhân vật nhựa 3D, thanh dưới kiểu game khác) |
| 2 | Chi tiết có chọn lọc | Có nơi cố ý để đơn giản không | Mọi thứ đều bóng, tròn, mềm, sạch như nhau; không có chỗ nghỉ |
| 3 | Thứ bậc nhìn | Mắt có đi vào một chỗ rõ ràng không | Logo, nhân vật, nền, nút cùng tranh sự chú ý bằng màu và độ bóng |
| 4 | Ánh sáng nhất quán | Một hướng sáng, bóng đổ khớp | Sáng đều khắp, bóng sai hướng, phản chiếu không khớp cảnh |
| 5 | Một hệ UI | Nút, icon, viên chứa, chữ có cùng độ dày viền, bo góc, đổ bóng không | Mỗi nút một kiểu, nhiều nút tranh nhau là nút chính |
| 6 | Nhân vật có cá tính và liên quan lối chơi | Tư thế, biểu cảm có hợp với game không | Dáng đứng chung chung, không liên quan hành động chính |
| 7 | Chữ và lỗi chi tiết | Chữ đúng, tay chân, hình lặp, khối méo | Chữ lỗi, hình lặp giống hệt, cạnh tan chảy |
| 8 | Ghép từ mảnh hay một tranh | Giống giao diện ghép từ asset rời hay một tấm áp phích | Cảm giác "áp phích": đẹp nhưng không có logic bố cục |

Tiêu chí đã loại sau lần đo đầu: "chung chung/mẫu có sẵn" và "bối cảnh ghép" (điểm ngược chiều, game casual thật vốn mang nét chung). Tiêu chí 4, 7, 8 là mới, chưa đo.

## Cần review
- Tiêu chí còn thiếu hoặc thừa; cách hỏi từng tiêu chí đã rõ và đo được chưa.
- Có nên dùng so sánh cặp với ảnh thật hay chấm từng ảnh riêng.
- Ngưỡng AUC ≥ 0,85 và số ảnh hiệu chỉnh (hiện 5 thật, 12 AI) có đủ không; cần bao nhiêu game thật thêm.
- Cách tránh để người chấm nhận ra ảnh qua tỉ lệ khung hay chữ lẻ.

## Giới hạn đã biết
- Người chấm là mô hình, có thiên lệch và có thể đổi ý theo ngữ cảnh.
- Ảnh thật hiện chỉ từ hai game (Colony Flow, Hole It).
- Cổng không thay mắt người làm game.
