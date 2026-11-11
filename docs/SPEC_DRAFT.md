# BẢN NHÁP KỸ THUẬT (spec, sẽ viết lại sau; plan đọc cho người nằm ở PLAN.md). Cập nhật 2026-11-11
Người làm game: solo, Unity, làm cùng AI (Claude điều phối, Codex viết mã / chạy Unity). Một công cụ trong repo này gồm hai phần: **khung game** (package Unity chứa phần vỏ dùng chung) và **bộ skill** (hướng dẫn Claude dùng khung và làm việc đúng quy trình). Không viết mã hay skill nào nếu chưa có bài học nguồn và cách kiểm.

## 0. Việc đang dở / bắt đầu phiên mới đọc cái này trước
- Trạng thái (2026-11-11): đã đọc game-5 và retro game-3, đã chốt hướng (mục 2). **CHƯA viết mã khung hay skill nào.** Việc kế: người làm game duyệt mục 9, rồi S0.
- Repo: `~/project/indie-game/game-skills/` (skill ở `skills/<tên>/SKILL.md`, symlink từ `~/.claude/skills/`). Đọc theo thứ tự: file này → `docs/SKILL_INVENTORY.md` (182 bài học + mục 6 game-3/game-5) → `docs/SKILL_AUTHORING_OFFICIAL.md` (10 quy tắc viết skill).
- Nguồn bằng chứng mới: `../game-3/docs/COLONY_FLOW_RETRO.md` (C1-C8), `../game-5/docs/HOLEIT_CLONE_RETRO.md` (R0-R9); cả hai chưa commit ở repo của chúng. Chỉ ĐỌC, không sửa game-3 / game-5 (phiên khác đang làm).
- Quyền tự chạy ở repo này: commit + push tự động (đường dẫn cụ thể), không `reset --hard`, không `pull --rebase`, không force push. Vẫn hỏi cho nhánh thật: đổi hướng đã khóa, tiêu tiền, xóa việc của người làm game, art / âm / cảm giác họ phải duyệt. Nguồn: memory `feedback-autonomy-game-kit` (2026-11-06), `docs/HANDOFF.md`.

## 1. Đích
Game mới dựng vỏ (Home, popup, Shop, Daily, Settings, Audio, kinh tế, ổ cắm nền tảng) bằng cách **dùng lại khung**, chỉ viết gameplay. Khung đổi giao diện được (theme + ảnh AI vẽ), không chứa hình lấy từ APK. Skill bảo đảm Claude làm đúng quy trình và không lặp lỗi cũ.
- Đích đo được (TỰ ĐẶT, chưa có nguồn, thay bằng số thật ở S7): giờ dựng vỏ game mới nhỏ < giờ dựng vỏ game-5. Giờ dựng vỏ game-5: CHƯA ĐO.

