# Kế hoạch chạy liên tục (đêm 2026-11-11) và mục tiêu bằng số

Người làm game treo máy, không hỏi. Claude quyết cùng Codex (phản biện chỉ đọc, Claude kiểm lại từng phát hiện). Mọi số dưới đây là **mục tiêu tự đặt** (chưa đo), trừ khi ghi rõ nguồn. Nhật ký ở cuối file, mới nhất ở trên.

## Luật chạy
- Không dừng khi mới xong một phần. Chỉ dừng khi: (a) mọi mục tiêu bảng dưới đạt; (b) Codex hết hạn mức: ghi giờ mở lại, KHÔNG thử lặp, làm tiếp việc Claude tự làm được; (c) việc cần người duyệt (ảnh phong cách): xuất ứng viên và làm tiếp việc không phụ thuộc.
- Mục tiêu không đạt thì ghi số đo thật + nguyên nhân và chuyển mục kế; quay lại cuối đêm.
- Mỗi job Codex trần 60 phút, có Monitor. Cả đêm trần 8 giờ.
- Unity: tại mỗi thời điểm chỉ MỘT người chạy (Claude hoặc một job Codex), không chạy song song. Chạy sớm một lát cắt (xem T1a) rồi chạy lại ở cuối. Logic kiểm bằng `dotnet test`.
- Không sửa game-3, game-5, game-kit. Commit + push repo này sau mỗi mục xong.

