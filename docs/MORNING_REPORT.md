# Báo cáo sáng (khung, cập nhật trong đêm 2026-11-11)

Số đo thật ghi kèm nguồn. Mục chưa chạy ghi "chưa".

| Mục | Mục tiêu | Trạng thái | Số đo / bằng chứng |
|---|---|---|---|
| T1a lát cắt dọc | build 0 lỗi, test 0 lỗi, Unity error CS = 0, APK build | Đạt | 21 test dotnet; Unity EditMode 3/3, error CS 0; APK 17,8 MB; commit 917f48d |
| T1 lõi khung (10 module) | test hành vi bắt buộc chạy thật | Đạt | 137 test dotnet; 3 lần phá cố ý đều đỏ; Codex 5,9 + 8,4 + 8,0 phút |
| T2 skill | ≥ 3 tình huống mỗi skill + baseline | Đạt cho cả 6 skill (viết vượt kế hoạch đêm) | `docs/SKILL_EVAL.md`; thử bằng `claude -p`, mỗi đề 1 lần, regex thô; chưa cài toàn cục |
| T3 hook | ≥ 3 hook, ca chặn đúng + không chặn nhầm | Đạt | 20/20 script + 3 ca phiên thật; chưa cài toàn cục |
| T4 xưởng phong cách | 4 bảng phong cách | Có ứng viên, chưa duyệt | `art/style_trial/montage.png`; ảnh 1254 px (brief 2048) |
| T5 đồ vật 3D | ≤ 2.000 tam giác, 0 cạnh hở, 3 góc | Đạt | 96 tam giác, 0 cạnh hở, 3 ảnh; chạy lại script cho cùng kết quả; Codex 3,8 phút |
| T6 lớp Unity | adapter + một lần chạy Unity sạch | Đạt | error CS 0, EditMode 19/19; chưa thử trên điện thoại |
| T8 spec module | spec nháp cho module T1 | Đạt (nháp) | `docs/SHELL_SPEC.md` |
| T11-T16 màn hình, bảng thông báo | 6 màn + 5 bảng, 2 phong cách, vi + en | Đạt kỹ thuật, chưa đẹp | dotnet 168; Unity EditMode 40/40, PlayMode 20/20; 132 ảnh; hình tạm; `art/shell_ui_montage_*.png` |

## Phần tool chưa nằm trong kế hoạch đêm
4 skill còn lại (phong cách và hình AI, giao diện 2D, đồ vật 3D, dùng khung game), bước E (thay hình tạm), bước F (thử bằng game mới nhỏ). Phụ thuộc ảnh phong cách bạn phải duyệt.

## Chưa kiểm
APK chưa cài lên điện thoại; hook chưa cài toàn cục; thời gian cả đêm chưa đo.

## Cần bạn quyết
1. **Hai skill UI chồng nhau:** `skills/game-ui-workflow` (có từ trước, dựa trên game-kit) và `skills/building-game-ui` (mới, dựa trên khung). Hiện tôi để cả hai và thêm dòng chỉ chéo; nên gộp hay bỏ một cái?
2. **Chọn một trong 4 phong cách** (`art/style_trial/montage.png`) để xưởng sinh bộ hình đầy đủ và thay hình tạm.
3. **Cài hook và skill toàn cục** (`~/.claude/settings.json`, `~/.claude/skills/`): chưa làm, vì sửa cấu hình chung của bạn.
4. **Đổi tên repo** `game-skills` (hiện chứa cả khung, xưởng, skill, hook).