## 2. Quyết định đã chốt (người làm game)
1. ~~Trọng tâm là skill, không phải kit.~~ **ĐÃ THAY bởi điểm 10.**
2. Mọi hình do AI vẽ (Codex `image_gen`, Claude viết brief, người làm game duyệt). Không dùng sprite game thương mại (kể cả trích từ APK Colony Flow). Kenney (CC0) chỉ là nguồn phụ; look Kenney bị đánh giá xấu.
3. ~~Look mục tiêu kiểu game-3 (kẹo bóng...) là mẫu cho skill 3-4.~~ **ĐÃ THAY bởi điểm 7.** Game-2 KHÔNG phải chuẩn tham chiếu.
4. Skill phải tuân tài liệu chính thức (`docs/SKILL_AUTHORING_OFFICIAL.md`) và được kiểm: ≥ 3 tình huống (gồm tình huống KHÔNG nên kích hoạt), baseline không skill, phiên mới.
5. (2026-11-11) **Mục đích: làm game MỚI, không có APK để reverse.** Mọi thứ phải dùng được khi không có bản gốc để trích hoặc so. Bỏ ý định làm skill "clone-từ-APK"; quy trình trích APK (Il2Cpp, typetree, GLSL) không vào công cụ. game-3 (Colony Flow) và game-5 (Hole it!) đều là clone 1:1 để học, không phải look do người làm game chọn.
6. **Ảnh chuẩn của game mới = ảnh do người làm game duyệt** (style frame / mockup AI vẽ), thay cho "ảnh chụp bản gốc" của game-5. Giữ nguyên tắc: test xanh chưa đủ, mỗi màn và mỗi popup của luồng phải so với ảnh chuẩn (R2, R9, C7).
7. **Skill art (3-4) dạy TẠO và KHÓA một phong cách bằng AI** (Claude viết brief, Codex `image_gen`, người làm game duyệt, khóa thành style frame + bảng thông số). Không dùng ảnh game-3 làm tài nguyên (look game-3 trên máy là sprite gốc Colony từ APK, mâu thuẫn điểm 2). game-3 và game-5 chỉ là MỨC CHẤT LƯỢNG THAM CHIẾU bằng số, mỗi game một vai: game-3 cho bố cục UI 2D và popup, game-5 cho 3D toon và hiệu năng máy yếu (A07: FPS 49,6 và RAM PSS 786 MB, mục tiêu 58 và 600, đo 2026-11-10). Ghi đơn vị và game nguồn cạnh mỗi số.
8. **Skill 1 có bước đầu phiên: đọc quyền tự chạy** (memory + docs), hỏi một lần nếu chưa có, kèm danh sách nhánh vẫn phải hỏi (C8: người làm game nhắc 4 lần "đừng dừng hỏi từng chút").
9. **Bài học ghi vào `docs/` của repo NGAY lúc gặp lỗi**, memory chỉ là đường tắt: memory nằm theo thư mục làm việc (bài học game-5 bị ghi vào memory game-3) và cả hai retro tự nhận phải dựng lại sau (C0, R0). Ứng viên hook / bước bắt buộc trong skill 1.
10. (2026-11-11) **Gộp kit và skill thành MỘT công cụ trong MỘT plan** (file này): khung game + skill hướng dẫn dùng. Lý do: phần lặp thật giữa game-3 và game-5 là mã vỏ (Home, Shop, Daily, Audio, Haptics, Settings), kit cũ không có các phần đó. Làm plan trước, chưa viết mã. Khung gồm cả phần còn dùng được của `game-kit` (xem mục 3).
11. Khung chỉ chứa phần chung của vỏ; gameplay luôn riêng từng game. Hình đi vào khung chỉ qua theme.

## 3. Hiện trạng đo được (2026-11-11, đếm file / dòng, chưa đọc sâu)
| | game-3 (Colony Flow) | game-5 (Hole it!) | game-kit |
|---|---|---|---|
| C# không test | 12.213 dòng, 130 file | 9.880 dòng, 124 file | 1.081 dòng |
| Vỏ | Home + Audio ~1.372 dòng | Shell + Platform + Audio ~1.505 dòng | Modal, Theme, SafeArea, Validator |
| Gameplay | `Core/`, `Battle/` | `Rules/`, `Game/` | không có |
- Khối trùng tên ở cả hai game: Home, Shop (`ShopCatalog` / `ShopView`), Daily (`DailyRewards` / `DailyChallengeView`), Audio, Haptics (52 và 62 dòng, diff 85 dòng), Settings, Loading.
- Kit (`~/project/indie-game/game-kit/`, github private `diptszyx/game-kit`, commit 8a23228 + ce3e483): package `com.indiegame.ui`, Theme Kenney + gallery, EditMode 38/38, PlayMode 10/10. Không có Shop, Daily, kinh tế, Settings, Audio: đúng chỗ hai game lặp lại. Bản trùng trong `game-2/unity/Packages/` chưa xóa. Skill cũ `skills/game-ui-workflow` ghi nơi sửa kit là game-2: lỗi thời, chưa kiểm.
- Hình của vỏ hai game dựng từ prefab gốc APK, KHÔNG dùng lại được.
- Chỉ có 2 mẫu, cùng thể loại hybrid-casual, cùng là clone. Rule of three: mẫu thứ ba là game mới (S7).
- Phiên bản Unity: game-5 6000.3.25f1 (theo plan game-5); game-3 CHƯA KIỂM.

