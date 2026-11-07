# Kế hoạch BỘ SKILL (nguồn duy nhất về trạng thái; cập nhật 2026-11-07)
Người làm game: solo, Unity, làm cùng AI (Claude điều phối, Codex viết mã/chạy Unity). Mục tiêu: một BỘ SKILL cho Claude để game mới làm nhanh, đẹp, ít lặp lỗi. Không viết skill nào nếu chưa có bài học nguồn và cách kiểm.

## 0. Việc đang dở / bắt đầu phiên mới đọc cái này trước
- Repo này: `~/project/indie-game/game-skills/` (skill sống ở `skills/<tên>/SKILL.md`, symlink từ `~/.claude/skills/`). Đang ở: sau S1 (kiểm kê), chờ người làm game chọn hướng (mục 6). Chưa viết skill mới nào.
- Đọc theo thứ tự: file này → `docs/SKILL_INVENTORY.md` (182 bài học, mục 2 lỗi lặp, mục 3 đề xuất 25 skill) → `docs/SKILL_AUTHORING_OFFICIAL.md` (10 quy tắc viết skill, nguồn chính thức) → `../indie-game/knowledge/game-kit/GAME_KIT_PLAN.md` mục 9-10 (kit, repo cha `indie-game`).
- Phiên mới KHÔNG chạm game-3 (project của phiên khác) và không mở lại kit.

## 1. Quyết định đã chốt (người làm game)
1. Trọng tâm là SKILL, không phải kit. Kit (code Unity `game-kit/`) tạm dừng tới khi chốt được một mẫu UI đẹp; phần code không phụ thuộc UI (Theme, UiModal, validator, harness) giữ nguyên.
2. Mọi hình do AI vẽ (Codex `image_gen`, Claude viết brief, người làm game duyệt). Không dùng sprite của game thương mại (kể cả trích từ APK Colony Flow). Kenney (CC0) chỉ là nguồn phụ; look Kenney bị đánh giá xấu.
3. Look mục tiêu của UI: kiểu game-3 trên điện thoại (nút kẹo bóng viền kem dày, ruy-băng xanh hai đuôi, khung kem-cam, nút X đỏ tròn, nút khoá xám + ổ khoá, chữ đậm viền+bóng). CHƯA xác nhận là bộ mẫu chính thức (câu hỏi mở 6.2). Game-2 KHÔNG phải chuẩn tham chiếu.
4. Skill phải tuân tài liệu chính thức (`docs/SKILL_AUTHORING_OFFICIAL.md`) và được kiểm: ≥ 3 tình huống (gồm tình huống KHÔNG nên kích hoạt), baseline không skill, phiên mới.

## 2. Bộ skill đề xuất (Claude gộp 25 → 9; là đề xuất, không phải quy tắc Anthropic)
| # | Skill | Gộp từ (SKILL_INVENTORY mục 3) | Trạng thái |
|---|---|---|---|
| 1 | session-orchestration | session-context, codex-brief, job-monitor, research-decision | chưa viết; đủ nguồn |
| 2 | verification | unity-verification, visual-acceptance | chưa viết; đủ nguồn |
| 3 | asset-reuse-art-direction | approved-asset-reuse, art-direction | chờ mẫu look đã chốt |
| 4 | ui-2d | ui-layout, ui-text-accessibility | chờ mẫu UI đã chốt; skill cũ `game-ui-workflow` là bản nháp |
| 5 | art-pipeline | sprite-cutout, sprite-import-atlas, sprite-animation | đủ nguồn |
| 6 | 3d-look | toon-lookdev, ai-mesh-preparation | thiếu mẫu penguin toon |
| 7 | puzzle-design | puzzle-rule-contract, puzzle-generation-check | đủ nguồn |
| 8 | feel-audio-meta | game-feedback, audio-qa, economy, save, service-adapter | cần thêm research |
| 9 | release | release-readiness, marketing-creative | thiếu research; để cuối |
Thứ tự đề xuất: 1 → 2 → 5 → 3 → 4 → 7 → 6 → 8 → 9. Phương án làm theo giai đoạn: viết skill 1-2 trước, dùng 1-2 phiên thật, đo kích hoạt, rồi mới viết tiếp (không cam kết 9 từ đầu).
Chưa có bằng chứng "9 skill nhỏ" tốt hơn "1 skill + references theo miền"; tài liệu chính thức không nói số tối đa hay cách tránh chồng lấn.

## 3. Lỗi lặp cần skill chặn (bằng chứng trong SKILL_INVENTORY mục 2.2-2.3)
Không đọc nền tảng theo lớp; làm lại asset đã duyệt (HOME_RETRO:16, LOSE_MODAL_RETRO:5); cắt ảnh còn viền mờ; brief gom nhiều việc; job nền không có cơ chế quay lại; shader mất trên build. Gốc [suy luận của Codex]: thiếu cơ chế buộc ÁP DỤNG tài liệu theo trigger (đòi bằng chứng đã áp dụng, không chỉ "đã đọc").

## 4. Kit (code) hiện trạng
`~/project/indie-game/game-kit/` (repo riêng, private github.com/diptszyx/game-kit, commit 8a23228 + ce3e483): package `com.indiegame.ui`, Theme Kenney + gallery. EditMode 38/38, PlayMode 10/10. Tạm dừng thêm thành phần (toggle/slider/tab/toast/HUD) tới khi có mẫu UI. Bản trùng trong `game-2/unity/Packages/` chưa xóa. Skill cũ ghi nơi sửa kit là game-2: lỗi thời.

## 5. Cách theo dõi (monitor) mọi việc
- Mỗi việc lớn có brief trong `briefs/` (skill) và `indie-game/knowledge/game-kit/briefs/` (kit) (S1 kiểm kê; K* về kit). Job Codex chạy nền: `nohup codex exec ... < /dev/null > /tmp/k3b/<tên>.log`, canh bằng Monitor (liveness + file tiến độ + tín hiệu kết thúc). Monitor hết hạn sau 30 phút: dựng lại và nói rõ khi không còn canh.
- Kết quả Codex luôn được Claude kiểm lại độc lập (số, ảnh, đọc lại nguồn) trước khi báo.
- Cập nhật mục 0 và mục 2 của file này sau MỖI mốc, rồi commit.

## 6. Câu hỏi mở (cần người làm game)
1. Gộp 9 skill theo thứ tự trên, hay tách nhiều hơn / gom ít hơn? Làm theo giai đoạn (1-2 trước)?
2. Look kẹo bóng (mục 1.3) có phải mẫu chính thức cho skill 3-4 không?
3. Skill đặt trong repo riêng `game-skills` (khuyến nghị) hay giữ trong `indie-game/knowledge/game-kit/skill/`? → ĐÃ CHỐT 2026-11-07: repo riêng `game-skills` (đã tạo).
4. Repo GitHub private `diptszyx/game-skills`: ĐÃ TẠO 2026-11-07.
