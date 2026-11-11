# Brief bàn giao phiên mới (game-skills), 2026-11-07
Bạn là Claude Code mở phiên mới trong `~/project/indie-game/game-skills`. Trả lời tiếng Việt, xưng tôi, gọi bạn. Người làm game: solo, Unity, làm cùng AI; Claude điều phối, Codex (CLI) viết mã/chạy Unity; người làm game chỉ duyệt và chọn.

## Đọc trước (theo thứ tự)
1. `docs/PLAN.md` (plan đọc cho người; trạng thái ở mục 6, việc cần duyệt mục 4; bản kỹ thuật cũ: `docs/SPEC_DRAFT.md`).
2. `docs/SKILL_AUTHORING_OFFICIAL.md` (10 quy tắc viết skill từ tài liệu chính thức).
3. `docs/SKILL_INVENTORY.md` (182 bài học theo miền, mục 2 lỗi lặp, mục 3 đề xuất 25 skill).
4. `../CLAUDE.md` (quy chuẩn chung indie-game) và `../knowledge/lam-viec-voi-codex.md`.

## Hướng đã chốt
- Làm BỘ SKILL cho Claude, không làm kit UI (code `../game-kit` tạm dừng, đừng mở lại).
- Mọi hình do AI vẽ (Codex `image_gen`, Claude viết brief, người làm game duyệt). Không dùng sprite game thương mại. Look mục tiêu theo game-3 trên điện thoại; game-2 không phải chuẩn; Kenney bị loại.
- KHÔNG đụng game-3 (phiên khác). KHÔNG commit/push repo cha `indie-game` các file không phải của mình (nhiều file đang sửa dở ở đó).
- Skill chỉ được coi là xong khi: tuân quy tắc chính thức, ≥ 3 tình huống test (gồm tình huống KHÔNG nên kích hoạt), có baseline không skill, chạy trong phiên mới.

## Việc đầu tiên
1. Hỏi người làm game MỘT LẦN, danh sách đánh số, mỗi câu có khuyến nghị: (a) gộp 9 skill theo thứ tự `SPEC_DRAFT.md` mục 5, làm theo giai đoạn (skill 1-2 trước, đo, rồi tiếp)? (b) look kẹo bóng game-3 có phải mẫu chính thức cho skill 3-4? (Câu 3-4 mục 6 đã chốt.)
2. Khi họ gật: viết skill 1 `session-orchestration` và skill 2 `verification` ở `skills/<tên>/SKILL.md` (nguồn: SKILL_INVENTORY mục 1.5, 1.6, 2.2-2.3). Skill phải đòi BẰNG CHỨNG ĐÃ ÁP DỤNG tài liệu, không chỉ "đã đọc". Hook cho quy tắc bắt buộc nếu cần (ví dụ đếm `error CS` trước khi báo xong).
3. Kiểm bằng ≥ 3 tình huống + baseline; ghi kết quả vào `docs/`. Skill cũ `skills/game-ui-workflow` là bản nháp, chưa kiểm.
4. Sau mỗi mốc: cập nhật `docs/PLAN.md` mục 6, commit, push (repo này được phép tự commit+push; không `reset --hard`, không `pull --rebase`, không force push).

## Cách làm việc (đã học)
- Việc lớn: Claude đề xuất → Codex phản biện CHỈ ĐỌC → Claude kiểm lại từng phát hiện → chốt vào plan → mới giao. Job Codex nền: `nohup codex exec -m gpt-6.1-sol -c model_reasoning_effort=low -s danger-full-access -C <dir> "<brief>" < /dev/null > /tmp/<tên>.log 2>&1 &`; dựng Monitor ngay (liveness + file tiến độ + tín hiệu kết thúc), Monitor hết hạn 30 phút thì dựng lại và nói rõ khi không còn canh. Kiểm lại kết quả Codex bằng số/ảnh/đọc nguồn, đừng tin báo cáo.
- Mọi việc muốn làm phải ghi vào `docs/` để người làm game theo dõi.
- Bẫy môi trường: zsh không tách `$VAR`; `sed -i` trên macOS cần `-i ''`; hook có thể chặn `Read` ảnh lần đầu (thử lại); ổ đĩa còn ~4 GB, tụt không rõ nguyên nhân, kiểm `df` trước việc nặng; điện thoại A07 đang cắm USB (package game-3 `com.zyx.colonyflowclone`), đừng chiếm khi họ đang dùng.
- Chỉ nói điều đã kiểm; số không có nguồn gắn "TỰ ĐẶT"; báo cáo kết quả thật kể cả test đỏ.
