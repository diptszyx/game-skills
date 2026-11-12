# IndieGame Shell (framework)

Bộ khung dùng chung cho game hybrid-casual. Spec: `../docs/SHELL_SPEC.md`. Báo cáo từng bước: `REPORT*.md`.

## Cấu trúc
| Thư mục | Nội dung |
|---|---|
| `Packages/com.indiegame.shell/Runtime/Core` | C# thuần (netstandard2.1, C# 9, Newtonsoft). Lưu, kinh tế, tim, điểm danh, nhiệm vụ ngày, cài đặt, đa ngôn ngữ, thông báo, cấu hình, mua hàng, ổ cắm giả, `ShellSession`. Không UnityEngine |
| `Packages/com.indiegame.shell/Runtime/Unity` | Adapter rung, âm thanh, vùng an toàn, màn hình tải; `UI` (ShellStyle, ShellUiFactory, PopupHost, ScreenHost); `Screens` (6 màn + 5 bảng thông báo, view + presenter); `Localization` (bảng en/vi) |
| `Packages/com.indiegame.shell/Tests` | EditMode, PlayMode của Unity |
| `dotnet/` | Cùng nguồn Core biên dịch bằng .NET và xUnit |
| `unity-smoke/` | Project Unity thử (nhập package, chạy test, build APK, chụp màn hình) |
| `tests/run_unity_smoke.sh` | Cổng Unity: `[--mode EditMode\|PlayMode]`; danh sách test kỳ vọng ở `tests/expected_<Mode>.txt` |
| `fixtures/` | Dữ liệu thử cho Lưu |

## Lệnh kiểm (từ thư mục `framework`)
- `dotnet build dotnet/Shell.Core -c Release` (0 cảnh báo, 0 lỗi)
- `dotnet test dotnet/Shell.Core.Tests -c Release`
- `bash tests/run_unity_smoke.sh` và `bash tests/run_unity_smoke.sh --mode PlayMode`: in `error CS count`, `passed/failed/expected`. Mỗi lần chỉ một Unity.
- Ảnh chụp: `unity-smoke/Logs/shots/` (bị git bỏ qua), ghép bằng `tools/montage.py`.

## Quy ước chính
- Mọi thay đổi trạng thái qua `ShellSession` rồi `Commit()` một lần (ghi mọi phần trong một file, nguyên tử).
- Thưởng qua `IRewardGrantor.Grant(claimId, reward)`; cùng mã chỉ cộng một lần.
- Thời gian qua `IClock` (UTC). Chữ qua khóa của `LocalizationService`.
- Kiểu dáng qua `ShellStyle` (tham chiếu theme của game-kit, chỉ đọc). Không màu hay hình viết cứng trong code.
- Game gắn vào qua `IGameSessionActions`. Không thêm nội dung riêng của game vào khung.

## Chưa kiểm
IL2CPP chạy thật trên điện thoại, cảm giác chạm, nút Back Android, SDK quảng cáo / mua hàng / bảng xếp hạng thật, hình thật (chỉ có hình tạm).

## Booting a game with ShellApp

Create a `ShellApp` component explicitly and call `Boot(new ShellAppConfig { ... })` once. Supply `Style`, `LanguageTables` (language code to JSON `TextAsset`, covering all `Settings.Languages`), `SavePath`, `Lives`, `Economy`, `CheckIn`, `Quests`, `Settings`, `RemoteConfig`, `SessionPopups`, `Actions` (`IGameSessionActions`), and `GameScreenFactory`. Optional fields are `Clock` (defaults to `SystemClock`), `SaveStore` (supports `ISaveFileSystem` injection), `RewardedAds`, `ShopFactory` or `ShopCatalog` + `Purchases`, and `Leaderboard` + `BoardId` + `LeaderboardRows`. Missing optional screens throw a named unavailable error when navigated to.

The game factory receives `ShellGameContext` with Factory, Session, Text, Popups, Flow, Parent, Navigate and ShowMain. Return one game `RectTransform`, built under Parent; ShellApp owns it. Keep gameplay input gated on `app.Ready`, the visible game root and an empty popup stack. The factory runs before Ready; wait for Ready after a successful BootResult before navigating. Main Play calls Actions.Play once and shows the game; Leave calls Actions.Leave and shows Main. NextLevel/Retry/Revive retain game-specific behavior; use context.ShowMain when the final level completes. Use `Flow.ShowPause/ShowLeave/ShowWin/ShowLose` for session popups, `Navigate(ShellScreen)` for service screens, and `Commit()` to retry persistence without replaying gameplay actions. ShellApp owns Back routing and restores the timeScale captured at Boot when destroyed. A failed load retains only a localized error surface, remains unready and refuses writes. Destroy ShellApp to release its roots.

PilotApp's own shell assembly/composition is **migration-only**, with removal checkpoint **T29b**. New games use ShellApp; the Pilot migration is not part of this package slice. Legacy Main layout is unchanged; home chrome remains deferred to T28e.
