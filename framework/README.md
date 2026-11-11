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
