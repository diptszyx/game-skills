# F1: game mới nhỏ dựng trên tool (2026-11-13)

Game thử: **Tap Order** (chạm các số 1..N theo thứ tự trên lưới 3x3, 3 màn, tối đa 3 lần sai). Nằm trong `game-2-v2/Assets/TapOrder/` (asmdef riêng, không dùng mã Board2D).

## Số đo
- Từ lệnh đầu tiên đến chữ chơi được trên A07: **~3 phút** theo đồng hồ máy (12:18:10 bắt đầu, APK 12:20:30, chụp màn hình 12:20:56).
- Không tính: thời gian chọn ý tưởng và thiết kế luật (tôi đã nghĩ trước), thời gian tạo project mới.
- Mã game: ~150 dòng (luật 30, app + màn 120), 3 file JSON (config, en, vi), 1 asmdef.
- Kiểm: 4 test luật, 1 test chơi 3 màn bằng chạm thật qua UI (Play, chạm số theo thứ tự, popup Win, Next, về Main). Gate PlayMode 9/9, `error CS` 0. Trên A07: chạm 3 số, popup Victory hiện.

## Giới hạn của phép đo
- **Không tạo project Unity mới**: ổ đĩa còn 7,7 GB, một `Library/` mới tốn 4,6 GB. Chưa có mẫu project mới: phải copy `game-2-v2` tay.
- Đồng hồ tính cả thời gian chạy test và build (~1,5 phút), nên phần viết mã thực sự ngắn hơn.
- Style dùng lại `PilotChunkyStyle` của game-2: chưa phải một style mới.

## Chỗ tool còn thiếu (thấy khi làm)
1. **Không có mẫu game mới** (project + asmdef + boot + config + test): phải copy từ pilot. Đây là việc tool cần có.
2. **Chữ nội dung popup gần như vô hình** (chữ kem trên nền kem: "Your reward is ready", "Gold 20"). Lỗi của style/tool, ảnh `work/ref/t_win.png`.
3. **Chữ trạng thái màn chơi trắng trên nền pastel** khó đọc (cùng lỗi như game-2).
4. **Không có vai trò ô vuông** (tile): nút `Primary` bị kéo giãn 9-slice thành khối phình khi vuông 190 px.
5. Không có chỗ khai báo "game nào chạy" khi một project có hai game: tôi dùng file `game-selector.txt` tạm.

## Cập nhật 2026-11-13
Cổng của tool hiện: shell EditMode 76 + PlayMode 36, pilot EditMode 325 + PlayMode 9 (số báo từ người làm game, chưa chạy lại ở phiên này; bản trên đây ghi 9/9 PlayMode cho Tap Order). Font (Baloo 2 ExtraBold qua `ShellFont`) và cổng tương phản chữ (`MinTextContrast` 4.5) đã nằm trong tool, nên chỗ thiếu số 2 và 3 ở trên (chữ popup và chữ trạng thái khó đọc) đã có cổng tương phản nhằm bắt lỗi loại này; chưa chụp lại để xác nhận hai lỗi đó hết. Các chỗ thiếu số 1 (mẫu game mới), 4 và 5 chưa làm.
