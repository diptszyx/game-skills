# Kế hoạch KHUNG GAME CHUNG (shell) + skill đi kèm (nguồn duy nhất về trạng thái; tạo 2026-11-11)

> Đây là PLAN, chưa có mã. Không viết mã trước khi người làm game duyệt mục 6. Quy trình: `../../CLAUDE.md` (Tầng 1, 2). Liên quan: `SKILL_PLAN.md`.

## 0. Việc đang dở
- Trạng thái: plan nháp, chờ người làm game duyệt (mục 6). Chưa đọc sâu mã hai game: các số ở mục 2 chỉ là đếm file / dòng.
- Không sửa `game-3`, `game-5` (chỉ đọc). Không mở lại `game-kit` để thêm tính năng.

## 1. Đích
Game mới dựng vỏ (Home, popup, Shop, Daily, Settings, Audio, kinh tế, ổ cắm nền tảng) bằng cách **dùng lại một package khung**, chỉ viết gameplay. Khung đổi giao diện được (theme + ảnh AI vẽ), không chứa hình lấy từ APK.
- Đích đo được (TỰ ĐẶT, chưa có nguồn, đổi sau bước S7): giờ dựng vỏ game mới nhỏ < giờ dựng vỏ game-5. Giờ dựng vỏ game-5: CHƯA ĐO.
- Một công cụ, một repo: package Unity khung + các skill hướng dẫn dùng nó. Kit cũ (`game-kit`) gộp vào đây rồi lưu trữ.

## 2. Hiện trạng đo được (2026-11-11, đếm file / dòng, chưa đọc sâu)
| | game-3 (Colony Flow) | game-5 (Hole it!) | game-kit |
|---|---|---|---|
| C# không test | 12.213 dòng, 130 file | 9.880 dòng, 124 file | 1.081 dòng |
| Vỏ | Home + Audio ~1.372 dòng | Shell + Platform + Audio ~1.505 dòng | Modal, Theme, SafeArea, Validator |
| Gameplay | `Core/`, `Battle/` | `Rules/`, `Game/` | không có |
- Khối trùng tên ở cả hai game: Home, Shop (`ShopCatalog` / `ShopView`), Daily (`DailyRewards` / `DailyChallengeView`), Audio, Haptics (52 và 62 dòng, diff 85 dòng), Settings, Loading.
- Kit không có Shop, Daily, kinh tế, Settings, Audio: đúng chỗ hai game lặp lại.
- Hình của vỏ hai game dựng từ prefab gốc APK, KHÔNG dùng lại được cho game mới (quyết định SKILL_PLAN mục 1.2).
- Chỉ có 2 mẫu, cùng thể loại hybrid-casual, cùng là clone. Rule of three: mẫu thứ ba là game mới (S7).
- Phiên bản Unity: game-5 6000.3.25f1 (đã ghi trong plan game-5); game-3 CHƯA KIỂM.

## 3. Quyết định đã chốt
1. (2026-11-11, người làm game) Gộp kit và skill thành một công cụ; hướng "chỉ skill" bỏ. Làm plan trước, chưa viết mã.
2. Khung chỉ chứa phần chung của vỏ; gameplay luôn riêng từng game.
3. Hình do AI vẽ qua pipeline đã chốt (SKILL_PLAN mục 1.2, 1.7); khung chỉ nhận hình qua theme.

## 4. Ranh giới đề xuất (CHƯA CHỐT, S1 kiểm lại)
| Lớp | Nội dung | Ứng viên giai đoạn |
|---|---|---|
| Nền tảng | Haptics, Settings (âm / rung), Audio (catalog + quy tắc), ổ cắm quảng cáo / IAP / Remote Config giả (`PlatformSockets`), `SaveData` | 1 (lặp rõ nhất) |
| Kinh tế | Một cổng vàng / tim / booster, kiểm số âm, test số dư | 1 |
| Popup | Win / Fail / Revive / Pause / Settings, vòng đời hiện-ẩn, an toàn màn hình | 2 |
| Màn hình | Home, Shop (catalog + giá), Daily, Loading | 3 |
| Chỉ một game | Quest, Rolling Offer, Sky Jump, Balloon Streak, Leaderboard, Spine... | không vào khung nếu chưa lặp lần 3 |
Hạt cắt: mỗi lớp là một assembly riêng, lớp trên phụ thuộc lớp dưới, không phụ thuộc gameplay. Luật thuần C# không tham chiếu `UnityEngine` (quy chuẩn `../../CLAUDE.md`).

