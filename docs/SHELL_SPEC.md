# Spec nháp: lõi khung (C# thuần) — sinh từ mã đã chạy, 2026-11-12

Nguồn sự thật là mã và test trong `framework/`; file này tóm tắt quyết định. Báo cáo từng job: `framework/REPORT_A.md`, `REPORT_B.md`, `REPORT_C.md`, `REPORT_T1b.md`, `REPORT_T6.md`. Số test: 137 (dotnet) + 19 (Unity EditMode).

## Quy ước chung
- Không UnityEngine trong Core. Thời gian qua `IClock` (UTC). Phần thưởng qua `Reward` + `IRewardGrantor.Grant(claimId, reward)`; cùng `claimId` chỉ cộng một lần.
- Mỗi module lưu MỘT phần (section) trong `SaveDocument`, có `Version = 1`. Phần thiếu → mặc định; phần hỏng / phiên bản lạ → module đó về mặc định kèm mã lỗi `CorruptSection`, không đụng module khác.
- Hàm đổi trạng thái trả đối tượng kết quả, không ném lỗi với đầu vào xấu. Constructor ném `ArgumentException` khi cấu hình sai (lỗi lập trình).
- Dịch vụ dùng tuần tự (không khóa đa luồng).

## Module
| Module | Phần lưu | Chính sách đã chốt |
|---|---|---|
| Lưu (`SaveStore`) | cả file | Ghi qua file tạm rồi thay thế, giữ `.bak`; đọc hỏng → thử `.bak`; phiên bản mới hơn → báo lỗi và không ghi đè; nâng cấp từng bước bằng hàm migration |
| Kinh tế (`EconomyService`) | `economy` | Vàng `long`, booster theo id; âm / tràn bị từ chối; giao dịch nhiều món là nguyên tử; sổ mã nhận thưởng |
| Tim (`LivesService`) | `lives` | Hồi 1 tim mỗi chu kỳ, tính lười theo `IClock`; đồng hồ lùi: không đổi gì (dấu mốc thời gian lưu cả khi nạp lại); tim vô hạn cộng dồn; đầy tim thì không đếm giờ |
| Điểm danh (`DailyCheckInService`) | `dailyCheckIn` | 5 ô, 2 bảng thưởng xoay theo ngày UTC; bỏ ngày thì giữ ô kế; mã nhận `checkin:<ngày>` |
| Nhiệm vụ ngày (`DailyQuestsService`) | `dailyQuests` | Danh sách từ cấu hình, chọn tuần hoàn xác định (không ngẫu nhiên); đặt lại 00:00 UTC; danh sách tuần đặt lại thứ Hai UTC; mã nhận `quest:<ngày>:<id>` |
| Thông báo (`NotificationPlanner`) | không | Làm mới tim TRƯỚC khi tính giờ đầy tim; danh sách trả về thay hoàn toàn kế hoạch cũ; giờ yên lặng UTC [bắt đầu, kết thúc) |
| Cài đặt (`SettingsService`) | `settings` | Âm thanh, nhạc, rung, ngôn ngữ; ngôn ngữ lạ → ngôn ngữ mặc định |
| Đa ngôn ngữ (`LocalizationService`) | không | Bảng khóa→chữ theo ngôn ngữ; rơi về ngôn ngữ mặc định rồi `[khóa]`, ghi lại khóa thiếu; format hỏng không ném lỗi |
| Cấu hình (`RemoteConfigService`) | `config` | Hợp lệ → dùng; hỏng → bản đệm hợp lệ cuối → mặc định; kết quả nói nguồn nào thắng |
| Ổ cắm (`ServiceSockets`) | không | Quảng cáo, mua hàng, nguồn cấu hình, bảng xếp hạng: interface + bản giả theo kịch bản; mua giả idempotent theo mã giao dịch |

## Lớp Unity (đã có)
Rung (`IHapticsPlayer`), âm thanh (`IAudioService`), vùng an toàn (`SafeAreaMath` + `SafeAreaFitter`), màn hình tải (máy trạng thái). Package: `com.indiegame.shell`.

## Rủi ro đã biết (chưa xử lý)
1. **Nguyên tử giữa module.** Mỗi module tự ghi phần của nó; nếu máy chủ gọi `Save` giữa chừng (ví dụ điểm danh đã đánh dấu nhận nhưng kinh tế chưa lưu) thì mất thưởng. Cách xử lý dự kiến: một lớp phối hợp ghi tất cả phần rồi gọi `SaveStore.Save` đúng một lần. Chưa có; việc này nên làm trước khi dựng màn hình.
2. Chưa kiểm trên IL2CPP chạy thật hay điện thoại (APK mới build, chưa cài).
3. Chưa kiểm đa luồng, chưa có SDK quảng cáo / mua hàng / bảng xếp hạng thật (chỉ bản giả).
4. Định nghĩa "bỏ ngày" của điểm danh và cách chọn nhiệm vụ là lựa chọn đơn giản nhất của Codex vì tài liệu im lặng (AMBIG trong REPORT_B); cần người làm game duyệt khi thấy cảm giác chơi.
