# Báo cáo sáng (đêm 2026-11-11 sang 12)

Mọi số bên dưới do tôi chạy lại độc lập, trừ chỗ ghi "Codex báo". Chi tiết từng bước: `docs/OVERNIGHT_PLAN.md` (nhật ký), `docs/SKILL_EVAL.md`, `docs/SHELL_SPEC.md`, `framework/REPORT*.md`.

## Mục tiêu đêm và kết quả
| Mục | Mục tiêu | Kết quả | Số đo |
|---|---|---|---|
| T1a lát cắt dọc | build 0 lỗi, test 0 lỗi, Unity error CS = 0, APK build | Đạt | dotnet 21 test lúc đầu; Unity EditMode đúng danh sách; APK 17,8 MB |
| T1 lõi khung (10 module) | test hành vi bắt buộc chạy thật | Đạt | 168 test dotnet; phá cố ý 5 chỗ đều làm test đỏ; Codex 5,9 + 8,4 + 8,0 phút |
| T2 skill | ≥ 3 tình huống mỗi skill + baseline | Đạt cho 6 skill (vượt kế hoạch) | `docs/SKILL_EVAL.md`; thử bằng `claude -p`, mỗi đề 1 lần, regex thô |
| T3 hook | ≥ 3 hook, ca chặn đúng và không chặn nhầm | Đạt | 20/20 script + 3 ca phiên thật; chưa cài toàn cục |
| T4 xưởng phong cách | 4 bảng phong cách | Có ứng viên, CHƯA duyệt | `art/style_trial/montage.png`; ảnh 1254 px (brief 2048) |
| T5 đồ vật 3D | ≤ 2.000 tam giác, 0 cạnh hở | Đạt | 96 tam giác, 0 cạnh hở; chạy lại script cho cùng kết quả |
| T6 lớp Unity | adapter + chạy Unity sạch | Đạt | error CS 0 |
| T8 spec | spec module | Đạt (nháp) | `docs/SHELL_SPEC.md` |
| Màn hình và bảng thông báo (T12-T17) | 6 màn, 5 bảng, 2 phong cách, vi + en | Đạt kỹ thuật, chưa đẹp | Unity EditMode 41/41, PlayMode 20/20; 132 ảnh; hình tạm |
| T18-T19 game thử `game-2-v2` | luật cũ + khung + hình tạm, chạy được vòng chơi | Đạt (sau 1 lần phát hiện sai lệch) | EditMode 325/325, PlayMode 4/4 (2 lần chạy lại), APK 38,5 MB chưa cài điện thoại |

Thời gian đo được từ nhật ký: Codex mất 4 đến 20 phút mỗi việc lớn; từ 23:25 (T1a bắt đầu) đến 01:45 là 2 giờ 20 phút, gồm cả lúc chờ.

## Đã phát hiện và sửa trong đêm
- Báo cáo của Codex về PlayMode game-2-v2 (4/4) không tái lập được: tôi chạy 4 lần đều 2/4. Nguyên nhân gốc là bài test lệch chỉ số kích thước màn sau khi đổi sang Android; đã sửa, chạy lại 4/4.
- Runner Unity viết cứng số test nên báo đỏ trên bộ xanh: chuyển sang file danh sách tên test.
- APK build lần đầu lỗi (Mono không có ARM64): đổi sang IL2CPP.
- Chữ dài bị cắt trong màn hình: gộp bước vừa chữ vào bên trong view.

## Chưa làm / chưa kiểm
- Điện thoại: không có máy cắm, chưa cài APK nào. Cảm giác chạm, âm thanh, nút Back Android chưa kiểm.
- Hình là hình tạm. Chưa có phong cách được duyệt nên chưa sinh bộ hình đầy đủ (bước E).
- SDK quảng cáo, mua hàng, bảng xếp hạng thật chưa nối (chỉ bản giả).
- Skill chưa thử trong phiên làm việc thật (chỉ `claude -p`), chưa cài vào `~/.claude/skills/`; hook chưa cài toàn cục.
- `game-2-v2` chưa có `.gitignore` và chưa commit; ổ đĩa còn 8,3 GiB.
- Hẹn giờ 04:12 (CronCreate) chưa kiểm chứng chạy thật.

## Cần bạn quyết
1. Chọn một trong 4 phong cách (`art/style_trial/montage.png`) để xưởng sinh bộ hình đầy đủ và thay hình tạm của `game-2-v2`.
2. Hai skill giao diện chồng nhau: `skills/game-ui-workflow` (cũ, dựa trên game-kit) và `skills/building-game-ui` (mới, dựa trên khung). Gộp hay bỏ một cái?
3. Cài hook và skill toàn cục (sửa `~/.claude/settings.json` và `~/.claude/skills/`): làm hay chưa?
4. Đổi tên repo `game-skills` (hiện chứa khung, xưởng, skill, hook); commit `game-2-v2` vào repo `indie-game` (cần thêm `.gitignore`).
