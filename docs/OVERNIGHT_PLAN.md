# Kế hoạch chạy liên tục (đêm 2026-11-11) và mục tiêu bằng số

Người làm game treo máy, không hỏi. Claude quyết cùng Codex (phản biện chỉ đọc, Claude kiểm lại từng phát hiện). Mọi số dưới đây là **mục tiêu tự đặt** (chưa đo), trừ khi ghi rõ nguồn. Nhật ký ở cuối file, mới nhất ở trên.

## Luật chạy
- Không dừng khi mới xong một phần. Chỉ dừng khi: (a) mọi mục tiêu bảng dưới đạt; (b) Codex hết hạn mức: ghi giờ mở lại, KHÔNG thử lặp, làm tiếp việc Claude tự làm được; (c) việc cần người duyệt (ảnh phong cách): xuất ứng viên và làm tiếp việc không phụ thuộc.
- Mục tiêu không đạt thì ghi số đo thật + nguyên nhân và chuyển mục kế; quay lại cuối đêm.
- Mỗi job Codex trần 60 phút, có Monitor. Cả đêm trần 8 giờ.
- Một lần chạy Unity duy nhất, ở cuối, sau khi mọi mã đã gộp. Logic kiểm bằng `dotnet test` (không cần Unity).
- Không sửa game-3, game-5, game-kit. Commit + push repo này sau mỗi mục xong.

## Mục tiêu
| # | Việc | Mục tiêu (đo được bằng lệnh) | Ai |
|---|---|---|---|
| T1 | Lõi khung (C# thuần): lưu, kinh tế (vàng/tim/booster), hồi tim, điểm danh, nhiệm vụ ngày, cài đặt, đa ngôn ngữ, thông báo, ổ cắm (quảng cáo, mua hàng, cấu hình từ xa, bảng xếp hạng) có bản giả | `dotnet test`: ≥ 10 module, ≥ 100 test, 0 lỗi; mỗi module ≥ 6 test; `grep -r "using UnityEngine"` ở lõi = 0 | Codex |
| T2 | Skill 1 (làm việc với AI), skill 2 (kiểm tra kết quả) | Mỗi `SKILL.md` < 500 dòng; mỗi skill ≥ 3 tình huống thử (≥ 1 không nên kích hoạt) + 1 ca baseline không skill, ghi kết quả vào `docs/` | Claude |
| T3 | Hook bắt buộc: chặn lệnh git nguy hiểm, chặn sửa game-3/game-5, nhắc tự kiểm khi kết thúc việc có sửa mã/hình | ≥ 3 hook, mỗi hook ≥ 1 ca chặn đúng + ≥ 1 ca không chặn nhầm, chạy bằng script thử, 0 ca sai | Claude |
| T4 | Xưởng phong cách (thử): 1 ý tưởng giả, ảnh phong cách | 4 bảng phong cách khác nhau (mỗi bảng đủ nút, bảng thông báo, chữ, nền, 3 biểu tượng); bảng quy định thông số cho bảng được chọn tạm | Codex vẽ, Claude brief |
| T5 | Dựng thử 1 đồ vật 3D bằng Blender từ 1 bảng phong cách | ≤ 2.000 tam giác; 0 cạnh hở; ảnh 3 góc; màu theo bộ phận; giờ chạy ghi lại | Codex |
| T6 | Lớp Unity: package, adapter rung/âm/vùng an toàn, màn hình tải; project thử | Một lần chạy Unity: `error CS` = 0, EditMode 0 lỗi, kiểm `asmdef` lõi không tham chiếu `UnityEngine` | Codex, Claude chạy |
| T7 | Docs: spec nháp các module T1, báo cáo sáng mai | `docs/MORNING_REPORT.md` có bảng mục tiêu: đạt / không đạt + số đo | Claude |

Thứ tự: T1 và T4 và T2/T3 song song (không đụng nhau: mã lõi / ảnh / tài liệu). T5 sau T4. T6 sau T1. T7 cuối.

## Nhật ký (mới nhất ở trên)
- 2026-11-11 khởi động: tạo kế hoạch; kiểm môi trường: dotnet 10.0.403, Unity 6000.3.25f1, Codex 0.160.1 (luna, low), A07 cắm USB, không có Unity đang chạy, ổ đĩa trống 13 GiB.
