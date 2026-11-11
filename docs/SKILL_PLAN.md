# Kế hoạch BỘ SKILL (nguồn duy nhất về trạng thái; cập nhật 2026-11-11)
Người làm game: solo, Unity, làm cùng AI (Claude điều phối, Codex viết mã/chạy Unity). Mục tiêu: một BỘ SKILL cho Claude để game mới làm nhanh, đẹp, ít lặp lỗi. Không viết skill nào nếu chưa có bài học nguồn và cách kiểm.

## 0. Việc đang dở / bắt đầu phiên mới đọc cái này trước
- Repo này: `~/project/indie-game/game-skills/` (skill sống ở `skills/<tên>/SKILL.md`, symlink từ `~/.claude/skills/`). Đang ở (2026-11-11): đã đọc game-5 và retro game-3, đã chốt lại hướng (mục 1, điểm 5-9). **CHƯA viết skill hay mã khung nào**; việc kế là duyệt `docs/SHELL_PLAN.md` (mục 6 của nó): người làm game còn muốn làm rõ thêm nhiều vấn đề trước khi bắt đầu.
- Đọc theo thứ tự: file này → `docs/SKILL_INVENTORY.md` (182 bài học + mục 6 bổ sung game-3/game-5) → `docs/SKILL_AUTHORING_OFFICIAL.md` (10 quy tắc viết skill) → `../knowledge/game-kit/GAME_KIT_PLAN.md` mục 9-10 (kit, tạm dừng).
- Nguồn bằng chứng mới: `../game-3/docs/COLONY_FLOW_RETRO.md` (C1-C8, chưa commit ở game-3), `../game-5/docs/HOLEIT_CLONE_RETRO.md` (R0-R9, chưa commit ở game-5). Chỉ ĐỌC, không sửa game-3 / game-5.
- Quyền tự chạy ở repo này: commit + push tự động (đường dẫn cụ thể), không `reset --hard`, không `pull --rebase`, không force push. Vẫn hỏi cho nhánh thật: đổi hướng đã khóa, tiêu tiền, xóa việc của người làm game, art / âm / cảm giác họ phải duyệt. Nguồn: memory `feedback-autonomy-game-kit` (2026-11-06) và `docs/HANDOFF.md`.