## 4. Khung game: ranh giới đề xuất (CHƯA CHỐT, S1 kiểm lại)
| Lớp | Nội dung | Giai đoạn |
|---|---|---|
| Nền tảng | Haptics, Settings (âm / rung), Audio (catalog + quy tắc), ổ cắm quảng cáo / IAP / Remote Config giả (`PlatformSockets`), `SaveData` | 1 (lặp rõ nhất) |
| Kinh tế | Một cổng vàng / tim / booster, kiểm số âm, test số dư | 1 |
| Popup | Win / Fail / Revive / Pause / Settings, vòng đời hiện-ẩn, an toàn màn hình, theme | 2 |
| Màn hình | Home, Shop (catalog + giá), Daily, Loading | 3 |
| Chỉ một game | Quest, Rolling Offer, Sky Jump, Balloon Streak, Leaderboard, Spine... | không vào khung tới lần lặp thứ 3 |
Mỗi lớp một assembly riêng, lớp trên phụ thuộc lớp dưới, không phụ thuộc gameplay. Luật thuần C# không tham chiếu `UnityEngine` (quy chuẩn `../../CLAUDE.md`).

## 5. Bộ skill đề xuất (Claude gộp 25 → 9; là đề xuất, không phải quy tắc Anthropic)
| # | Skill | Gộp từ (SKILL_INVENTORY mục 3) | Trạng thái |
|---|---|---|---|
| 1 | session-orchestration | session-context, codex-brief, job-monitor, research-decision | chưa viết; đủ nguồn; thêm: đọc quyền tự chạy, retro ngay lúc lỗi, một lần chạy Unity khi chia agent, canh agent bằng 2 nguồn (INVENTORY mục 6) |
| 2 | verification | unity-verification, visual-acceptance | chưa viết; đủ nguồn; ảnh chuẩn = ảnh người làm game duyệt (điểm 6), chụp MỌI popup của luồng (R9) |
| 3 | asset-reuse-art-direction | approved-asset-reuse, art-direction | dạy tạo và khóa phong cách bằng AI (điểm 7) |
| 4 | ui-2d | ui-layout, ui-text-accessibility | số tham chiếu từ game-3; dùng khung (lớp popup / theme); skill cũ `game-ui-workflow` là bản nháp |
| 5 | art-pipeline | sprite-cutout, sprite-import-atlas, sprite-animation | đủ nguồn |
| 6 | 3d-look | toon-lookdev, ai-mesh-preparation | thêm số 3D toon và hiệu năng A07 từ game-5; thiếu mẫu penguin toon |
| 7 | puzzle-design | puzzle-rule-contract, puzzle-generation-check | đủ nguồn |
| 8 | feel-audio-meta | game-feedback, audio-qa, economy, save, service-adapter | cần thêm research; phần economy / save / service-adapter nằm trong khung (lớp Nền tảng, Kinh tế) nên skill chỉ hướng dẫn dùng |
| 9 | release | release-readiness, marketing-creative | thiếu research; để cuối |
Thứ tự đề xuất: 1 → 2 → 5 → 3 → 4 → 7 → 6 → 8 → 9. Viết theo giai đoạn: skill 1-2 trước, dùng 1-2 phiên thật, đo kích hoạt, rồi mới tiếp (không cam kết 9 từ đầu). Chưa có bằng chứng "9 skill nhỏ" tốt hơn "1 skill + references theo miền".

## 6. Lỗi lặp cần chặn (bằng chứng: SKILL_INVENTORY mục 2.2-2.3 và 6.1)
Không đọc nền tảng theo lớp; làm lại asset đã duyệt (HOME_RETRO:16, LOSE_MODAL_RETRO:5); cắt ảnh còn viền mờ; brief gom nhiều việc; job nền không có cơ chế quay lại; shader mất trên build; báo hình ảnh xong khi chưa so ảnh chuẩn (C7, R2, R9); số tự đặt (C1, R1); mỗi agent tự chạy Unity (R5, R8). Gốc [suy luận của Codex]: thiếu cơ chế buộc ÁP DỤNG tài liệu theo trigger (đòi bằng chứng đã áp dụng, không chỉ "đã đọc").

