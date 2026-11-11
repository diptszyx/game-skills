# Báo cáo sáng (khung, cập nhật trong đêm 2026-11-11)

Số đo thật ghi kèm nguồn. Mục chưa chạy ghi "chưa".

| Mục | Mục tiêu | Trạng thái | Số đo / bằng chứng |
|---|---|---|---|
| T1a lát cắt dọc | build 0 lỗi, test 0 lỗi, Unity error CS = 0, APK build | Đạt | 21 test dotnet; Unity EditMode 3/3, error CS 0; APK 17,8 MB; commit 917f48d |
| T1 lõi khung (10 module) | test hành vi bắt buộc chạy thật | Đạt | 137 test dotnet; 3 lần phá cố ý đều đỏ; Codex 5,9 + 8,4 + 8,0 phút |
| T2 hai skill | ≥ 3 tình huống mỗi skill + baseline | Đạt | `docs/SKILL_EVAL.md` |
| T3 hook | ≥ 3 hook, ca chặn đúng + không chặn nhầm | Đạt | 20/20 script + 3 ca phiên thật; chưa cài toàn cục |
| T4 xưởng phong cách | 4 bảng phong cách | Có ứng viên, chưa duyệt | `art/style_trial/montage.png`; ảnh 1254 px (brief 2048) |
| T5 đồ vật 3D | ≤ 2.000 tam giác, 0 cạnh hở, 3 góc | Chưa | |
| T6 lớp Unity | adapter + một lần chạy Unity sạch | Chưa | |
| T8 spec module | spec nháp cho module T1 | Chưa | |

## Phần tool chưa nằm trong kế hoạch đêm
4 skill còn lại (phong cách và hình AI, giao diện 2D, đồ vật 3D, dùng khung game), bước E (thay hình tạm), bước F (thử bằng game mới nhỏ). Phụ thuộc ảnh phong cách bạn phải duyệt.

## Chưa kiểm
APK chưa cài lên điện thoại; hook chưa cài toàn cục; thời gian cả đêm chưa đo.