## 1. Quyết định đã chốt (người làm game)
1. Trọng tâm là SKILL, không phải kit. Kit (code Unity `game-kit/`) tạm dừng tới khi chốt được một mẫu UI đẹp; phần code không phụ thuộc UI (Theme, UiModal, validator, harness) giữ nguyên.
2. Mọi hình do AI vẽ (Codex `image_gen`, Claude viết brief, người làm game duyệt). Không dùng sprite của game thương mại (kể cả trích từ APK Colony Flow). Kenney (CC0) chỉ là nguồn phụ; look Kenney bị đánh giá xấu.
3. Look mục tiêu của UI: kiểu game-3 trên điện thoại (nút kẹo bóng viền kem dày, ruy-băng xanh hai đuôi, khung kem-cam, nút X đỏ tròn, nút khoá xám + ổ khoá, chữ đậm viền+bóng). CHƯA xác nhận là bộ mẫu chính thức (câu hỏi mở 6.2). Game-2 KHÔNG phải chuẩn tham chiếu.
4. Skill phải tuân tài liệu chính thức (`docs/SKILL_AUTHORING_OFFICIAL.md`) và được kiểm: ≥ 3 tình huống (gồm tình huống KHÔNG nên kích hoạt), baseline không skill, phiên mới.
5. (2026-11-11) **Mục đích: làm game MỚI, không có APK để reverse.** Mọi skill phải dùng được khi không có bản gốc để trích hoặc so. Bỏ ý định làm skill "clone-từ-APK"; quy trình trích APK (Il2Cpp, typetree, GLSL) không vào skill. game-3 (Colony Flow) và game-5 (Hole it!) đều là clone 1:1 để học, không phải look do người làm game chọn.
6. **Ảnh chuẩn của game mới = ảnh do người làm game duyệt** (style frame / mockup AI vẽ), thay cho "ảnh chụp bản gốc" của game-5. Nguyên tắc giữ: test xanh chưa đủ, mỗi màn và mỗi popup của luồng phải so với ảnh chuẩn (R2, R9, C7).
7. **Skill art (3-4) dạy TẠO và KHÓA một phong cách bằng AI** (Claude viết brief, Codex `image_gen`, người làm game duyệt, khóa thành style frame + bảng thông số). Không dùng ảnh game-3 làm tài nguyên (look game-3 trên máy là sprite gốc Colony lấy từ APK, mâu thuẫn với điểm 2). game-3 và game-5 chỉ là MỨC CHẤT LƯỢNG THAM CHIẾU bằng số, mỗi game một vai: game-3 cho bố cục UI 2D và popup, game-5 cho 3D toon và hiệu năng trên máy yếu (A07: FPS 49,6 và RAM PSS 786 MB, mục tiêu 58 và 600, đo 2026-11-10). Ghi đơn vị và game nguồn cạnh mỗi số.
8. **Skill 1 có bước đầu phiên: đọc quyền tự chạy** (memory + docs) và hỏi một lần nếu chưa có; kèm danh sách nhánh vẫn phải hỏi (C8: người làm game nhắc 4 lần "đừng dừng hỏi từng chút").
9. **Bài học ghi vào `docs/` của repo NGAY lúc gặp lỗi**, memory chỉ là đường tắt: memory nằm theo thư mục làm việc (game-5 không có thư mục memory riêng; bài học game-5 bị ghi vào memory của game-3) và cả hai retro tự nhận phải dựng lại sau (C0, R0). Ứng viên hook / bước bắt buộc trong skill 1.
10. (2026-11-11) **Gộp kit và skill thành một công cụ: khung game (package Unity) + skill hướng dẫn dùng**, cùng repo này. Đổi hướng "chỉ skill". Lý do: phần lặp thật giữa game-3 và game-5 là mã vỏ (Home, Shop, Daily, Audio, Haptics, Settings), kit cũ không có các phần đó. Plan riêng: `docs/SHELL_PLAN.md`. Chưa viết mã.

## 2. Bộ skill đề xuất (Claude gộp 25 → 9; là đề xuất, không phải quy tắc Anthropic)
| # | Skill | Gộp từ (SKILL_INVENTORY mục 3) | Trạng thái |
|---|---|---|---|
| 1 | session-orchestration | session-context, codex-brief, job-monitor, research-decision | chưa viết; đủ nguồn; thêm bước đọc quyền tự chạy, retro ngay lúc lỗi, một lần chạy Unity khi chia agent, canh agent bằng 2 nguồn (INVENTORY mục 6) |
| 2 | verification | unity-verification, visual-acceptance | chưa viết; đủ nguồn; ảnh chuẩn = ảnh người làm game duyệt (điểm 6), chụp MỌI popup của luồng (R9) |
| 3 | asset-reuse-art-direction | approved-asset-reuse, art-direction | dạy tạo và khóa phong cách bằng AI (điểm 7); tham chiếu chất lượng game-3 / game-5; không còn "chờ mẫu look" |
| 4 | ui-2d | ui-layout, ui-text-accessibility | số tham chiếu từ game-3 (bố cục, popup); skill cũ `game-ui-workflow` là bản nháp |
| 5 | art-pipeline | sprite-cutout, sprite-import-atlas, sprite-animation | đủ nguồn |
| 6 | 3d-look | toon-lookdev, ai-mesh-preparation | thêm số 3D toon và hiệu năng A07 từ game-5 (xem INVENTORY mục 6); thiếu mẫu penguin toon |
| 7 | puzzle-design | puzzle-rule-contract, puzzle-generation-check | đủ nguồn |
| 8 | feel-audio-meta | game-feedback, audio-qa, economy, save, service-adapter | cần thêm research |
| 9 | release | release-readiness, marketing-creative | thiếu research; để cuối |
Thứ tự đề xuất: 1 → 2 → 5 → 3 → 4 → 7 → 6 → 8 → 9. Phương án làm theo giai đoạn: viết skill 1-2 trước, dùng 1-2 phiên thật, đo kích hoạt, rồi mới viết tiếp (không cam kết 9 từ đầu).
Chưa có bằng chứng "9 skill nhỏ" tốt hơn "1 skill + references theo miền"; tài liệu chính thức không nói số tối đa hay cách tránh chồng lấn.