## 5. Bảng bước
Cột "Ai": **C** = Claude, **X** = Codex, **N** = người làm game. Bước nào chưa có lệnh kiểm thì Claude viết trước khi giao.
| # | Việc | Ai | Kiểm | Phụ thuộc |
|---|---|---|---|---|
| S0 | Bảng đối chiếu từng file vỏ game-3 vs game-5 → `docs/SHELL_COMPARE.md` (file, giống / gần giống / khác / một game, ghi chú). Đọc mã thật, chỉ đọc | C | Mọi file trong `Home`, `Audio` (game-3) và `Game/Shell`, `Game/Platform`, `Audio` (game-5) có đúng một dòng; mẫu 5 dòng được đọc lại độc lập | |
| S1 | Chốt ranh giới và API mức giao diện (không mã) cho giai đoạn 1; Codex phản biện CHỈ ĐỌC bằng dữ liệu S0 | C + X | Từng phát hiện của Codex được Claude kiểm (đúng / sai / chưa kiểm), kết quả ghi vào mục 4 | S0 |
| S2 | Duyệt ranh giới, phạm vi giai đoạn 1, tên package | N | Người làm game gật ở mục 6 | S1 |
| S3 | Quyết vị trí và cấu trúc: thư mục trong `game-skills`, assembly, Unity phiên bản (kiểm game-3), quan hệ với `game-kit` (lưu trữ, giữ Validator / Modal nếu dùng được) | C | Ghi vào mục 3; `unity` của hai game mở được package đó (kiểm cấu trúc, chưa cần chạy) | S2 |
| S4 | Giai đoạn 1: rút Haptics, Settings, Audio, `SaveData`, kinh tế, ổ cắm nền tảng vào package kèm test EditMode | X | `tests/run_unity.sh` xanh, `error CS` = 0, mỗi lớp có test số dư / bật tắt; Claude chạy lại cổng độc lập | S3 |
| S5 | Giai đoạn 2: popup + theme + an toàn màn hình; ảnh chuẩn do người làm game duyệt | X + N | Ảnh ghép so ảnh chuẩn ở 16:9 và 20:9, mọi popup của luồng | S4 |
| S6 | Giai đoạn 3: Home, Shop, Daily, Loading | X | Như S5 | S5 |
| S7 | Kiểm bằng game mới nhỏ (3 màn): dựng vỏ chỉ bằng khung, ghi GIỜ đo được | C + X | Giờ thật vào mục 1, thay số TỰ ĐẶT | S4 (gđ 1), S6 (đủ) |
| S8 | Viết skill hướng dẫn dùng khung (SKILL_PLAN mục 2, skill 1-2 trước), kiểm ≥ 3 tình huống + baseline + phiên mới | C | Theo `SKILL_AUTHORING_OFFICIAL.md` | S4 |
Cổng mỗi bước: xanh thì commit + push repo này rồi sang bước kế; đỏ thì dừng và báo.

## 6. Cần người làm game duyệt (trước S3)
1. Ranh giới ở mục 4 và thứ tự giai đoạn 1 → 2 → 3.
2. Chỉ giai đoạn 1 làm trước, đo, rồi mới tiếp (khuyến nghị: có, vì mới 2 mẫu).
3. Tên package (gợi ý: `com.indiegame.shell`).

## 7. Rủi ro
| # | Rủi ro | Cách giảm |
|---|---|---|
| R1 | Rút khung quá sớm từ 2 clone cùng thể loại, game mới lại khác | Chỉ giai đoạn 1; S7 dùng game mới làm mẫu thứ ba; chỗ chỉ một game không vào khung |
| R2 | Mã vỏ game-3 / game-5 phụ thuộc prefab gốc APK nên khó tách | S0 ghi rõ phụ thuộc từng file; khung nhận hình qua theme |
| R3 | Lặp lại lỗi kit cũ: làm lâu, kết quả xấu | Giai đoạn 1 không có UI; popup chỉ làm khi có ảnh chuẩn do người làm game duyệt |
| R4 | Hai game đang được phiên khác sửa, đọc phải ảnh chụp lệch | Ghi commit / thời điểm đọc vào `SHELL_COMPARE.md`; không sửa hai game |
| R5 | Con số "nhanh hơn" không có nguồn | Giữ nhãn TỰ ĐẶT tới S7 |
