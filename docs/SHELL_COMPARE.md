# Bước A: so sánh phần xung quanh (vỏ) của game-3 và game-5 (2026-11-11)

Chỉ đọc, không sửa hai game. Đọc ở: game-3 commit `8997bab` (2026-11-05), game-5 commit `db3a328` (2026-11-11; có một thay đổi chưa commit ở `ObjectPhysicsDetection.cs`, là gameplay nên không ảnh hưởng bảng). Thêm game-2 chỉ cho bảng xếp hạng.

**Cách làm và giới hạn:** liệt kê toàn bộ file mã chạy (không test, không editor) của hai game, đọc chữ ký lớp và hàm công khai từng file vỏ, đếm tên bằng `grep`. Chưa đọc từng dòng thân hàm. Cột "kiểm lại" ghi chỗ đã kiểm bằng cách thứ hai.

## 1. Kết luận chính
1. **Logic và giao diện có độ tái sử dụng rất khác nhau.**
   - Logic: toàn bộ 26 file `Rules/` của game-5 là C# thuần, không dùng `UnityEngine` (kiểm bằng `grep`: 0/26). Đây là phần tái dùng được gần như nguyên.
   - Giao diện của cả hai game bám vào **tên node của prefab gốc** (ví dụ `"CoinCountText"`, `"ButtonAds"`, `"HeartFull-Text"`): khoảng 65 lần ở game-3 và 91 lần ở game-5 (đếm xấp xỉ bằng mẫu `Find/Text/Active("tên")`). Giao diện không dùng lại được; phải viết lại theo cấu trúc prefab của phong cách mới.
2. **Hai game lưu dữ liệu theo hai cách khác nhau:** game-3 dùng `PlayerPrefs` và lớp tĩnh `HomeState` (3 file chạm `PlayerPrefs`); game-5 dùng một lớp `SaveData` ghi file JSON (16 file dùng). Khung chỉ nên giữ một cách (quy chuẩn "một cách cho một việc"). Đề xuất theo game-5: có kiểm tra số âm, kiểm được bằng test, không phụ thuộc Unity.
3. **Cả hai đều chưa có đa ngôn ngữ** (không có mã `Localiz`/`Language`). Hybrid-casual thường cần; quy chuẩn chung (`../CLAUDE.md`, mục Definition of Done) cũng nhắc "mọi ngôn ngữ của game". Cần quyết định có đưa vào khung không.
4. **Game-5 không có màn hình tải riêng và không thấy xử lý vùng an toàn** (grep `SafeArea` 0 file), game-3 có cả hai (`LoadingScreen`, `HomeSafeArea`). Kit cũ `game-kit` đã có `UiSafeArea`.
5. **"Thưởng hằng ngày" ở hai game là hai thứ khác nhau:** game-3 là điểm danh 7 ô (`DailyRewards`); game-5 là nhiệm vụ theo ngày trong tuần (`DailyChallengeRules`) và không thấy điểm danh đăng nhập. Hai thứ này đều nằm trong danh sách khung.
6. **Bảng xếp hạng:** game-3 là giả (màn "đang tải" 0,5 giây rồi hiện danh sách dựng sẵn; không có dịch vụ nào trong `manifest.json`); game-5 không có; game-2 là thật.

## 2. Bảng chức năng
Cột "Kết luận": **Cả hai** = cả hai game có; **Một game** = chỉ một game có; **Khác cách** = cả hai có nhưng làm khác nhau. "Dùng lại" ghi phần nào lấy được.