## 3. Lỗi lặp cần skill chặn (bằng chứng trong SKILL_INVENTORY mục 2.2-2.3)
Không đọc nền tảng theo lớp; làm lại asset đã duyệt (HOME_RETRO:16, LOSE_MODAL_RETRO:5); cắt ảnh còn viền mờ; brief gom nhiều việc; job nền không có cơ chế quay lại; shader mất trên build. Gốc [suy luận của Codex]: thiếu cơ chế buộc ÁP DỤNG tài liệu theo trigger (đòi bằng chứng đã áp dụng, không chỉ "đã đọc").

## 4. Kit (code) hiện trạng (đã quyết gộp vào khung game, xem mục 1 điểm 10 và `SHELL_PLAN.md`)
`~/project/indie-game/game-kit/` (repo riêng, private github.com/diptszyx/game-kit, commit 8a23228 + ce3e483): package `com.indiegame.ui`, Theme Kenney + gallery. EditMode 38/38, PlayMode 10/10. Tạm dừng thêm thành phần (toggle/slider/tab/toast/HUD) tới khi có mẫu UI. Bản trùng trong `game-2/unity/Packages/` chưa xóa. Skill cũ ghi nơi sửa kit là game-2: lỗi thời.

## 5. Cách theo dõi (monitor) mọi việc
- Mỗi việc lớn có brief trong `briefs/` (skill) và `indie-game/knowledge/game-kit/briefs/` (kit) (S1 kiểm kê; K* về kit). Job Codex chạy nền: `nohup codex exec ... < /dev/null > /tmp/k3b/<tên>.log`, canh bằng Monitor (liveness + file tiến độ + tín hiệu kết thúc). Monitor hết hạn sau 30 phút: dựng lại và nói rõ khi không còn canh.
- Kết quả Codex luôn được Claude kiểm lại độc lập (số, ảnh, đọc lại nguồn) trước khi báo.
- Cập nhật mục 0 và mục 2 của file này sau MỖI mốc, rồi commit.

## 6. Câu hỏi mở (cần người làm game)
1. Gộp 9 skill theo thứ tự trên, hay tách nhiều hơn / gom ít hơn? Làm theo giai đoạn (1-2 trước)? CHƯA TRẢ LỜI: người làm game còn muốn làm rõ thêm vấn đề trước khi bắt đầu viết (2026-11-11).
2. ~~Look kẹo bóng có phải mẫu chính thức cho skill 3-4?~~ ĐÃ THAY (2026-11-11): skill 3-4 dạy tạo phong cách bằng AI, game-3 / game-5 chỉ là mức chất lượng tham chiếu (mục 1, điểm 7).
3. Skill đặt trong repo riêng `game-skills`: ĐÃ CHỐT 2026-11-07.
4. Repo GitHub private `diptszyx/game-skills`: ĐÃ TẠO 2026-11-07.
5. Chưa kiểm: vì sao bài học game-5 nằm trong memory của game-3 (xem mục 1, điểm 9); và `game-3/docs/colony_flow/STATUS.md` mục 6 "Sai sót đã ghi nhận" có thêm bài nào ngoài C1-C8.
