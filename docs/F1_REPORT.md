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

## F2: game thử thứ hai, "Sum Ten" (2026-11-17)
Game thử: chọn các ô cho tổng bằng mục tiêu (10, 14, 17), tối đa 3 lần sai, 3 màn. Nằm trong `game-2-v2/Assets/SumTen/`. Dựng sau khi thêm `ShellGameConfig` + `ShellAppConfig.Standard` (việc Codex, 11:29 đến ~11:31, gate shell xanh: EditMode 110, PlayMode 45).

### Số đo (đồng hồ máy)
- Từ lúc bắt đầu viết game (11:35:05) đến gate PlayMode có chơi 3 màn bằng chạm thật qua UI xanh: **11:37:23, ~2,3 phút**, gồm 5 lần chạy gate (2 lần hỏng biên dịch hoặc test do lỗi của tôi: thiếu `using`, tính nhầm tổng trong test).
- Không tính: thời gian nghĩ luật, tạo project mới (ổ đĩa còn 5,8 GB, một `Library/` mới ~4,6 GB), chạy trên A07 (bạn cấm dùng điện thoại), style mới (dùng `PilotChunkyStyle`).
- Mã game: 81 dòng app + 32 dòng luật + 4 file JSON; `Initialize` còn 8 dòng (TapOrder cũ 21 dòng, chưa chuyển sang `Standard`).

### Chỗ tool còn thiếu thấy khi làm
1. Hàm `Click` chạm thật cho test (~30 dòng) phải chép từ game này sang game khác (đã 3 bản: pilot, TapOrder, SumTen). Nên có sẵn trong shell.
2. Phải nhớ hai `using` (`IndieGame.Shell`, `IndieGame.Shell.Unity.Screens`) cho `IGameSessionActions`.
3. Test chạm thật chập chờn: trong 7 lần chạy PlayMode sau khi thêm SumTen, 3 lần có 1 đến 2 test hỏng (Pilot, TapOrder; lỗi "Button never became raycastable", "Expected 2 but was 1", NullReference), 4 lần 17/17. Trước khi thêm SumTen thấy 11/11 hai lần. Chưa tìm ra nguyên nhân gốc.

### Cập nhật 2026-11-17 (sau hàm chạm thật chung)
Package `com.indiegame.shell.testkit` (`RealPointer.Click`) thay 4 bản chép tay. Đo 10 lần PlayMode liên tiếp của game-2-v2 (17 test): trước khi chờ popup yên trước lúc nhấn 8/10 xanh (2 lần hỏng "timeout waiting for onClick"), sau khi sửa 10/10 xanh. Nguyên nhân gốc của 2 lần hỏng còn lại: chưa chứng minh được; giả thuyết có bằng chứng từ mã: popup đang mở phóng to quanh tâm nên nút dịch chỗ. Thông báo lỗi của `RealPointer` giờ in thêm tiêu điểm cửa sổ và trạng thái InputSystem để bắt lần hỏng kế.