### Nền tảng và dữ liệu
| Chức năng | game-3 | game-5 | Kết luận | Dùng lại |
|---|---|---|---|---|
| Lưu dữ liệu | `HomeState` (tĩnh, PlayerPrefs) | `Rules/SaveData` (JSON, có Save/Load) | Khác cách | Theo game-5 |
| Vàng | `HomeState` | `SaveData.AddGold/TrySpendGold` | Cả hai | Logic |
| Tim, hồi tim theo thời gian | `LifeRegen`, `HomeState.LoseLife` | `SaveData.RefillLives/TryLoseLife`, `SecondsToNextLife` | Cả hai | Logic (game-5 kiểm số âm tốt hơn) |
| Mua đầy tim bằng vàng | `FillHeartModal`, `HomeState.TryBuyFullLives` | `SaveData.TryBuyFullLives`, cửa hàng điều kiện `try_refill` | Cả hai | Logic |
| Số booster tồn | `HomeState.BoosterCount` | `SaveData.AddBooster/TryUseBooster` | Cả hai | Logic |
| Cài đặt âm / rung | `Audio/GameSettings` (PlayerPrefs) | `Audio/ISoundSettings` | Cả hai | Giao diện nhỏ `ISoundSettings` |
| Cấu hình từ xa | không có | `IRemoteConfig`, `PackagedRemoteConfig` (đọc JSON đóng gói) | Một game | Theo game-5 |
| Quảng cáo xem nhận thưởng | gọi lại `onWatchAd` trong `ModalLose` (không có lớp ổ cắm) | `IRewardedAds`, `FakeRewardedAds`, 5 vị trí quảng cáo | Cả hai (game-5 gọn hơn) | Theo game-5 |
| Mua hàng | `ShopPurchase.Buy` (giả, 16 dòng) | cửa hàng giả, `StorePrices` | Cả hai | Làm một ổ cắm mua hàng như ổ quảng cáo |
| Gỡ quảng cáo (No Ads) | `NoAdsModal`, trường `noAds` trong `ShopPack` | thành phần của cửa hàng | Cả hai | Logic |
| Thông báo đẩy | không có | `NotificationPlanner`, `INotificationScheduler` (bản ghi log) | Một game | Theo game-5 |
| Khóa tính năng theo level | `HomeFeatureLocks`, `BoosterBar.UnlockLevel` | `ButtonVisible/CanOpen(level)` trong từng `*Rules` | Cả hai | Logic |
| Đa ngôn ngữ | không có | không có | Không game nào | Cần quyết định |

### Âm thanh và rung
| Chức năng | game-3 | game-5 | Kết luận | Dùng lại |
|---|---|---|---|---|
| Âm thanh | `GameAudio` (Play, ChangeMusic), `BattleAudio` | `AudioManager`, `AudioRules` (giãn cách phát), `SoundCatalog` | Cả hai | Theo game-5 (tách luật thuần, bảng âm) |
| Rung | `Haptics.LightImpact` (52 dòng) | `Haptics.Play(HapticKind)` (62 dòng) | Cả hai, gần trùng | Theo game-5 |

### Màn hình
| Chức năng | game-3 | game-5 | Kết luận | Dùng lại |
|---|---|---|---|---|
| Màn hình chính | `HomeScreen`, `HomeTabs` (các trang vuốt), `HomeState` | `HomeStage`, `HomeView`, `HomeStageData`, `HomeBackdrop` | Cả hai | Chỉ ý tưởng: nhãn vàng / tim / level / nút chơi. Phần trang vuốt và trang trí là riêng từng game |
| Cửa hàng | `ShopCatalog` (gói cố định), `ShopPurchase` | `ShopView`, dữ liệu JSON `ShopData`, `StorePrices` | Cả hai | Dữ liệu cửa hàng dạng JSON theo game-5 |
| Điểm danh ngày | `DailyRewards`, `DailyRewardModal` | không thấy | Một game | Cần quyết định có vào khung |
| Nhiệm vụ ngày | không có | `DailyChallengeRules`, `DailyChallengeView` | Một game | Logic theo game-5 |
| Bảng xếp hạng | giả (`HomeLeaderboardPage`, `LeaderboardJumpButtons`) | không có | Một game (giả) | Dùng bản thật của game-2 (`UnityLeaderboardService`, `ILeaderboardService`) |
| Màn hình tải | `LoadingScreen`, `SceneRouter` | không thấy (chỉ cờ `loader.IsLoading` của màn chơi) | Một game | Theo game-3 |
| Vùng an toàn | `HomeSafeArea` | không thấy | Một game | Theo kit cũ `UiSafeArea` |
| Hồ sơ, hình đại diện, lọc từ chửi | không có | `ProfileRules`, `ProfileView`, `ProfanityFilter` | Một game | Chưa quyết (danh sách từ chửi của game-5 trống) |
| Chọn booster trước ván | không có | `PreGameSelection`, `PreGameView` | Một game | Chưa quyết |