## Mục tiêu
| # | Việc | Mục tiêu (đo được bằng lệnh) | Ai |
|---|---|---|---|
| T1a | Lát cắt dọc đầu tiên (làm TRƯỚC mọi module): package `framework/Packages/com.indiegame.shell` → Core `netstandard2.1` (LangVersion 9) + Newtonsoft → module Lưu (có số phiên bản, ghi nguyên tử, đọc bản hỏng) → fixture → `dotnet test` → dự án Unity thử nhập package → 1 lần EditMode → (nếu kịp) build APK debug | `dotnet build -c Release` 0 cảnh báo lỗi; `dotnet test` 0 lỗi; Unity: `error CS` = 0, EditMode chạy đúng danh sách test mong đợi (không bỏ qua); APK build được | Codex, Claude chạy lại |
| T1 | Lõi khung (C# thuần): lưu, kinh tế (vàng/tim/booster), hồi tim, điểm danh, nhiệm vụ ngày, cài đặt, đa ngôn ngữ, thông báo, ổ cắm (quảng cáo, mua hàng, cấu hình từ xa, bảng xếp hạng) có bản giả | 10 module (lưu, kinh tế, tim, điểm danh, nhiệm vụ ngày, cài đặt, đa ngôn ngữ, thông báo, cấu hình, ổ cắm + bản giả); `dotnet test` 0 lỗi; ≥ 100 test là chỉ số phụ. **Tiêu chí chính: danh sách hành vi bắt buộc đều có test chạy thật**: bản lưu thiếu/hỏng, ghi bị ngắt giữa chừng, số âm và tràn, giao dịch lỗi không đổi trạng thái, nhận thưởng lặp (chỉ cộng một lần), qua ngày / tuần (UTC), đồng hồ lùi, hồi tim trước khi lập lịch thông báo. Lõi không tham chiếu Unity: kiểm bằng biên dịch lõi riêng không có Unity (không chỉ `grep`) | Codex |
| T2 | Skill 1 (làm việc với AI), skill 2 (kiểm tra kết quả) | Mỗi `SKILL.md` < 500 dòng; mỗi skill ≥ 3 tình huống thử (≥ 1 không nên kích hoạt) + 1 ca baseline không skill, ghi kết quả vào `docs/` | Claude |
| T3 | Hook bắt buộc: chặn lệnh git nguy hiểm, chặn sửa game-3/game-5, nhắc tự kiểm khi kết thúc việc có sửa mã/hình | ≥ 3 hook, mỗi hook ≥ 1 ca chặn đúng + ≥ 1 ca không chặn nhầm, chạy bằng script thử, 0 ca sai | Claude |
| T4 | Xưởng phong cách (thử): 1 ý tưởng giả, ảnh phong cách | 4 bảng phong cách khác nhau (mỗi bảng đủ nút, bảng thông báo, chữ, nền, 3 biểu tượng); bảng quy định thông số cho bảng được chọn tạm | Codex vẽ, Claude brief |
| T5 | Dựng thử 1 đồ vật 3D bằng Blender từ 1 bảng phong cách | ≤ 2.000 tam giác; 0 cạnh hở; ảnh 3 góc; màu theo bộ phận; giờ chạy ghi lại | Codex |
| T6 | Lớp Unity: package, adapter rung/âm/vùng an toàn, màn hình tải; project thử | Một lần chạy Unity: `error CS` = 0, EditMode 0 lỗi, kiểm `asmdef` lõi không tham chiếu `UnityEngine` | Codex, Claude chạy |
| T7 | Docs: spec nháp các module T1, báo cáo sáng mai | `docs/MORNING_REPORT.md` có bảng mục tiêu: đạt / không đạt + số đo | Claude |

Thứ tự: T1 và T4 và T2/T3 song song (không đụng nhau: mã lõi / ảnh / tài liệu). T5 sau T4. T6 sau T1. T7 cuối.

## Quyết định chính sách (Claude chốt sau phản biện Codex, người làm game ủy quyền)
- Ngày tính theo UTC. Tim vô hạn cộng dồn thời gian. Mọi phần thưởng có mã nhận, nhận lặp cùng mã chỉ cộng một lần.
- Bản lưu chia theo phần, mỗi module một phần, có số phiên bản và hàm nâng cấp; game riêng thêm phần của mình mà không sửa khung.
- Chưa đổi Newtonsoft sang System.Text.Json (mã cũ dùng thuộc tính Newtonsoft); dùng gói Unity `com.unity.nuget.newtonsoft-json`.
- Kiểm hook bằng phiên Claude Code thật (không chỉ script) và kiểm skill bằng `claude -p` có/không skill.
- Quyền commit + push ở repo này đã được cấp (HANDOFF, memory `feedback-autonomy-game-kit`).

## Nhật ký (mới nhất ở trên)
- T3 đạt thêm bằng phiên `claude -p` thật (settings dự án): `git reset --hard` bị chặn (bản staged còn nguyên); ghi vào game-3 bị chặn (file không tạo); ghi ok.txt + `git status` không bị chặn nhầm; hook Stop chạy sau khi tạo notes.py và model trả bản tự kiểm 7 mục (trả lời "not verified" đúng cho ca không có Unity). Lưu ý: hook Stop hơi ồn với việc nhỏ không liên quan game. Hook vẫn CHƯA cài toàn cục.
- T1b (Unity) giao Codex lúc này, Monitor `bti3drcpx`.
- T1a phần dotnet xong, Claude tự kiểm: build 0 lỗi 0 cảnh báo; 21 test qua; 0 dòng UnityEngine trong Core; cố ý phá logic bản sao lưu → 2 test đỏ, khôi phục → 21 xanh. Còn lại của T1a: dự án Unity nhập package + EditMode + APK. Job Codex mất ~4 phút (đo: giờ ghi file pid 23:25:58 đến giờ ghi cuối của log 23:29:54).
- T2 đạt: 6 đề (2 skill × 3, mỗi skill 1 ca không nên kích hoạt), có skill 5/5, 5/5, 4/4, 3/3 so với không skill 0/5, 1/5, 2/4, 1/3; hai ca âm không kích hoạt. Chi tiết `docs/SKILL_EVAL.md`.
- Codex phản biện xong (`work/critique1.log`, đã kiểm 4 phát hiện đều đúng); skill 1 và 2 viết bản đầu (commit 5927f54); 3 hook + 20 ca thử đạt 20/20 (commit 086b475); thử `claude -p` có skill: skill kích hoạt đúng và từ chối báo xong khi thiếu bằng chứng (1 mẫu, chưa phải đánh giá).
- 2026-11-11 khởi động: tạo kế hoạch; kiểm môi trường: dotnet 10.0.403, Unity 6000.3.25f1, Codex 0.160.1 (luna, low), A07 cắm USB, không có Unity đang chạy, ổ đĩa trống 13 GiB.