## 7. Bảng bước
Cột "Ai": **C** = Claude, **X** = Codex, **N** = người làm game. Bước nào chưa có lệnh kiểm thì Claude viết trước khi giao.
| # | Việc | Ai | Kiểm | Phụ thuộc |
|---|---|---|---|---|
| S0 | Bảng đối chiếu từng file vỏ game-3 vs game-5 → `docs/SHELL_COMPARE.md` (file, giống / gần giống / khác / một game, phụ thuộc prefab gốc). Đọc mã thật, chỉ đọc; ghi commit và thời điểm đọc | C | Mọi file trong `Home`, `Audio` (game-3) và `Game/Shell`, `Game/Platform`, `Audio` (game-5) có đúng một dòng; mẫu 5 dòng được đọc lại độc lập | |
| S1 | Chốt ranh giới và API mức giao diện (không mã) cho giai đoạn 1; Codex phản biện CHỈ ĐỌC bằng dữ liệu S0 | C + X | Từng phát hiện của Codex được Claude kiểm (đúng / sai / chưa kiểm), kết quả ghi vào mục 4 | S0 |
| S2 | Duyệt ranh giới, phạm vi giai đoạn 1, tên package | N | Người làm game gật ở mục 9 | S1 |
| S3 | Quyết vị trí và cấu trúc: thư mục trong repo, assembly, phiên bản Unity (kiểm game-3), quan hệ với `game-kit` (giữ Validator / Modal nếu dùng được, lưu trữ phần còn lại) | C | Ghi vào mục 2; cấu trúc package mở được trong project Unity thử | S2 |
| S4 | Giai đoạn 1: rút Haptics, Settings, Audio, `SaveData`, kinh tế, ổ cắm nền tảng vào package kèm test EditMode | X | `tests/run_unity.sh` xanh, `error CS` = 0, mỗi lớp có test số dư / bật tắt; Claude chạy lại cổng độc lập | S3 |
| S5 | Giai đoạn 2: popup + theme + an toàn màn hình; ảnh chuẩn do người làm game duyệt | X + N | Ảnh ghép so ảnh chuẩn ở 16:9 và 20:9, mọi popup của luồng | S4 |
| S6 | Giai đoạn 3: Home, Shop, Daily, Loading | X | Như S5 | S5 |
| S7 | Kiểm bằng game mới nhỏ (3 màn): dựng vỏ chỉ bằng khung, ghi GIỜ đo được | C + X | Giờ thật vào mục 1, thay số TỰ ĐẶT | S4 (gđ 1), S6 (đủ) |
| S8 | Viết skill 1-2 (và hướng dẫn dùng khung), kiểm ≥ 3 tình huống + baseline + phiên mới | C | Theo `SKILL_AUTHORING_OFFICIAL.md`; kết quả ghi vào `docs/` | S2 (skill 1-2 không cần chờ khung) |
| S9 | Các skill còn lại theo thứ tự mục 5, mỗi skill sau khi skill trước được dùng thật và đo kích hoạt | C | Như S8 | S8 |
Cổng mỗi bước: xanh thì commit + push repo này rồi sang bước kế; đỏ thì dừng và báo.

## 8. Cách theo dõi
- Mỗi việc lớn có brief trong `briefs/`. Job Codex nền: `nohup codex exec ... < /dev/null > /tmp/<tên>.log`, canh bằng Monitor (liveness + file tiến độ + tín hiệu kết thúc). Monitor hết hạn sau 30 phút: dựng lại và nói rõ khi không còn canh.
- Kết quả Codex luôn được Claude kiểm lại độc lập (số, ảnh, đọc lại nguồn) trước khi báo.
- Cập nhật mục 0 của file này sau MỖI mốc, rồi commit.