### Bảng thông báo
| Chức năng | game-3 | game-5 | Kết luận | Dùng lại |
|---|---|---|---|---|
| Thắng | `ModalWin`, `ModalConfetti` | `ShellController.ShowWin`, hiệu ứng | Cả hai | Luồng |
| Thua, hồi sinh | `ModalLose` (xem quảng cáo / trả vàng), `ModalHost` | `reviveHandler`, `FailOfferView` | Cả hai | Luồng + ổ quảng cáo |
| Tạm dừng | `ModalPause` + công tắc âm / rung | `ShellController.PauseGame` | Cả hai | Luồng |
| Rời ván | `ModalLeave` | trong `ShellController` | Cả hai | Luồng |
| Điều phối bảng | `ModalHost` (lớp riêng, 201 dòng) | `ShellController` (610 dòng, gom hầu hết vỏ) | Khác cách | Theo game-3 (tách), tránh lớp 610 dòng |
| Hiệu ứng mở / nhấn | `ModalTween` (đường cong, thời gian) | `PopupEffect`, `ButtonPressEffect` | Cả hai | Gộp một cách |
| Khung hiện / ẩn nền | `ModalBackdrop` | `CanvasFadeEffect` | Cả hai | Gộp |

### Sự kiện chỉ có ở một game
| Chức năng | game-3 | game-5 | Kết luận |
|---|---|---|---|
| Quest (Balloon Streak) | không có | `QuestRules`, `QuestView` | Một game |
| Ưu đãi xoay vòng | không có | `RollingOfferRules`, `RollingOfferView` | Một game |
| Sky Jump | không có | `SkyJumpRules`, `SkyJumpView` (chỉ vỏ) | Một game |
| Nhìn kiến bay ở màn chính | `HomeAntFlight`… | không có | Một game (trang trí riêng) |

## 3. Việc kiểm lại bằng cách thứ hai
| Mục | Cách đầu | Cách kiểm lại | Kết quả |
|---|---|---|---|
| Luật game-5 thuần C# | đọc đầu file `Rules/*.cs` | `grep -l "using UnityEngine"` trong `Rules/` | 0/26: khớp |
| Bảng xếp hạng game-3 là giả | đọc `HomeLeaderboardPage.cs` | đọc `Packages/manifest.json` tìm dịch vụ | không có dịch vụ: khớp |
| Bảng xếp hạng game-2 là thật | đọc mã | `manifest.json` có `com.unity.services.leaderboards` 2.3.4 | khớp |
| Cách lưu | đọc chữ ký | đếm file chạm `PlayerPrefs` / `SaveData` mỗi game | 3 / 1 và 0 / 16: khớp |
| Điểm danh ngày không có ở game-5 | đọc danh sách file | `grep` từ khóa điểm danh trong `ShellController` | chỉ thấy phần thưởng của nhiệm vụ tuần: khớp (chưa đọc thân hàm) |
| Vùng an toàn ở game-5 | đọc danh sách file | `grep "SafeArea"` | 0 file: khớp |

## 4. Chưa kiểm
- Thân hàm của từng file vỏ: mức giống nhau thật sự (ví dụ `Haptics.cs` hai game) chưa đo bằng `diff`.
- Số lần bám tên node chỉ là ước tính bằng một mẫu `grep`.
- Chưa mở scene / prefab của hai game, nên "màn hình tải của game-5 không có" chỉ là "không thấy trong mã".
- Chưa đọc `ModalSpine.cs` (571 dòng), `BattleHud.cs`, `HudView.cs` (HUD trong ván); chưa phân loại HUD là vỏ hay gameplay.

## 5. Câu cần người làm game quyết (từ bảng)
1. Đưa **điểm danh ngày** vào khung (game-3 có, game-5 không)?
2. Đưa **màn hình tải** và **vùng an toàn** vào khung (game-3 có, game-5 không thấy)?
3. Đưa **hồ sơ + lọc từ chửi**, **chọn booster trước ván**, **thông báo đẩy**, **cấu hình từ xa** (chỉ game-5) vào khung?
4. **Đa ngôn ngữ**: có đưa vào khung không?
5. Quest, ưu đãi xoay vòng, Sky Jump: giữ ngoài khung tới khi lặp ở game thứ ba (theo plan)?