## 9. Cần người làm game duyệt / câu hỏi mở
1. Ranh giới mục 4 và thứ tự giai đoạn 1 → 2 → 3. (khuyến nghị: duyệt)
2. Chỉ giai đoạn 1 làm trước, đo, rồi mới tiếp (khuyến nghị: có, vì mới 2 mẫu).
3. Tên package (gợi ý: `com.indiegame.shell`).
4. Gộp 9 skill theo thứ tự mục 5, làm theo giai đoạn (1-2 trước)? CHƯA TRẢ LỜI (2026-11-11).
5. Chưa kiểm: vì sao bài học game-5 nằm trong memory game-3 (điểm 9); `game-3/docs/colony_flow/STATUS.md` mục 6 "Sai sót đã ghi nhận" có thêm bài nào ngoài C1-C8 không.
Đã chốt: skill đặt trong repo riêng `game-skills` (2026-11-07); repo GitHub private `diptszyx/game-skills` đã tạo (2026-11-07).

## 10. Rủi ro
| # | Rủi ro | Cách giảm |
|---|---|---|
| R1 | Rút khung quá sớm từ 2 clone cùng thể loại, game mới lại khác | Chỉ giai đoạn 1; S7 dùng game mới làm mẫu thứ ba; chỗ chỉ một game không vào khung |
| R2 | Mã vỏ game-3 / game-5 phụ thuộc prefab gốc APK nên khó tách | S0 ghi rõ phụ thuộc từng file; khung nhận hình qua theme |
| R3 | Lặp lỗi kit cũ: làm lâu, kết quả xấu | Giai đoạn 1 không có UI; popup chỉ làm khi có ảnh chuẩn do người làm game duyệt |
| R4 | Hai game đang được phiên khác sửa, đọc phải bản lệch | Ghi commit / thời điểm đọc vào `SHELL_COMPARE.md`; không sửa hai game |
| R5 | Con số "nhanh hơn" không có nguồn | Giữ nhãn TỰ ĐẶT tới S7 |

## 11. Câu hỏi mở và điều chưa biết (2026-11-11, chuyển từ plan người đọc)
- Cần người làm game duyệt: danh sách thứ đưa vào khung; làm chức năng nền (D) trước rồi đo, mới làm màn hình (E); bắt đầu bước A.
- Tiết kiệm bao nhiêu giờ: chưa đo, chỉ biết sau bước F.
- AI có vẽ ra bộ hình nhất quán không: chưa thử trong công cụ này (bước C kiểm); hình AI hay lệch phong cách, viền xấu khi cắt (SKILL_INVENTORY mục 1.3).
- Mới có 2 game để so, cùng thể loại, đều là clone; chỉ đưa vào khung phần thật sự giống nhau.
- Hình hai game cũ lấy từ game gốc nên không dùng lại; chỉ dùng lại cách tổ chức và logic.
- Phần chỉ có ở một game (Quest, Rolling Offer, Sky Jump...) chưa vào khung.
- Phân vai: Claude điều phối, đọc, kiểm; Codex viết mã, vẽ ảnh AI, chạy Unity; người làm game chọn, duyệt, thử trên điện thoại.

## 12. Quyết định 3D (2026-11-11, người làm game): dựng đồ vật 3D bằng Claude + Blender, không dùng AI ảnh-sang-3D
- Bằng chứng ủng hộ: game-3 dựng 3 mesh bằng script Blender (ô 198 tam giác, kiến 1.616, hộp 2.584), quy trình đo số → Blender → nướng AO → shader toon (`knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md` dòng 5-45).
- Bằng chứng phản đối: game-2 dựng bằng script Blender 3 lần "không đạt" cho vật cần chi tiết vẽ tay (vân băng, vết nứt), sau đó chuyển sang AI ảnh-sang-3D và gặp topology xấu (`game-2/docs/ART.md` dòng 69-73, 131-134).
- Hệ quả: phong cách chọn ở xưởng phải là kiểu mỗi bộ phận một màu, hình khối đơn giản; bước C thử dựng một đồ vật 3D để kiểm.
- CHƯA KIỂM: dựng đồ vật mới từ ảnh concept (game-3 dựng theo số đo từ game gốc); nguyên nhân thật của 3 lần thất bại ở game-2 (cách viết script hay chủ đề quá khó).
- Việc chạy Blender: Codex chạy với quyền đầy đủ (`codex exec -s danger-full-access`), plugin sandbox giấu GPU nên có thể làm Blender sập (`~/.claude/CLAUDE.md`). Blender có ở `/Applications/Blender.app`.
