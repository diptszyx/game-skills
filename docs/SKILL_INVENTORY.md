# Kiểm kê bài học để xây bộ skill cho Claude — S1

Ngày kiểm: 2026-11-07. Phạm vi: đọc tài liệu; không chạy Unity/git, không sửa kit/mã/skill, không đọc reference/apk hoặc nhị phân.
Trạng thái: hoàn tất kiểm kê 12 miền, 182 bài học; kiểm chéo và giới hạn phạm vi ở cuối file.

## Cách đọc và phạm vi áp dụng
- Mỗi hàng: ID + hành động kiểm được | nguồn file:dòng | xuất xứ game | độ phủ trong skill hiện tại.
- “Có” = skill hiện tại có chỉ dẫn tương ứng; “thiếu” = chưa có đủ chỉ dẫn chuyên môn đó; “mâu thuẫn” = hướng dẫn hiện tại lệch nguồn mới. Có một phần ghi rõ phần còn thiếu. Đây là đối chiếu văn bản, không phải test kích hoạt skill.
- G1 = Memory Thief; G2 = puzzle cánh cụt; G3 = bài tập clone Colony Flow; CF = game gốc được tài liệu nghiên cứu mô tả. Số của CF là số tài liệu đã ghi, không phải tôi đo lại trong lượt này.
- [suy luận] đánh dấu khuyến nghị tổng hợp/khái quát của kiểm kê; ngưỡng riêng game không trở thành chuẩn chung. Không chép art, mã hay thông số thành bản sắc game mới.
- Giữ chín miền gợi ý; thêm art-direction, motion/game-feel và architecture/data vì chúng có đầu ra riêng (ảnh đích, dòng thời gian phản hồi, hợp đồng dữ liệu), không nên bị chìm trong UI/3D hoặc điều phối.

## 1. Bài học theo miền

### 1.1 ui-2d — bố cục, chữ, tương tác
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| U01. Kiểm kê từng thành phần UI và gắn tiêu chí, trạng thái, bằng chứng trước khi báo độ hoàn thiện. | game-1/docs/UI.md:3; lessons/bai-hoc-chung.md:123 | G1 | thiếu |
| U02. Đọc luồng chính trước, rồi rà interaction, UI, motion và lượt usability phủ lại toàn bộ. | learning/ui-ux/UI_UX_FUNDAMENTALS.md:169 | chung | thiếu |
| U03. Tìm sheet/component đã duyệt và ghi tên vào brief trước khi đặt bố cục mới. | game-2/docs/LOSE_MODAL_RETRO.md:5; game-2/docs/WORKFLOW_RETRO.md:15 | G2 | có (§1) |
| U04. Mở contact sheet trước khi mô tả mảnh panel/ribbon để tránh ghép tiêu đề hai lần. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:14 | G2/K0b | có |
| U05. Kiểm cách dùng sprite, tỉ lệ và 9-slice trước khi đề nghị sinh lại art đã duyệt. | game-2/docs/HOME_RETRO.md:20 | G2 | có (§1) |
| U06. Căn nhãn theo mặt nút thật bằng FaceLabel thay vì tâm hình học gồm cả phần hông. | game-2/docs/LOSE_MODAL_RETRO.md:5 | G2 | có (§1, §6); thiếu giải thích hình học |
| U07. Đo pixel icon không giao viền bằng cặp ảnh hiện/ẩn; rect nằm trong thanh chưa đủ. | game-2/docs/HOME_RETRO.md:24; lessons/bai-hoc-chung.md:128 | G2 | thiếu |
| U08. Dùng anchor/layout, safe-area cho nội dung và cover riêng cho nền; kiểm không lồng safe-area hai lần. | game-3/docs/colony_flow/CODE_HOME_BATTLE_FROM_APK.md:249; game-3/docs/colony_flow/CODE_HOME_BATTLE_FROM_APK.md:250 | G3/CF | thiếu cách phân vùng; có UiSafeArea |
| U09. Chụp mọi màn ở 16:9, 20:9 và mọi ngôn ngữ được hỗ trợ, kiểm nút nằm trọn màn. | game-1/docs/PHASE1_RETRO.md:52; lessons/bai-hoc-chung.md:124 | G1/G2 | có hai tỉ lệ; thiếu mọi ngôn ngữ |
| U10. Kiểm glyph và độ phân biệt các cặp chữ có dấu trên cùng màn với các font ứng viên trước khi chốt. | game-1/docs/PHASE1_RETRO.md:68; game-1/docs/ART.md:171 | G1 | thiếu |
| U11. Chọn font asset và material bóng/viền theo nền sáng/tối, kiểm chữ thật sau render. | knowledge/colony-flow-research/03-ui-va-material.md:19; game-3/docs/colony_flow/CODE_HOME_BATTLE_FROM_APK.md:241 | G3/CF | có dữ liệu Theme; thiếu bộ quy tắc theo nền |
| U12. Đo tương phản chữ trên ảnh và ghi đơn vị vùng chạm; không coi px tham chiếu là dp/mm vật lý. | game-1/docs/UI.md:26; game-1/docs/UI.md:32; learning/ui-ux/UI_UX_FUNDAMENTALS.md:192 | G1/chung | thiếu (cảnh báo contrast ở §7 chưa thành cổng) |
| U13. Thiết kế riêng thường/khoá/rỗng/tải/lỗi/hết tài nguyên, không suy tải từ danh sách rỗng. | knowledge/colony-flow-research/00-tong-quan-cong-nghe.md:53; learning/ui-ux/UI_UX_FUNDAMENTALS.md:180 | CF/chung | thiếu |
| U14. Cho mọi nút dùng chung phản hồi nhấn, âm và trạng thái khóa; kiểm từng đường tương tác. | knowledge/colony-flow-research/03-ui-va-material.md:83 | CF | có motion/locked; thiếu âm mặc định |
| U15. Kiểm chạm xuyên, bấm đúp, chạm giữa transition và mở lại khi đang đóng; Back chỉ tác động modal trên cùng. | game-1/docs/UI.md:30; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:40 | G1/G2 | có modal stack/hủy motion; thiếu bộ ca chạm đầy đủ |
| U16. Tìm lý do một hướng dẫn tồn tại và thử hé lộ dần trước khi xóa để giảm chữ/dòng code. | game-1/docs/PHASE1_RETRO.md:54; game-2/docs/GAMEPLAY_UI_RETRO.md:19 | G1/G2 | thiếu |
| U17. Giữ một prefab nền là nơi duy nhất định nghĩa khung; kiểm abstraction hiện có trước khi thêm loại mới. | game-2/docs/META_UI_PLAN.md:92 | G2 | có tìm cái sẵn; thiếu nguyên tắc liên kết prefab |
| U18. Khi chỉ đổi Theme, kiểm A→B→A, chữ dài/ngắn và trạng thái khóa trên cả hai nền. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:45; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:47 | G2/kit | có A→B→A; thiếu ma trận ca nội dung |
| U19. Đọc cả HUD và bàn phía sau popup trong ảnh nghiệm thu, kiểm số ở khung kết thúc. | game-2/docs/LOSE_MODAL_RETRO.md:7 | G2 | thiếu |
| U20. Chỉ coi cấu trúc round-trip khớp là bằng chứng cho thuộc tính đã kiểm; ghi rõ sprite placeholder và component bị loại trừ. | game-3/docs/colony_flow/IMPORTER_NOTES.md:28; game-3/docs/colony_flow/IMPORTER_NOTES.md:29 | G3 | thiếu |

### 1.2 art-direction — chốt ảnh đích và khung kỹ thuật
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| D01. Chốt phép chiếu, hướng nhân vật, cỡ, palette, viền và hướng sáng bằng ảnh ghép trong game trước mọi asset. | game-1/docs/PHASE1_RETRO.md:27; workflow/quy-trinh-phase.md:29 | G1/G2 | thiếu |
| D02. Có ảnh full-screen đích đúng màn đang dựng trước khi chỉnh số hình ảnh. | game-2/docs/PENGUIN3D_RETRO.md:59; lessons/bai-hoc-chung.md:213 | G2 | thiếu |
| D03. Làm một đơn vị từ concept tới ảnh engine/máy thật đạt trước khi nhân cả bộ. | lessons/bai-hoc-chung.md:80; game-2/docs/HOME_RETRO.md:7 | G2 | thiếu |
| D04. Phân tích nhiều tác phẩm thật, ghi nguyên tắc và khác biệt với pillars riêng trước khi chốt style. | lessons/bai-hoc-chung.md:29; game-1/docs/PHASE1_RETRO.md:63 | G1/G2 | thiếu |
| D05. Chẩn đoán ảnh theo độ sáng → bố cục → màu → chất liệu và kiểm lại bằng ảnh xám. | learning/game/pixel-art-fundamentals.md:267 | chung | thiếu |
| D06. Mô tả điều thấy rồi nêu dấu hiệu trên ảnh trước khi phán, coi nhận xét AI là giả thuyết. | learning/game/pixel-art-fundamentals.md:94; learning/game/pixel-art-fundamentals.md:116 | chung | thiếu |
| D07. Gán nghĩa màu vào một từ điển, kiểm màu phe hai chiều và thêm ký hiệu cho thông tin quan trọng. | game-1/docs/ART.md:79; game-1/docs/PHASE1_RETRO.md:46; game-1/docs/ART.md:97 | G1 | thiếu |
| D08. Đo palette sau render/hoàn thiện và bảo vệ màu mang luật khỏi grade. | lessons/bai-hoc-chung.md:91; lessons/bai-hoc-chung.md:94; game-1/docs/ART.md:52 | G1/G2 | thiếu |
| D09. Hiệu chỉnh thước đo trên mẫu tốt/xấu; không dùng phần trăm pixel rực làm cổng đẹp. | knowledge/08-mau-va-anh-sang.md:53 | G2 | có nguyên tắc số không chấm đẹp; thiếu hiệu chỉnh |
| D10. Giữ nền lùi về chi tiết/tương phản và kiểm vùng chơi còn nổi khi làm mờ cả màn. | knowledge/08-mau-va-anh-sang.md:83; knowledge/08-mau-va-anh-sang.md:87 | G2/nghiên cứu blog | thiếu |
| D11. Kiểm silhouette, tỉ lệ, hướng sáng, phép chiếu và mật độ chi tiết của cả họ asset ở cỡ sử dụng. | game-1/docs/ART.md:187; game-1/docs/ART.md:194 | G1 | thiếu |
| D12. Lưu Gold Standard từ engine và báo rõ bản nháp còn thiếu lớp nào trước khi đưa duyệt. | game-1/docs/ART.md:198; lessons/bai-hoc-chung.md:98 | G1/G2 | thiếu |

### 1.3 art-pipeline — ảnh AI → sprite → atlas/9-slice
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| A01. Liệt kê asset đã duyệt đúng chủ thể trước khi sinh; dùng ảnh đó làm gốc biến thể. | game-2/docs/HOME_RETRO.md:16; game-2/docs/UI_MATTE_RETRO.md:5 | G2 | có tái dùng UI; thiếu pipeline ảnh |
| A02. Sinh một mẫu duyệt và chạy trọn import/hiển thị rồi mới sản xuất hàng loạt. | lessons/bai-hoc-chung.md:15; lessons/bai-hoc-chung.md:80 | G1/G2 | thiếu |
| A03. Tạo ảnh trạng thái đầy đủ của cảnh trước; suy các trạng thái bớt món từ cùng nguồn. | game-2/docs/HOME_RETRO.md:8 | G2 | thiếu |
| A04. Đếm từng món theo danh sách và gán file theo danh tính/nội dung, kiểm hash/pixel thay vì tin thứ tự ảnh. | game-2/docs/HOME_RETRO.md:9; game-2/docs/HOME_RETRO.md:10; game-2/docs/HOME_RETRO.md:13; game-2/docs/PENGUIN2D_RETRO.md:14 | G2 | thiếu |
| A05. Cắt nền từ biên bằng flood fill thay vì xoá màu toàn ảnh, rồi kiểm alpha thật và bbox. | lessons/bai-hoc-chung.md:142; game-2/docs/HOME_RETRO.md:14 | G2 | thiếu |
| A06. Kiểm mỗi lớp trên nền sáng/tối/đích và ở trạng thái dịch khỏi nền, không chỉ MAE khi ghép lại nguồn. | game-2/docs/HOME_RETRO.md:30; game-2/docs/ASSET_PIPELINE.md:49 | G2 | thiếu |
| A07. Khử nhiễm màu rìa ngay trong bước cắt và soi từng icon phóng to trên nền tối. | game-2/docs/UI_MATTE_RETRO.md:14 | G2 | thiếu |
| A08. Kiểm sương alpha, vật vụn, pixel đặc ở viền và độ phủ hình gốc trước khi nhận sprite cắt. | game-2/docs/SPRITE_RETRO.md:25; game-2/docs/HOME_RETRO.md:34 | G2 | thiếu |
| A09. Không chỉ giữ khối liên thông lớn nhất khi hình có tia sáng/chi tiết rời hợp lệ. | game-2/docs/UI_MATTE_RETRO.md:17 | G2 | thiếu |
| A10. Chọn công cụ theo nhiệm vụ: SAM chọn vật; refinement tinh viền; LaMa lấp nền; pymatting khử viền, kèm QA từng bước. | lessons/bai-hoc-chung.md:65; lessons/bai-hoc-chung.md:68; lessons/bai-hoc-chung.md:69; game-2/docs/ASSET_PIPELINE.md:46 | G2 | thiếu |
| A11. Gắn cờ confidence thấp và buộc xem QA; cache phải khớp nguồn/prompt trước khi tái dùng. | game-2/docs/ASSET_PIPELINE.md:46 | G2 | thiếu |
| A12. Dùng ảnh đảo đã duyệt làm nguồn duy nhất khi tách vật động, không sinh lại cả đảo. | game-2/docs/ASSET_PIPELINE.md:42; game-2/docs/POLISH_RETRO.md:16 | G2 | thiếu |
| A13. Đi đúng quy trình neo hướng của sprite-gen và đo cỡ trên ảnh nguồn trước khi cắt khung. | game-1/docs/PHASE1_RETRO.md:70; game-1/docs/PHASE1_RETRO.md:71 | G1 | thiếu |
| A14. Kiểm đầu ra thực sự đổi khi sửa prompt; ép sinh lại nếu công cụ bỏ hàng đã có. | game-2/docs/SPRITE_RETRO.md:17 | G2 | có (skill sprite-import-atlas) |
| A15. Khớp camera, neo chân, bóng tiếp đất và ánh sáng sprite với sàn trước khi sản xuất chuyển động. | game-2/docs/SPRITE_RETRO.md:30; game-2/docs/PENGUIN2D_RETRO.md:13 | G2 | có (skill sprite-import-atlas) |
| A16. Giữ PNG nguồn, build atlas theo màn/tính năng và kiểm nén Android, không sửa trực tiếp trang atlas. | knowledge/colony-flow-research/11-sprite-png-atlas.md:32; knowledge/colony-flow-research/11-sprite-png-atlas.md:35 | CF/G2 | có (skill sprite-import-atlas) |
| A17. Đặt Full Rect, border và Image Sliced cho khung co giãn; kiểm border của sprite đang dùng và rect tối thiểu. | lessons/bai-hoc-chung.md:134; game-2/docs/UI_MATTE_RETRO.md:15 | G2 | có (skill sprite-import-atlas) |
| A18. Chọn mipmap theo mức thu nhỏ và đo A/B trên máy, không áp lệnh tắt mipmap UI cho mọi sprite. | game-2/docs/UI_MATTE_RETRO.md:16; knowledge/colony-flow-research/11-sprite-png-atlas.md:33 | G2/CF | có (skill sprite-import-atlas) |
| A19. Tách preset pixel-art (nearest, integer scale, alpha cứng) khỏi UI vẽ mịn và kiểm sau import. | game-1/docs/ART.md:32; learning/game/pixel-art-fundamentals.md:389; learning/game/pixel-art-fundamentals.md:392 | G1/chung | có (skill sprite-import-atlas) |
| A20. Ghi prompt, model/phiên bản, nguồn và phép sinh lại ở nơi bền; đổi nguồn chung phải liệt kê đầu ra cần làm lại. | game-1/docs/ART.md:21; game-1/docs/PHASE1_RETRO.md:69; game-2/docs/ART.md:126 | G1/G2 | có (skill sprite-import-atlas) |

### 1.4 3d-look — hình, material, đèn, render, toon
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| T01. Báo trạng thái từng lớp hình → material → đèn/render → art direction; ghi cách xử lý grade trong lớp render. | 3d/3d-fundamentals.md:9; CLAUDE.md:24; game-2/docs/PENGUIN3D_RETRO.md:38 | G2/chung | thiếu |
| T02. Chốt camera và palette chung trước khi chọn shader, rồi thử một cảnh nhỏ so ảnh đích. | knowledge/colony-flow-research/09-so-sanh-chi-tiet-game-2.md:83 | G2/CF | thiếu |
| T03. Phân biệt toon và stylized PBR theo ảnh đích, không bắt mọi cảnh bật bloom/tone mapping. | 3d/3d-map.md:374; knowledge/colony-flow-research/08-3d-do-tu-du-lieu-goc.md:18 | chung/CF | thiếu |
| T04. Với look toon theo bộ phận, chia mesh/material theo vai trò và dùng cặp màu sáng/bóng cùng hệ. | knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:31; knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:35 | CF/G2 | thiếu; chưa thử cánh cụt (nguồn:37) |
| T05. Xem các tham số toon CF theo vật liệu cụ thể; kiểm cờ dùng rim/spec/MatCap trước khi sao số. | game-3/docs/colony_flow/LOOK_findings.md:25; game-3/docs/colony_flow/LOOK_findings.md:26; game-3/docs/colony_flow/LOOK_findings.md:31 | G3/CF | thiếu |
| T06. Phân biệt default serialized, material được gán và render runtime; hiệu chỉnh màu theo đường chuyển linear/sRGB. | game-3/docs/colony_flow/ANT_MEASUREMENTS_2026-11-05.md:113; game-3/docs/colony_flow/ANT_MEASUREMENTS_2026-11-05.md:122; game-2/docs/PENGUIN3D_RETRO.md:33 | G2/G3 | thiếu |
| T07. Đo mesh bản thô nhiều góc cạnh concept trước khi sửa hoặc kết luận model kém. | game-2/docs/ART.md:112; game-2/docs/BOARD3D_RETRO.md:48 | G2 | thiếu |
| T08. Kiểm component/cạnh hở/pháp tuyến, làm kín nếu cần trước khi giảm và bake. | game-2/docs/ART.md:113; game-2/docs/BOARD3D_RETRO.md:45 | G2 | thiếu |
| T09. Giảm mesh theo loại hình và theo nấc, so silhouette ba góc thay vì decimate một phát. | game-2/docs/ART.md:115 | G2 | thiếu; số ngưỡng là quy chuẩn G2 |
| T10. Tạo UV mới, bake có cage/ray distance và nghiệm thu từ chính FBX/texture xuất ra. | game-2/docs/ART.md:116; game-2/docs/ART.md:117; game-2/docs/ART.md:118 | G2 | thiếu |
| T11. Với concept chứa vân/ánh sáng vẽ sẵn, thử chiếu concept khớp camera và tránh nhân sáng hai lần; không mặc định áp cho toon chia bộ phận. | game-2/docs/ART.md:93; game-2/docs/ART.md:94; knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:37 | G2/CF | thiếu; cần phân xử C02 |
| T12. Đo trục/bounds sau import và giữ xoay nhập khi ghép transform; không đoán Euler. | game-2/docs/BOARD3D_RETRO.md:8; game-2/docs/BOARD3D_RETRO.md:15; game-2/docs/PENGUIN3D_RETRO.md:34 | G2 | thiếu |
| T13. Dùng một đơn vị lưới/pivot/camera, duyệt vật cả trên cụm ô và đứng riêng. | game-2/docs/KIT_SPEC.md:6; game-2/docs/KIT_SPEC.md:30 | G2 | thiếu |
| T14. Với viền mô-đun, xoay cùng góc đối xứng, khớp mật độ texel và đo khoảng cách mặt cắt đa tuyến; bỏ nắp ở mối lặp. | game-2/docs/KIT_SPEC.md:38; game-2/docs/KIT_SPEC.md:39; game-2/docs/KIT_SPEC.md:40; game-2/docs/BOARD3D_RETRO.md:59 | G2 | thiếu |
| T15. Hàn đỉnh trùng theo vị trí trước khi sửa mesh có seam UV, rồi kiểm nứt bằng ảnh engine có bóng. | game-2/docs/PENGUIN3D_RETRO.md:52 | G2 | thiếu |
| T16. Nướng AO/ánh sáng tĩnh khi phù hợp và đo chi phí hậu kỳ trên máy yếu trước khi giữ. | 3d/3d-map.md:258; knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:26; lessons/bai-hoc-chung.md:212 | G2/G3 | thiếu |
| T17. Đặt ngân sách bằng số lượng × độ phức tạp; ưu tiên mesh rẻ cho vật xuất hiện nhiều. | knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:18 | G3 | thiếu; 198/1616/2584 là mesh clone |
| T18. Cân nhắc instancing/pool/VAT khi nhiều vật giống nhau, rồi đo draw call/frame time thật. | knowledge/colony-flow-research/04-hien-thi-va-hieu-nang.md:35; knowledge/colony-flow-research/04-hien-thi-va-hieu-nang.md:50; 3d/3d-map.md:399 | CF/chung | thiếu; mốc 100 vật chỉ đề xuất |
| T19. Kiểm shader/material thật được nạp và không bị strip trước khi diễn giải số GPU đẹp. | game-2/docs/PENGUIN3D_RETRO.md:36; game-2/docs/SPRITE_RETRO.md:19 | G2 | thiếu |
| T20. Xuất biến shader trung gian thành RGB để chẩn đoán lỗi màu thay vì sửa liên tiếp theo giả thuyết. | game-2/docs/PENGUIN3D_RETRO.md:39 | G2 | thiếu |

### 1.5 unity-verification — cổng logic, hình và thiết bị
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| V01. Xác nhận biên dịch hiện tại sạch và lỗi CS mới bằng mốc log trước khi tin số test. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:24; game-2/docs/BOARD3D_RETRO.md:18 | G2 | có; khác CLAUDE cũ (C03) |
| V02. Xóa kết quả test cũ hoặc kiểm dấu lượt chạy trước khi đọc báo cáo xanh. | lessons/bai-hoc-chung.md:182; game-2/docs/VIEWGATE_RETRO.md:29 | G2 | thiếu |
| V03. Ép refresh, chờ hết compile và kiểm dấu bản sửa trước khi chụp Editor chạy nền. | game-2/docs/BOARD3D_RETRO.md:60; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:51 | G2 | có refresh; thiếu kiểm dấu phiên bản |
| V04. Cho test đi qua input thật với EventSystem và input module đúng, không chỉ Invoke callback. | game-2/docs/HOME_RETRO.md:19; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:67 | G2/kit | có bootstrap; thiếu DoD input toàn luồng |
| V05. Khởi động nguội từ dữ liệu lưu và thao tác người dùng thật, kể cả hết tim trước khi nạp bàn. | game-2/docs/LOSE_MODAL_RETRO.md:9; game-2/docs/WORKFLOW_RETRO.md:14 | G2 | thiếu |
| V06. Dùng InputTestFixture, nạp scene sau Setup và dọn input/scene/static ở TearDown. | game-2/docs/WORKFLOW_RETRO.md:20; game-2/docs/POLISH_RETRO.md:20; game-2/docs/BOARD3D_RETRO.md:42 | G2 | có fixture; thiếu lifecycle đủ |
| V07. Lỗi chỉ khi chạy nhóm thì tìm test gây ô nhiễm bằng cặp và log trạng thái, không thêm chờ theo đoán. | game-2/docs/POLISH_RETRO.md:22; lessons/bai-hoc-chung.md:196 | G2 | thiếu |
| V08. Thay dịch vụ mạng bằng bản giả trong test dùng thư mục lưu tạm; khi lỗi nổi ở test kế tiếp, đọc stack trace trong output thay vì đoán scene còn sống. | game-2/docs/WORKFLOW_RETRO.md:21 | G2 | thiếu |
| V09. Chạy test liên quan trong vòng sửa, cổng nhóm sau thay đổi, cổng đầy đủ ở mốc hoặc khi đổi mã chung. | workflow/lam-viec-voi-codex.md:7; lessons/bai-hoc-chung.md:228; game-2/docs/POLISH_RETRO.md:18 | G2 | thiếu chính sách phạm vi |
| V10. Dùng guard đo tiến triển và tên test, phân biệt bộ test lâu với test treo thật. | game-2/docs/WORKFLOW_RETRO.md:12; game-2/docs/WORKFLOW_RETRO.md:13 | G2 | có mẹo dialog; thiếu guard chung |
| V11. Giữ một Editor/project và một chủ sở hữu phiên Unity, không reload giữa test của job khác. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:30; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:32 | G2 | có |
| V12. Kiểm scene/camera/build settings đang dùng trước khi tin ảnh và tránh builder thử ghi đè danh sách scene chung. | game-2/docs/GAMEPLAY_UI_RETRO.md:20; game-2/docs/SPRITE_RETRO.md:20 | G2 | thiếu |
| V13. Dùng RenderTexture đúng kích thước khi batch không đổi Screen.SetResolution, tính click qua cùng camera. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:70; knowledge/game-kit/GAME_KIT_PLAN.md:137 | kit/G3 | có; chưa cập nhật bẫy K9 local |
| V14. Người kiểm tự mở ảnh toàn màn, crop 100% và các mốc giữa hoạt cảnh trước khi nhận kết quả. | CLAUDE.md:67; game-2/docs/POLISH_RETRO.md:9; lessons/bai-hoc-chung.md:165 | G1/G2 | có ảnh hai tỉ lệ; thiếu giữa hoạt cảnh/crop |
| V15. Phân biệt golden cấu trúc, golden pixel và kiểm hành vi; thử làm hỏng có chủ ý để chứng minh cổng bắt được. | game-3/docs/colony_flow/IMPORTER_NOTES.md:28; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:26; game-2/docs/PENGUIN2D_RETRO.md:16 | G2/G3 | có validator/golden; thiếu phân biệt và mutation chung |
| V16. Kiểm công cụ đo bằng ca sạch/hỏng và phép đếm độc lập, kể cả dtype và alpha. | game-2/docs/PENGUIN2D_RETRO.md:16; game-2/docs/VIEWGATE_RETRO.md:26 | G2 | thiếu |
| V17. Quét cả họ prefab/sprite sau một lỗi hình ảnh thay vì sửa một mẫu rồi báo xong. | game-2/docs/WORKFLOW_RETRO.md:18 | G2 | thiếu |
| V18. Đọc metadata asset hoặc kiểm quan hệ thay vì đóng cứng rect/pivot vào nhiều test. | lessons/bai-hoc-chung.md:195 | G2 | thiếu |
| V19. Kiểm đúng app đang foreground, ảnh không đen, log Unity không lỗi trước khi dùng kết quả điện thoại. | lessons/bai-hoc-chung.md:193; game-2/docs/PENGUIN3D_RETRO.md:43 | G2/G3 | thiếu |
| V20. Đo frame time/p95/hitch, tần số quét và cấu hình FPS trên máy thật, không kết luận chỉ từ FPS Editor. | lessons/bai-hoc-chung.md:181; game-2/docs/POLISH_PLAN.md:24; 3d/3d-map.md:415 | G2/chung | thiếu |
| V21. Đo A/B cùng cảnh, lặp và cho máy nguội; ghi rõ cảnh đứng yên hay đang có hoạt ảnh. | game-2/docs/PENGUIN3D_RETRO.md:51; game-2/docs/POLISH_PLAN.md:36 | G2 | thiếu |
| V22. Chỉ đọc điện thoại khi người làm game đang chơi; xin máy rảnh trước thao tác làm gián đoạn. | game-2/docs/WORKFLOW_RETRO.md:16; game-2/docs/POLISH_PLAN.md:75 | G2 | thiếu; phạm vi ủy quyền riêng xem C09 |
| V23. Báo riêng Editor/máy thật/chưa kiểm và lỗi runtime/material, không suy chất lượng nhìn từ hiệu năng. | CLAUDE.md:70; game-2/docs/PENGUIN3D_RETRO.md:37 | G2 | có phần chưa kiểm; thiếu ma trận bằng chứng |

### 1.6 codex-orchestration — brief, vòng lặp và Monitor
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| O01. Đầu phiên đọc mục lục, pillars/plan hiện hành, retro và kiến thức đúng việc rồi kiểm trạng thái bằng lệnh. | workflow/quy-tac-lam-viec.md:9; CLAUDE.md:23 | G1/G2/G3 | có tìm UI sẵn; thiếu toàn quy trình |
| O02. Research đủ mảng con của phase trước khi lập plan, đối chiếu từng dòng bảng phase để không sót save/audio. | lessons/bai-hoc-chung.md:26; lessons/bai-hoc-chung.md:27; game-2/docs/PHASE1_RETRO.md:23 | G1/G2 | thiếu |
| O03. Mỗi việc lớn có plan hiện hành và retro riêng, tách lịch sử khỏi nguồn trạng thái. | workflow/quy-tac-lam-viec.md:20; workflow/quy-tac-lam-viec.md:21; game-2/docs/LEVELGEN_RETRO.md:57 | G2 | thiếu |
| O04. Viết brief một mục tiêu, file cho phép/cấm, đọc trước, lệnh kiểm/kỳ vọng, trần và mẫu báo cáo. | workflow/quy-tac-lam-viec.md:30 | G2 | có (§2) |
| O05. Ghi AMBIG khi tài liệu im lặng/lệch code, gom câu cần quyết trước khi giao phần phụ thuộc. | workflow/quy-tac-lam-viec.md:22; game-2/docs/LEVELGEN_RETRO.md:81 | G2 | có AMBIG; thiếu đối chiếu luật |
| O06. Đưa API/công thức đã chạy thử vào brief cho khâu từng thất bại và xác nhận file nguồn/lệnh có thật. | game-2/docs/PENGUIN3D_RETRO.md:22; game-2/docs/LEVELGEN_RETRO.md:82; game-2/docs/LEVELGEN_RETRO.md:148; game-3/docs/colony_flow/STATUS.md:73 | G2/G3 | thiếu |
| O07. Với việc lớn, đồng thiết kế/rá chỉ đọc và kiểm lại từng phát hiện đúng/sai/chưa kiểm trước khi sửa plan. | workflow/quy-tac-lam-viec.md:23; game-2/docs/VIEWGATE_RETRO.md:18 | G2 | có kiểm độc lập kết quả; thiếu co-design |
| O08. Chọn nhánh đầy đủ hay nhẹ theo rủi ro; cổng đỏ dừng phần phụ thuộc và thử ca phá cổng khi cần. | workflow/quy-tac-lam-viec.md:32; workflow/quy-tac-lam-viec.md:40 | G2 | thiếu |
| O09. Đo trước khi giao: việc nhỏ rõ làm trong vòng ngắn thì tự sửa, việc lớn mới giao coder. | CLAUDE.md:37 | G2 | thiếu |
| O10. Phân quyền file và Unity cho job song song, tránh cả hai build/reload cùng project. | workflow/quy-tac-lam-viec.md:30; workflow/lam-viec-voi-codex.md:53 | G2 | có Unity; thiếu lịch file/CPU |
| O11. Dựng Monitor ngay khi giao, xác nhận PID/log sống và theo dõi tiến triển đúng lượt. | workflow/quy-tac-lam-viec.md:42; game-2/docs/VIEWGATE_RETRO.md:29 | G2/G3 | thiếu |
| O12. Canh/dừng đúng PID, tránh regex tự khớp chính Monitor hoặc giết mọi job cùng tên. | game-2/docs/BOARD3D_RETRO.md:9; game-2/docs/WORKFLOW_RETRO.md:10 | G2 | thiếu |
| O13. Trước khi kết thúc lượt chờ job, bảo đảm còn cơ chế đánh thức, kể cả sau compaction. | game-2/docs/POLISH_RETRO.md:19; game-2/docs/POLISH_RETRO.md:28 | G2 | thiếu |
| O14. Lưu brief/log/checkpoint bền và ghi từng mục ngay khi xong; resume phải mang phiên bản mã. | game-2/docs/LEVELGEN_RETRO.md:34; game-2/docs/LEVELGEN_RETRO.md:82; game-2/docs/POLISH_RETRO.md:23 | G2 | thiếu |
| O15. Một luồng việc một session, bàn giao bằng trạng thái kiểm được thay vì trí nhớ chat. | game-2/docs/WORKFLOW_RETRO.md:7 | G2 | thiếu |
| O16. Đo thời gian thật và tách trần/ước tính/đã đo, nêu mẫu sẵn, người đo và chi phí duyệt. | game-2/docs/LEVELGEN_RETRO.md:35; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:35 | G2 | có thời gian UI |
| O17. Lọc công cụ bằng miễn phí, máy/RAM/dung lượng/engine-version và đọc docs trước pilot một mẫu. | CLAUDE.md:43; lessons/bai-hoc-chung.md:48; lessons/bai-hoc-chung.md:51; lessons/bai-hoc-chung.md:52 | G1/G2 | thiếu |
| O18. Tìm theo nhiều cách giải bài toán, ghi nơi/từ khóa/chưa đọc, không kết luận hết công cụ từ một hướng tìm. | lessons/bai-hoc-chung.md:38 | G1/G2 | thiếu |
| O19. Kiểm nguồn cũ khi ràng buộc/model đổi, ghi nhãn độ tin; không dùng số tốc độ blog làm cam kết. | lessons/bai-hoc-chung.md:53; workflow/lam-viec-voi-codex.md:131 | G1/G2 | thiếu |
| O20. Tôn trọng phạm vi ủy quyền hiện tại; câu hỏi không là lệnh và đã hỏi không đồng nghĩa được duyệt. | lessons/bai-hoc-chung.md:201; game-2/docs/BOARD3D_RETRO.md:37; workflow/quy-tac-lam-viec.md:46 | G1/G2/G3 | thiếu; Git không tự động theo tài liệu cũ |

### 1.7 level/puzzle-design — luật, sinh màn, độ khó
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| L01. Chốt luật lõi thành bảng + ca nhỏ giữa docs/code/ý người làm game trước khi xây bộ sinh. | game-2/docs/LEVELGEN_RETRO.md:9; game-2/docs/LEVELGEN_RETRO.md:94 | G2 | thiếu |
| L02. Dựng greybox để quyết giữ/bỏ/sửa lõi trước art; không dùng khác biệt concept làm bằng chứng vui. | workflow/quy-trinh-phase.md:11; lessons/bai-hoc-chung.md:43 | G1/G2/G3 | thiếu |
| L03. Dạy một ý theo chuỗi giới thiệu → thử → kết hợp → biến tấu và xen nhịp căng/nghỉ. | learning/game/game-dev-map.md:312 | chung | thiếu |
| L04. Cho prototype có hướng dẫn/mục tiêu, preview, undo, vừa màn và lý do thắng/thua ngay từ brief. | lessons/bai-hoc-chung.md:192; game-3/CLAUDE.md:12 | G3 | thiếu |
| L05. Viết selftest config về khoảng mục tiêu, thứ tự ngưỡng và cơ chế hỗ trợ trước khi chạy hàng loạt. | game-2/docs/LEVELGEN_RETRO.md:78; lessons/bai-hoc-chung.md:184 | G2 | thiếu |
| L06. Đo phân bố chỉ số theo nhóm số màu/cơ chế trước khi thiết kế lịch độ khó. | game-2/docs/LEVELGEN_RETRO.md:43 | G2 | thiếu |
| L07. Dùng luật hợp lệ/giải được/cơ chế cần thiết làm cổng cứng; dùng điểm D xếp hạng gần mục tiêu. | lessons/bai-hoc-chung.md:185 | G2 | thiếu |
| L08. Giữ oracle độc lập, không cho solver tự sinh đáp án chuẩn của chính nó. | game-2/docs/LEVELGEN_RETRO.md:18; game-2/docs/LEVELGEN_RETRO.md:97 | G2 | thiếu |
| L09. Kiểm cả bản tham chiếu bằng differential trace và ca mới cho từng cơ chế. | game-2/docs/LEVELGEN_RETRO.md:19; game-2/docs/LEVELGEN_RETRO.md:116 | G2 | thiếu |
| L10. Đòi mỗi đặc trưng đo thay đổi trên tập mẫu, rà cột hằng và chỉ giữ một công thức/một đại lượng. | game-2/docs/LEVELGEN_RETRO.md:79 | G2 | thiếu |
| L11. Ghi LIMIT/truncated là chưa xác minh, không coi hết ngân sách tìm là vô nghiệm. | game-2/docs/LEVELGEN_RETRO.md:22; game-2/docs/LEVELGEN_RETRO.md:132 | G2 | thiếu |
| L12. Kiểm cơ chế bắt buộc bằng mọi lời giải trong giới hạn nước thật có dùng nó, không chỉ thay đổi số nước khi gỡ. | game-2/docs/LEVELGEN_RETRO.md:145; game-2/docs/LEVELGEN_RETRO.md:146 | G2 | thiếu |
| L13. Đo đa dạng bàn gốc và phủ cơ chế cùng số màn, không lấy số biến thể làm độ phong phú. | game-2/docs/LEVELGEN_RETRO.md:45; game-2/docs/LEVELGEN_RETRO.md:141 | G2 | thiếu |
| L14. Tái tạo pack từ config/seed/checkpoint đầy đủ, kiểm hai lượt hash và bảo vệ slot đã chốt khi sửa cục bộ. | game-2/docs/LEVELGEN_RETRO.md:122; game-2/docs/LEVELGEN_RETRO.md:137; game-2/docs/LEVELGEN_RETRO.md:140 | G2 | thiếu |
| L15. Hiệu chỉnh mô hình khó bằng lượt thử người thật; nếu chủ game chấp nhận bỏ playtest thì ghi rủi ro và tăng kiểm số độc lập. | game-2/docs/LEVELGEN_RETRO.md:80; lessons/bai-hoc-chung.md:187 | G2 | thiếu |
| L16. Đưa lời giải qua view bằng input thật và kiểm tổ hợp/thứ tự hoạt cảnh, không dừng ở solver đúng. | game-2/docs/VIEWGATE_RETRO.md:16; game-2/docs/VIEWGATE_RETRO.md:28 | G2 | thiếu |

### 1.8 motion/game-feel — phản hồi và hoạt cảnh
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| M01. Lập timeline input→hình/âm/rung, đo phản hồi đầu tiên; ~100 ms là hướng dẫn của tài liệu, không phải thời gian kết thúc cả hoạt cảnh. | learning/game/game-dev-map.md:189; learning/game/game-dev-map.md:191 | chung | thiếu |
| M02. Dùng một tầng phản hồi chung và kiểm từng lớp tween, âm, hạt, rung khi bật riêng và phối hợp. | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:200 | CF | có motion UI; thiếu phối hợp các lớp |
| M03. Phân bổ thời lượng theo cảm giác nặng/nhạy, thử frame giữ dài/ngắn trước khi tăng số frame. | learning/game/pixel-art-fundamentals.md:363 | chung | thiếu |
| M04. Kiểm viền rung, pixel chớp và mép bò trên chuỗi frame ở cỡ thật, giữ chuyển động theo pixel nguyên khi chọn pixel art. | learning/game/pixel-art-fundamentals.md:365 | chung | thiếu |
| M05. Dựng Idle trước và chốt ngân sách frame/hướng đồng đều trước khi sản xuất hàng loạt. | learning/game/pixel-art-fundamentals.md:366; learning/game/pixel-art-fundamentals.md:367 | chung | thiếu |
| M06. Ghi fps, thiết bị, tỉ lệ, đơn vị, số mẫu và sai số khi suy vận tốc/timing từ video; dùng nhiều frame vì video có frame lặp. | game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:3; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:5 | G3/CF | thiếu |
| M07. Kiểm đường đi, điểm xuất phát và pha lấy/mang vật bằng chuỗi ảnh; không suy đường đi chỉ từ tên Bezier trong stub. | game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:12; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:20 | G3/CF | thiếu |
| M08. Tách hằng cấu hình khỏi số đo: CF delay 0,30 s khác trung bình video 0,312 s nhưng nằm trong nhiễu ghi hình. | game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:23; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:39 | G3/CF | thiếu |
| M09. Giới hạn kết luận theo cảnh đã quan sát; đường mang về thẳng mới thấy ở góc dưới-trái, chưa chứng minh mọi ô. | game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:52 | G3/CF | thiếu |
| M10. Phân biệt phản hồi chạm tức thì với thời gian vật tới đích/sinh nhân vật; CF chạm→kiến đầu ~0,8 s không phủ định phản hồi nút nhanh. | game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:24; learning/game/game-dev-map.md:191 | G3/CF | thiếu |

### 1.9 audio — chọn, phối và kiểm âm
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| S01. Chốt audio bible theo trụ cột, ghi vai trò nhạc/SFX/UI và đối chiếu từng tiếng mới. | game-1/docs/AUDIO.md:3; game-1/docs/AUDIO.md:9 | G1 | thiếu |
| S02. Giữ motif xuyên các màn khi đó là chủ đích; thử nghe mù với bản đối chứng trước khi chốt phối. | game-1/docs/AUDIO.md:54; game-1/docs/AUDIO.md:65 | G1 | thiếu |
| S03. Đo LUFS, đỉnh cục bộ, phổ trầm, nối vòng và mức chuyển màn trên file xuất thực; ghi ngưỡng riêng dự án. | game-1/docs/AUDIO.md:84; game-1/docs/AUDIO.md:116 | G1 | thiếu |
| S04. Kiểm mix trên loa điện thoại lẫn tai nghe, bảo đảm không clipping. | game-1/docs/AUDIO.md:48 | G1 | thiếu |
| S05. Đo lớp gõ thưa tại các cú gõ và đỉnh, không chỉnh gain chỉ theo trung bình cả track. | game-1/docs/AUDIO.md:69 | G1 | thiếu |
| S06. Hiệu chỉnh bộ chấm AI bằng mẫu người nghe; chỉ dùng xếp hạng phụ nếu chưa khớp nhạc cụ/cảm xúc. | game-1/docs/AUDIO.md:63; game-1/docs/AUDIO.md:66; game-1/docs/AUDIO.md:110 | G1 | thiếu |
| S07. Pilot đúng precision/GPU và đo tốc độ thật trước khi chạy batch; SFT float16 từng tạo 16/16 NaN trên T4. | game-1/docs/AUDIO.md:60 | G1 | thiếu |
| S08. Kiểm đầu ra cover có giữ motif và tuân prompt bằng đối chứng, không mặc định tên chế độ bảo đảm thành công. | game-1/docs/AUDIO.md:67 | G1 | thiếu |
| S09. Ghi nguồn và license từng file; điều kiện thương mại của model/thư viện phải kiểm lại khi chọn, không coi ghi chú cũ là cấp phép hiện tại. | game-1/docs/AUDIO.md:71; game-1/docs/AUDIO.md:82 | G1 | thiếu |
| S10. Lập ánh xạ sự kiện→điểm gọi→clip→gain/mixer; clip nằm trong bank chưa chứng minh đang phát. | game-3/docs/colony_flow/AUDIO_FROM_APK.md:118; game-3/docs/colony_flow/AUDIO_FROM_APK.md:120; game-3/docs/colony_flow/AUDIO_FROM_APK.md:124 | G3/CF | thiếu |
| S11. Kiểm sự kiện khởi tạo có im lặng đúng thiết kế và âm được gắn đúng vật, tránh gán cube-break cho hộp hết đạn. | game-3/docs/colony_flow/AUDIO_FROM_APK.md:139; game-3/docs/colony_flow/AUDIO_FROM_APK.md:174 | G3/CF | thiếu |
| S12. Kiểm gain hiệu dụng qua cả source, clip và mixer; lưu rõ chỗ clone còn lệch và chưa quyết. | game-3/docs/colony_flow/AUDIO_FROM_APK.md:158; game-3/docs/colony_flow/AUDIO_FROM_APK.md:161; game-3/docs/colony_flow/AUDIO_FROM_APK.md:174 | G3/CF | thiếu |

### 1.10 meta/economy — tài nguyên, lịch và giao dịch
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| E01. Cho mọi cộng/trừ xu, tim, booster qua một cổng và kiểm UI tự cập nhật từ sự kiện thay đổi. | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:217 | CF | thiếu |
| E02. Đưa giá/phần thưởng vào config và xác định nguồn ghi đè trước khi dựng shop/meta. | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:219 | CF | thiếu |
| E03. Lưu thời điểm hồi tim và tính khi mở lại app, không phụ thuộc timer chỉ chạy lúc app sống. | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:220 | G3 | thiếu |
| E04. Phân biệt Hidden/Scheduled/Prefetching/Live/Ended; kiểm giờ đồng bộ, bundle và gói còn bán thay vì chỉ so countdown. | game-3/docs/colony_flow/SPEC_economy.md:110 | CF; điều kiện Live còn suy luận trong nguồn | thiếu |
| E05. Lưu giao dịch pending và kiểm giao bù khi app chết; giữ nhãn “thấy chữ ký” cho hành vi chưa đọc thân hàm. | game-3/docs/colony_flow/SPEC_economy.md:113 | CF | thiếu |
| E06. Kiểm khóa mua, giới hạn lượt mua, sold-out và giá store/fallback là các trạng thái riêng. | game-3/docs/colony_flow/SPEC_economy.md:113; game-3/docs/colony_flow/SPEC_economy.md:120 | CF | thiếu |
| E07. Kiểm claim theo tier mở, chưa nhận và quyền premium; kiểm phần thưởng tồn khi hết mùa bằng fixture thời gian. | game-3/docs/colony_flow/SPEC_economy.md:138; game-3/docs/colony_flow/SPEC_economy.md:139 | CF; tự trả cuối mùa còn suy luận | thiếu |
| E08. Tách thời điểm mua premium, quyền trong mùa và gỡ perk hết mùa; không dùng một boolean vĩnh viễn. | game-3/docs/colony_flow/SPEC_economy.md:131; game-3/docs/colony_flow/SPEC_economy.md:141 | CF | thiếu |
| E09. Quyết định lớp meta theo thể loại và mục tiêu, không bê team/league/pass chỉ vì mẫu có. | learning/game/game-fundamentals.md:331; workflow/quy-trinh-phase.md:45 | chung | thiếu |
| E10. Gắn chấm đỏ với dữ liệu nhận được/chưa đọc và kiểm trạng thái chưa vào đội, bảo trì, khóa. | game-3/docs/colony_flow/SPEC_social.md:97; game-3/docs/colony_flow/SPEC_social.md:115; game-3/docs/colony_flow/SPEC_economy.md:138 | CF | thiếu |
| E11. Khi tài liệu chỉ thấy API sự kiện, ghi bố cục chưa biết thay vì dựng thành kết luận đã quan sát. | game-3/docs/colony_flow/SPEC_events.md:79; game-3/docs/colony_flow/SPEC_events.md:212 | CF | thiếu |

### 1.11 architecture/data — ranh giới luật, dữ liệu và dịch vụ
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| C01. Giữ Rules thuần không phụ thuộc Unity/UI và kiểm được không engine; ghi rõ quan hệ assembly. | knowledge/colony-flow-research/06-de-xuat-kien-truc-game-sau.md:9 | G2/CF | thiếu |
| C02. Tách dữ liệu, hiển thị và điều phối; ghi sơ đồ phụ thuộc trước khi nhân bản module. | knowledge/colony-flow-research/06-de-xuat-kien-truc-game-sau.md:16 | đề xuất từ G2/CF | thiếu |
| C03. Mỗi mảng một lớp save, version/migrate và kiểm file cũ/hỏng; tách settings khỏi tiến trình. | knowledge/colony-flow-research/05-du-lieu-vong-doi-va-backend.md:15; learning/game/game-dev-map.md:336; learning/game/game-dev-map.md:337 | CF/chung | thiếu |
| C04. Đặt nội dung trong dữ liệu và chữ trong bảng khóa; thử chữ dài/font khác trước khi khóa bố cục. | learning/game/game-dev-map.md:335; learning/game/game-dev-map.md:338 | chung | thiếu |
| C05. Bọc từng dịch vụ ads/IAP/cloud/backend bằng adapter, quyết định cần hay bỏ sớm. | knowledge/colony-flow-research/06-de-xuat-kien-truc-game-sau.md:13; knowledge/colony-flow-research/00-tong-quan-cong-nghe.md:58 | CF | thiếu |
| C06. Đăng ký cập nhật UI theo dữ liệu và quản lý vòng đời subscription; [suy luận] tránh đồng bộ bằng nhiều giá trị sao chép. | knowledge/colony-flow-research/02-kien-truc-ma.md:62; knowledge/colony-flow-research/05-du-lieu-vong-doi-va-backend.md:16 | CF | thiếu |
| C07. Pool vật xuất hiện lặp lại và đo cùng mật độ mục tiêu; chỉ chọn instancing sau khi xác định số lượng thật. | knowledge/colony-flow-research/04-hien-thi-va-hieu-nang.md:19; knowledge/colony-flow-research/04-hien-thi-va-hieu-nang.md:37 | CF | thiếu |
| C08. Chuyển động theo speed×dt và kiểm tốc độ trên hai mức fps, không dịch một đoạn cố định mỗi frame. | learning/game/game-dev-map.md:77 | chung | thiếu |
| C09. Dùng state machine khi đủ phức tạp và liệt kê Enter/Update/Exit; mốc khoảng bốn trạng thái trong nguồn là đề xuất. | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:160; game-3/docs/colony_flow/SPEC_core.md:93 | CF | thiếu |
| C10. Phân biệt chữ ký stub, dữ liệu, thân mã và quan sát runtime; lưu phần chưa biết cạnh kết luận. | game-3/docs/colony_flow/SPEC_core.md:3; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:47 | G3/CF | thiếu |

### 1.12 release/marketing — phát hành và kiểm giả thuyết thị trường
| ID / hành động kiểm được | Nguồn | Game | Skill hiện tại |
|---|---|---|---|
| R01. Chỉ sang phase sau khi câu hỏi phase hiện tại có câu trả lời và rủi ro chuyển tiếp được ghi. | workflow/quy-trinh-phase.md:7; workflow/quy-trinh-phase.md:34; workflow/quy-trinh-phase.md:37 | G1/chung | thiếu |
| R02. Dùng hình vertical slice để thử hook cuối phase 1/đầu phase 2 trước khi sản xuất nhiều nội dung. | workflow/quy-trinh-phase.md:20 | mobile hybrid-casual | thiếu |
| R03. Đưa analytics, retention, accessibility, store, ASO, ads/IAP và localization vào danh mục research phase 3–5. | workflow/quy-trinh-phase.md:46 | chung | thiếu |
| R04. Kiểm build theo ràng buộc nền tảng và quality tier, thay vì chỉ xác nhận chạy được Editor. | learning/game/game-dev-map.md:412; learning/game/game-dev-map.md:413 | chung | thiếu |
| R05. Chuẩn bị icon/capsule, ảnh, trailer và mô tả ngắn rồi kiểm người lạ hiểu game; nguồn hiện mới nêu nhu cầu, chưa có quy trình ASO đã thử. | learning/game/game-dev-map.md:414; game-1/docs/TOOLING.md:61 | chung/G1 | thiếu |
| R06. Kiểm lại yêu cầu store tại thời điểm phát hành; số 12 người/14 ngày trong tài liệu là ghi chú đã tra 2026-10-01, không phải xác nhận mới. | workflow/quy-trinh-phase.md:15; workflow/quy-trinh-phase.md:18 | G1/Android | thiếu |
| R07. Ghi lỗ hổng ký app/AAB/store/ads/analytics thành việc research có đầu ra trước khi viết skill thao tác phát hành. | game-1/docs/TOOLING.md:15; game-1/docs/TOOLING.md:60; workflow/quy-trinh-phase.md:60 | G1 | thiếu |
| R08. Theo dõi bản vá, cộng đồng, analytics và sự kiện sau launch; không coi đưa app lên store là hết vòng đời. | learning/game/game-dev-map.md:416 | chung | thiếu |

## 2. Mâu thuẫn, lỗi lặp và bằng chứng bỏ qua tài liệu
### 2.1 Mâu thuẫn phải giữ ngữ cảnh
| Vấn đề | Hai nguồn / bằng chứng | Cách xử lý khi viết skill [suy luận] |
|---|---|---|
| Mipmap | knowledge/colony-flow-research/11-sprite-png-atlas.md:33 nói tắt; game-2/docs/UI_MATTE_RETRO.md:16 ghi bật có lợi khi thu nhỏ. | Tách pixel/native-size khỏi UI thu nhỏ; thử trên máy thật, không áp blanket preset. |
| Mesh + texture chiếu hay tô từng phần | game-2/docs/ART.md:93; lessons/bai-hoc-chung.md:231; knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:31. | Hai hướng cho hai look; toon nhiều vật cần thử tách phần/material. Chưa có mẫu penguin toon đã duyệt thì không tuyên bố hướng mới thắng. |
| Đếm lỗi CS | CLAUDE.md:56 đếm log; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:24 dùng mốc lỗi mới/trạng thái compiler. | Tách lỗi cũ khỏi lượt hiện tại; vẫn phải loại trường hợp test trên assembly cũ. |
| Nơi sửa kit | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:39 chỉ game-2; knowledge/game-kit/GAME_KIT_PLAN.md:133 chỉ game-kit là nguồn chính. | Đánh dấu recipe cũ; tách workflow chung khỏi cách dùng package. Kit đang dừng nên chưa sửa gì. |
| Thành phần gallery ≠ API kit | knowledge/game-kit/GAME_KIT_PLAN.md:134; knowledge/game-kit/GAME_KIT_PLAN.md:135. | Không ghi toggle/slider/progress đã có trong kit chỉ vì thấy gallery/test xanh. |
| Cổng xanh ≠ đẹp | knowledge/game-kit/GAME_KIT_PLAN.md:127; knowledge/game-kit/GAME_KIT_PLAN.md:129; game-2/docs/HOME_RETRO.md:21. | Validator, ảnh và người duyệt là ba bằng chứng riêng; bổ sung contrast và hình dạng. |
| Toon không đồng nghĩa bật mọi hậu kỳ | game-2/docs/PENGUIN3D_RETRO.md:38; knowledge/colony-flow-research/08-3d-do-tu-du-lieu-goc.md:18. | Rà đủ lớp, nhưng chỉ bật hiệu ứng có ích cho ảnh đích và ngân sách. |
| Tham số toon tổng quát ≠ material thật | knowledge/colony-flow-research/08-3d-do-tu-du-lieu-goc.md:23; game-3/docs/colony_flow/LOOK_findings.md:25; game-3/docs/colony_flow/ANT_MEASUREMENTS_2026-11-05.md:119. | Ghi per-material, linear/sRGB và runtime; không chép một bộ ramp/rim cho mọi vật. |
| Độ to mục tiêu ≠ lịch sử mix | game-1/docs/AUDIO.md:91 mục tiêu −16 LUFS; game-1/docs/AUDIO.md:116 cập nhật −25 LUFS đo. | Giữ ngày/trạng thái; kiểm cả chuyển màn và loa máy, không ban hành LUFS cố định cho mọi game. |
| Có random-pitch/cooldown ≠ đang dùng | game-1/docs/AUDIO.md:47 gợi ý biến pitch; game-3/docs/colony_flow/AUDIO_FROM_APK.md:118; game-3/docs/colony_flow/AUDIO_FROM_APK.md:119 ghi CF không gọi. | Đây là lựa chọn thiết kế khác nhau; tra điểm gọi trước khi mô tả hành vi game mẫu. |
| Stub suy luận ≠ đường đi quan sát | game-3/docs/colony_flow/SPEC_core.md:32; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:12; game-3/docs/colony_flow/ANT_MOTION_MEASURED.md:20. | Giữ thứ bậc chứng cứ và phạm vi mẫu; đường về thẳng chưa chứng minh toàn bộ bảng. |
| AntLab đã tìm thấy nhưng mục cuối chưa sửa | game-3/docs/colony_flow/SPEC_events.md:79 xác nhận modal; game-3/docs/colony_flow/SPEC_events.md:274 còn ghi không thấy. | Dùng phát hiện cụ thể mới hơn; bố cục vẫn chưa biết. |
| “Mọi bước điện thoại” và vòng ảnh trung gian | CLAUDE.md:69; game-2/docs/PENGUIN3D_RETRO.md:42. | Tách vòng look Editor khỏi nghiệm thu máy thật cuối bước; không tự chiếm điện thoại đang dùng. |
| Benchmark nhanh có mẫu sẵn | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:35; knowledge/game-kit/skill/game-ui-workflow/SKILL.md:36; knowledge/game-kit/GAME_KIT_PLAN.md:129. | Không dùng 85–98 s như tốc độ thiết kế UI mới hoặc bằng chứng kit nhanh hơn quy trình cũ. |
| Touch px ≠ dp; palette là guideline | game-1/docs/UI.md:32; learning/ui-ux/UI_UX_FUNDAMENTALS.md:192; learning/game/pixel-art-fundamentals.md:291. | Giữ đơn vị, scale và phong cách; không ép một ngưỡng màu/chạm cho mọi game. |
| Quy trình git/quyền chạy công cụ cũ | workflow/quy-trinh-phase.md:37; game-2/docs/POLISH_RETRO.md:14; chỉ dẫn người dùng lượt hiện tại. | Quyền hiện tại thắng recipe cũ: lượt này không git/Unity, không hạ sandbox theo retro. |

### 2.2 Cùng lỗi đã xảy ra ít nhất hai lần
Các cặp dưới là hai sự cố riêng trong retro, không đếm bản tổng hợp lessons như một sự cố mới.
| Lỗi lặp | Sự cố A | Sự cố B | Cổng ngăn lặp [suy luận] |
|---|---|---|---|
| Không đọc nền tảng/look theo lớp | game-2/docs/PENGUIN3D_RETRO.md:38 | game-2/docs/BOARD3D_RETRO.md:20 | Brief nêu tài liệu đã đọc + lớp còn thiếu trước khi mở rộng. |
| Làm lại asset/component sẵn có | game-2/docs/HOME_RETRO.md:16 | game-2/docs/LOSE_MODAL_RETRO.md:5 | Liệt kê tên asset/prefab đã duyệt và lý do không dùng được. |
| Cắt ảnh còn haze/fringe | game-2/docs/HOME_RETRO.md:14 | game-2/docs/SPRITE_RETRO.md:25 | So alpha trên nền sáng/tối và ảnh 100%, kiểm mọi tờ. |
| Brief gom nhiều việc | game-2/docs/PENGUIN3D_RETRO.md:28 | game-2/docs/BOARD3D_RETRO.md:50 | Một đầu ra, phạm vi và tiêu chí mỗi job. |
| Job nền không có cơ chế quay lại | game-2/docs/POLISH_RETRO.md:19 | game-2/docs/POLISH_RETRO.md:28 | Ghi owner/job/trạng thái và dựng Monitor ngay khi giao. |
| Shader mất trên build | game-2/docs/SPRITE_RETRO.md:19 | game-2/docs/PENGUIN3D_RETRO.md:36 | So Editor/build và kiểm shader inclusion trên thiết bị. |
| Thay art đã khóa khi chưa được phép | game-2/docs/BOARD3D_RETRO.md:37 | game-2/docs/BOARD3D_RETRO.md:49 | Brief ghi protected assets; chưa có đồng ý thì giữ nguyên. |
| Tự chấm số rồi bỏ hình tổng thể | game-1/docs/PHASE1_RETRO.md:53 | game-2/docs/HOME_RETRO.md:21 | Đối chiếu cả khung ở kích thước dùng thật và xin duyệt gu. |
| Nhầm trạng thái scene với đối tượng thật | game-2/docs/GAMEPLAY_UI_RETRO.md:20 | game-2/docs/POLISH_RETRO.md:27 | Fixture dựng sạch, kiểm root/component thật; chạy nhóm test. |
| Tìm lại công cụ đã có | game-2/docs/SPRITE_RETRO.md:12 | game-2/docs/POLISH_RETRO.md:24 | Kiểm docs/tool/cache trước khi nghiên cứu hoặc xóa công cụ. |

### 2.3 Bằng chứng Claude bỏ qua tài liệu đã có
- game-2/docs/BOARD3D_RETRO.md:36 ghi rõ ART.md + bài #46 đã cấm cách dựng sàn/vành bằng script nhưng vẫn làm; đây là vi phạm đã ghi, không phải suy đoán động cơ.
- game-2/docs/BOARD3D_RETRO.md:20 ghi không đọc learning đầu phiên và lặp retro #29/#31; game-2/docs/POLISH_RETRO.md:7 ghi bỏ qua kiến thức màu/ánh sáng đã có.
- game-2/docs/HOME_RETRO.md:41 ghi bỏ qua chỉ dẫn UNITY_LIVE; game-2/docs/POLISH_RETRO.md:24 ghi nghiên cứu lại SAM/BiRefNet đã so trước đó và xóa tool vì hiểu sai phạm vi.
- game-2/docs/LEVELGEN_RETRO.md:79 ghi đã có bài review độc lập nhưng vẫn tự đo/tự kết luận qua nhiều lượt.
- [suy luận] Thiếu cơ chế buộc truy xuất/check tài liệu theo trigger có vẻ là khoảng trống lớn hơn thiếu thêm lời khuyên; skill nên yêu cầu một bằng chứng áp dụng cụ thể, không chỉ “đã đọc”.

## 3. Bộ skill đề xuất [suy luận]
Mọi tên, thứ tự và phụ thuộc dưới đây là đề xuất; chưa tạo skill, chưa khẳng định đã kiểm chất lượng đầu ra. “Viết ngay” nghĩa đủ nguồn để viết quy trình, không có nghĩa mọi thẩm mỹ/SDK đã được nghiệm thu.
Phụ thuộc dùng tên ngắn trong bảng; chỉ tải skill phụ khi công việc cần. Mỗi SKILL.md ngắn gồm trigger, đầu vào, các cổng, đầu ra và điểm dừng; công thức, bảng lỗi và recipe theo engine/game nằm trong reference riêng có file:dòng/ngày kiểm.
| Ưu tiên / skill | Một việc / lúc kích hoạt | Nguồn chính | Phụ thuộc | Sẵn sàng / còn thiếu |
|---|---|---|---|---|
| P0 session-context | Chốt bộ tài liệu và quyền áp dụng khi bắt đầu/khôi phục phiên. | workflow/quy-tac-lam-viec.md:9; CLAUDE.md:23; O01/O19/O20 | — | Viết ngay; reference map theo miền và quyền hiện tại. |
| P0 codex-brief | Biến một đầu ra thành brief có contract, AMBIG và cổng; dùng trước giao việc. | workflow/lam-viec-voi-codex.md:30; L01/O04/O05 | session-context | Viết ngay; reference mẫu UI/asset/solver, không gộp recipe Unity. |
| P0 job-monitor | Theo dõi và tiếp tục một job nền, kể cả sau compact/reboot. | game-2/docs/POLISH_RETRO.md:19; game-2/docs/POLISH_RETRO.md:28; O09–O15 | codex-brief | Viết ngay; cần kiểm lại tên tool/lệnh trên môi trường dùng thật. |
| P0 unity-verification | Tạo bằng chứng code/test/input đúng trên đúng bản biên dịch. | knowledge/game-kit/skill/game-ui-workflow/SKILL.md:23; V01–V23 | session-context | Viết ngay quy trình; tách reference live/batch/phone và quyền thiết bị. |
| P0 visual-acceptance | Nghiệm thu một ảnh/cảnh so ảnh đích bằng mắt và phép đo độc lập. | game-2/docs/BOARD3D_RETRO.md:20; U12/V14/V15 | unity-verification | Viết ngay cổng; mỗi dự án cần MẪU ĐÃ CHỐT và tiêu chí riêng. |
| P0 approved-asset-reuse | Tìm và chọn đúng asset/component đã duyệt trước khi tạo mới. | game-2/docs/LOSE_MODAL_RETRO.md:5; game-2/docs/POLISH_RETRO.md:16 | session-context | Viết ngay; reference trạng thái approved/rejected/protected. |
| P1 art-direction | Chốt một ảnh đích trong khung kỹ thuật trước khi sản xuất art. | workflow/quy-trinh-phase.md:29; D01–D12 | approved-asset-reuse, visual-acceptance | Viết ngay quy trình; look mới cần MẪU ĐÃ CHỐT toàn cảnh ở cỡ game. |
| P1 ui-layout | Dựng một màn từ hierarchy, component, trạng thái và safe-area đã chọn. | U01–U20; game-3/docs/colony_flow/CODE_HOME_BATTLE_FROM_APK.md:249 | art-direction, approved-asset-reuse | Đủ quy trình; để cam kết “đẹp” cần MẪU ĐÃ CHỐT panel/nút/modal/HUD, font/material và 16:9/20:9. |
| P1 ui-text-accessibility | Kiểm chữ, tương phản, glyph, vùng chạm và tín hiệu ngoài màu. | U09–U12; learning/game/pixel-art-fundamentals.md:371 | ui-layout, visual-acceptance | Viết ngay kiểm cơ bản; thiếu kiểm người dùng accessibility và bộ dịch thực. |
| P1 sprite-cutout | Tách một lớp ảnh mà giữ alpha, identity và mép đúng. | A04–A09; game-2/docs/UI_MATTE_RETRO.md:14 | approved-asset-reuse, visual-acceptance | Viết ngay; reference matte/flood-fill/segmentation, pilot đủ kiểu nền. |
| P1 sprite-import-atlas | Chuyển sprite sạch sang import/atlas/9-slice đúng scale. | A14–A20; knowledge/colony-flow-research/11-sprite-png-atlas.md:32 | sprite-cutout, unity-verification | Viết ngay recipe theo engine; mẫu scale/phone để chọn mipmap. |
| P1 puzzle-rule-contract | Đóng đặc tả luật và oracle trước viết generator. | L01/L08–L12; game-2/docs/LEVELGEN_RETRO.md:94 | codex-brief | Viết ngay; reference counterexample/ambiguity theo cơ chế. |
| P1 puzzle-generation-check | Kiểm pack về lời giải, độ khó, đa dạng và tái tạo. | L05–L15; game-2/docs/LEVELGEN_RETRO.md:137 | puzzle-rule-contract | Viết ngay quy trình; khó/vui cần dữ liệu playtest, không chỉ solver. |
| P1 research-decision | So các lựa chọn công cụ trên một mẫu thật rồi chốt. | workflow/quy-trinh-phase.md:66; O17/O18 | session-context | Viết ngay; giá/license/tương thích phải kiểm lại lúc chọn. |
| P2 toon-lookdev | Hiệu chỉnh một cảnh toon qua geometry/material/light/render. | T01–T20; game-3/docs/colony_flow/ANT_MEASUREMENTS_2026-11-05.md:113 | art-direction, visual-acceptance | Viết ngay checklist; thiếu MẪU ĐÃ CHỐT penguin toon và shader thay thế đã thử trên A07. |
| P2 ai-mesh-preparation | Làm sạch một mesh AI cho import, UV/bake và ngân sách. | T07–T10; game-2/docs/ART.md:112 | research-decision, unity-verification | Viết ngay nhánh đã ghi; phải pilot theo silhouette/texture đã chọn, không ép toon dùng projection. |
| P2 sprite-animation | Tạo và kiểm một chuỗi frame nhất quán theo state/direction. | M03–M05; learning/game/pixel-art-fundamentals.md:376 | art-direction, sprite-import-atlas | Đủ quy trình kiểm; style mới cần MẪU ĐÃ CHỐT identity, idle, một hành động và số hướng. |
| P2 game-feedback | Thiết kế/đo timeline phản hồi cho một hành động. | M01–M10; knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:200 | unity-verification, visual-acceptance | Viết ngay checklist; thiếu thư viện preset được duyệt trên máy thật cho từng feel. |
| P2 audio-qa | Lọc, nghe mù và kiểm mix một nhóm âm trong game. | S01–S12; game-1/docs/AUDIO.md:84 | research-decision, unity-verification | Viết ngay quy trình; theme mới cần MẪU ĐÃ CHỐT bằng nghe, không dựa điểm AI. |
| P2 economy-rules | Đóng contract tài nguyên/claim/thời gian và ca biên. | E01–E11; game-3/docs/colony_flow/SPEC_economy.md:113 | puzzle-rule-contract | Viết ngay luật/fixture; thiếu dữ liệu balance/retention cho việc chọn giá và phần thưởng. |
| P2 save-migration | Kiểm một phiên bản save mới với bản cũ/hỏng/khôi phục. | C03/C04; learning/game/game-dev-map.md:336 | session-context | Viết ngay khung; fixture theo schema thật của từng game. |
| P2 service-adapter | Tách một dịch vụ khỏi game và kiểm nhánh lỗi/offline. | C05; knowledge/colony-flow-research/06-de-xuat-kien-truc-game-sau.md:13 | save-migration, research-decision | Đủ thiết kế adapter; thiếu research SDK/provider hiện tại và integration mẫu để viết thao tác chi tiết. |
| P3 release-readiness | Tạo bằng chứng sẵn sàng cho một đợt phát hành. | R01/R03/R04/R06/R07 | unity-verification, save-migration | Thiếu research/mẫu: ký/AAB, policy store hiện hành, SDK/khai báo dữ liệu, pilot release và rollback. Chỉ viết checklist khoảng trống lúc này. |
| P3 marketing-creative | Thử một hook với creative từ vertical slice. | R02/R05; workflow/quy-trinh-phase.md:20 | art-direction | Thiếu research/mẫu: creative đã thử, đối tượng, cách đo và kết quả; chưa đủ để cam kết ASO/UA hiệu quả. |
| Tạm dừng unity-ui-kit-usage | Hướng dẫn dùng đúng API/version kit khi người dùng cho tiếp tục kit. | knowledge/game-kit/GAME_KIT_PLAN.md:126; knowledge/game-kit/GAME_KIT_PLAN.md:132 | ui-layout, unity-verification | Thiếu MẪU ĐÃ CHỐT: K9 còn chờ duyệt look; bootstrap phone chưa kiểm; recipe nguồn cũ cần đối chiếu. Không mở lại kit trong S1. |
Thứ tự [suy luận]: P0 chặn lỗi báo sai/lặp việc; P1 tạo lát nhỏ và tài nguyên tái dùng; P2 mở rộng chuyên môn khi có ảnh đích; P3 nghiên cứu trước khi tự động hóa việc có hậu quả ngoài dự án. Không có phép đo ROI nên không gán điểm hay thời gian tiết kiệm.
Các khoảng trống xuyên miền: chưa có chuẩn đẹp được người làm game duyệt cho UI kit mới; thiếu mẫu toon penguin hoàn chỉnh; thiếu playtest/thiết bị đa dạng; thiếu kiểm đóng gói/phát hành thật. Ghi chú CF là nghiên cứu một game/version, không thay nghiên cứu phong cách và quyền sử dụng tài sản game mới.

## 4. Kiểm chéo ngẫu nhiên 10 bài học
Cách lấy: Python `random.Random(20261107).sample(rows, 10)` trên toàn bộ 182 hàng bài học, giữ thứ tự bảng; seed công khai để tái lập. Đã mở lại nguồn, không chỉ kiểm file tồn tại. Trích dưới đây giữ nguyên chữ nguồn, không phải câu hành động đã diễn giải.
| ID | Nguồn mở lại / trích nguyên văn ngắn | Kết quả |
|---|---|---|
| S04 | game-1/docs/AUDIO.md:48 — “Mix nhỏ hơn 0 dBFS; kiểm trên **loa điện thoại** A07, không chỉ tai nghe.” | Đúng. |
| C06 | knowledge/colony-flow-research/02-kien-truc-ma.md:62 — “Reactive (`Reactive<T>`, `Subscribe`, `AddTo`)”; có ví dụ SlotDaily cập nhật UI. | Đúng cơ chế đăng ký/vòng đời; tránh sao chép là cách áp dụng [suy luận]. |
| O03 | workflow/quy-tac-lam-viec.md:20 — “nguồn duy nhất về trạng thái; chỉ chứa hiện hành”; dòng 21 yêu cầu retro riêng. | Đúng. |
| O19 | lessons/bai-hoc-chung.md:53 — “Khi ràng buộc đổi”; workflow/lam-viec-voi-codex.md:131 — “là tuyên bố của blog, không đo”. | Đúng; đã kiểm cả hai nguồn. |
| U09 | game-1/docs/PHASE1_RETRO.md:52 — “2 cỡ × 2 ngôn ngữ”; lessons/bai-hoc-chung.md:124 — “mọi ngôn ngữ”. | Đúng; tổng hợp mới mở rộng đúng ngôn ngữ hỗ trợ. |
| A07 | game-2/docs/UI_MATTE_RETRO.md:14 — “Xem từng icon trên nền tối phóng to sau mỗi lần cắt; khử viền ngay trong bước cắt”. | Đúng. |
| E03 | knowledge/colony-flow-research/12-huong-dan-lam-game-nhu-colony-flow.md:220 — “lưu thời điểm, tính khi mở app (`LifeRegen`)”. | Đúng; nguồn ghi rõ clone. |
| V08 | game-2/docs/WORKFLOW_RETRO.md:21 — “Mọi test dùng thư mục lưu tạm phải thay dịch vụ mạng bằng bản giả (`OfflineLeaderboard`)”. | Bản đầu thêm chờ/dọn async chưa được nguồn quy định; đã sửa về bản giả + đọc stack trace. |
| T11 | game-2/docs/ART.md:93 — “CHIẾU concept đã duyệt lên mesh”; dòng 94 — “chống sáng gấp đôi”; knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:37 — “chưa chạy thử dựng cánh cụt theo cách này”. | Đúng; giữ điều kiện, không suy là toon đã thắng. |
| T04 | knowledge/colony-flow-research/10-mesh-3d-clone-va-bai-hoc.md:31 — “mỗi bộ phận là một material riêng”; dòng 35 giải thích thứ tự chia bộ phận. | Đúng; chưa thử penguin là giới hạn có ghi. |
Kết quả lần đầu: 9/10 đầy đủ căn cứ; V08 có phần thêm quá nguồn. Sau sửa và kiểm lại: **10/10**. Không suy tỷ lệ đúng toàn bộ kho từ mẫu mười hàng. Đã kiểm riêng sự tồn tại và số dòng của mọi đường dẫn trích dẫn; kiểm đường dẫn không thay kiểm nội dung.

## 5. Phạm vi nguồn và giới hạn
- Đã đọc CLAUDE.md; cả ba file workflow/quy-tac-lam-viec.md, workflow/quy-trinh-phase.md, workflow/lam-viec-voi-codex.md; toàn bộ lessons/bai-hoc-chung.md.
- knowledge/colony-flow-research thực tế có **15** file Markdown (00–13 + README), khác con số 13 của brief; đã đọc cả 15. Giữ nhãn [APK]/[đo]/đề xuất/chưa kiểm của nguồn khi tổng hợp.
- Đã đọc skill hiện tại toàn bộ 72 dòng; GAME_KIT_PLAN.md gồm mục 9–10; knowledge/08-mau-va-anh-sang.md; learning/README.md; learning/ui-ux/UI_UX_FUNDAMENTALS.md; cả ba file learning/game/game-fundamentals.md, game-dev-map.md, pixel-art-fundamentals.md; 3d/3d-map.md và 3d/3d-fundamentals.md.
- Đường dẫn brief nêu `knowledge/lam-viec-voi-codex.md` không tồn tại; đã đọc toàn bộ tài liệu cùng tên tại `workflow/lam-viec-voi-codex.md`, không tự tạo đường dẫn thay thế.
- G2: đã đọc BOARD3D_RETRO, GAMEPLAY_UI_RETRO, HOME_RETRO, LEVELGEN_RETRO, LOSE_MODAL_RETRO, PENGUIN2D_RETRO, PENGUIN3D_RETRO, PHASE1_RETRO, POLISH_RETRO, SPRITE_RETRO, UI_MATTE_RETRO, VIEWGATE_RETRO, WORKFLOW_RETRO trong game-2/docs; cùng ART, ASSET_PIPELINE, KIT_SPEC, TOOLING, META_UI_PLAN, POLISH_PLAN (đuôi .md). RETROS.md cũng được xem để đối chiếu mục lục.
- G1: đã đọc toàn bộ game-1/docs/ART.md, AUDIO.md, UI.md, TOOLING.md, PHASE1_RETRO.md.
- G3: đã đọc CLAUDE.md, docs/PILLARS.md, CLONE_PLAN.md, SPEC.md và nhóm ghi chú colony_flow: LOOK_findings, UX_REFERENCE, HOME_PARITY, IL2CPP, ANT_PORT, BATTLE_PARITY, VISUAL_plugins, VISUAL_assets, SPINE_ANIMATIONS_FROM_APK, VFX, BATTLE_NOTES, CORE_NOTES, ANT_MOTION_MEASURED, ANT_MEASUREMENTS_2026-11-05, CODE_HOME_BATTLE_FROM_APK, CODE_POPUP_SETTINGS, STATUS, SPEC_ui, DATA_findings, AUDIO_FROM_APK, APK_COVERAGE, APK_DEEP, LEVEL_FORMAT (tên rút gọn trong danh mục này; các chứng cứ chính dùng đường dẫn đầy đủ trong bảng).
- SPEC_economy được đọc; SPEC_core/LEVEL_MECHANICS được đối chiếu thêm theo phần liên quan luật, chuyển động, dữ liệu và hình; SPEC_events/SPEC_social được rà các phần UI, popup và trạng thái. Không tuyên bố đã xác minh backend hay đọc thân mã APK từ các chữ ký stub.
- Một số lessons nhắc HANDOFF cũ của G3 nhưng không có bản nguồn đó trong phạm vi docs tìm được; chỉ dùng nội dung lessons đã đọc, không nhận đã mở lại HANDOFF.
- Đây là kiểm kê tài liệu, không phải thử Unity/SDK, đo hiệu năng mới, kiểm giấy phép hiện tại hoặc kiểm chứng toàn bộ tuyên bố nghiên cứu gốc. Không đọc reference/apk, không truy cập nhị phân, không chạy git/gh/Unity, không đọc .env; chỉ ghi hai file đầu ra được phép.
- Không còn miền bắt buộc chưa tổng hợp. Những phần thiếu research/MẪU ĐÃ CHỐT nằm trong bảng skill, chưa được lấp bằng suy đoán hoặc bằng sửa kit đang tạm dừng.

## 6. Bổ sung 2026-11-11: game-3 (Colony Flow) và game-5 (Hole it!)
Nguồn: `game-3/docs/COLONY_FLOW_RETRO.md` (C1-C8), `game-5/docs/HOLEIT_CLONE_RETRO.md` (R0-R9), memory `~/.claude/projects/-Users-tho-project-indie-game-game-3/memory/` (7 mục) và `-Users-tho-project-indie-game/memory/` (2 mục). Cả hai game là clone 1:1 từ APK để học; chỉ phần không phụ thuộc APK mới vào skill. Retro của hai game chưa commit ở repo của chúng (đọc ngày 2026-11-11).

### 6.1 Lỗi lặp qua cả hai game (đáng thành skill)
| Lỗi | game-3 | game-5 | Skill |
|---|---|---|---|
| Báo hình ảnh "xong" khi chưa tự ghép ảnh so với ảnh chuẩn; test xanh nhưng lỗi lọt (bi trắng xám, Home chồng cảnh, bảng thua ghi "Level 10" chữ mẫu) | C7 | R2, R9 | 2 |
| Nói "không có gì đang chạy" / chuyển tiếp báo cáo "đang chờ" của agent | C4 | R5, R6, R8 | 1 |
| Số tự đặt hoặc đoán theo mắt, không nguồn | C1 | R1, R4 | 1, 2 |
| Trả lời "chưa biết" / hỏi người làm game khi dữ liệu đã có (R1: tìm theo TÊN trong khi scene tham chiếu bằng GUID, kết luận "không có số liệu") | C6 | R1 | 1 |
| Không ghi retro lúc gặp lỗi, phải dựng lại sau | đầu file | R0 | 1 (+ hook) |
| Chạy quét hàng loạt tốn giờ, số vô nghĩa (574 level, 140/574 sau 1,5 giờ, hộp thời gian 4 s) | C5 | cùng lỗi | 1 |
| Dừng hỏi từng chút khi đã được giao chạy liên tục (người làm game nhắc 4 lần) | C8 | — | 1 |
| Chia nhiều agent, mỗi agent tự chạy Unity trên một cây code (mất lượt PlayMode 32 phút, một agent chờ ~50 phút) | — | R5, R8 | 1 |

### 6.2 Chi tiết vận hành chưa có ở mục 1.6 (từ memory game-3)
- `ListAgents` có thể báo "completed" trong khi `TaskStop` liệt kê agent vẫn chạy: kiểm cả hai nguồn và `ps` (`monitor-delegated-agents`).
- Lệnh dài phải có `timeout` rõ ràng, nếu không bị đẩy nền và không có thông báo kết thúc.
- Batch Unity tự chạy phải gọi `EditorApplication.Exit`, nếu không thư mục khóa chặn lần sau; kiểm Unity bằng `ps -ax -o pid,etime,command | grep "MacOS/Unity"` (mẫu `[M]acOS/Unity -` từng bắt hụt); không tự xóa khóa.
- Codex so ảnh tự mâu thuẫn: coi là gợi ý, số đo quyết định (`colony-flow-identical-clone-goal`).
- Khi xem kích thước qua symlink dùng `stat -L`.
- Ước lượng thời gian sai: PlayMode nói ~35 phút, đo thật ~3 phút (R8). Phù hợp quy tắc "mọi số chi phí / thời gian phải từ một lần chạy đã đo".

### 6.3 Số đo tham chiếu (ghi đơn vị và game nguồn)
- game-5, Galaxy A07, bot level 1, 2026-11-10 10:57: FPS gameplay TB 49,6 (mục tiêu 58, TRƯỢT), p95 16,7 ms (đạt), RAM PSS đỉnh 786 MB (mục tiêu 600, TRƯỢT), APK debug 138 MiB. Nguồn: `game-5/docs/OVERNIGHT_PLAN.md` (nhật ký).
- game-5, Editor (không phải máy thật): khung CPU 3,35 ms, 259 renderer, ~58 MB asset trùng byte (`game-5/docs/spec/PERF_EDITOR.md`).
- game-3: kiểm vòng tròn bố cục 0 sai lệch trên khoảng 35 thuộc tính, thử làm hỏng cố ý bắt được 273 lỗi (`game-3/docs/colony_flow/STATUS.md` dòng 20).

### 6.4 Không đưa vào skill (chỉ đúng khi có APK)
Trích APK, Il2Cpp, typetree, GLSL từ bundle, bảng `docs/spec/*` toàn số đo từ gốc, quy tắc "mọi số đọc từ APK". Giữ lại dạng tổng quát: mỗi số gắn nhãn nguồn (đo / quyết định đã duyệt / TỰ ĐẶT); tìm trong dữ liệu đã có (cả theo tên lẫn GUID) trước khi nói "chưa biết".

### 6.5 Mâu thuẫn ghi nhận
- Look mục tiêu "kiểu game-3" (PLAN mục 1.3) nhưng game-3 trên máy dùng sprite gốc Colony từ APK (C2), còn quyết định mục 1.2 cấm dùng sprite game thương mại. Xử lý: PLAN mục 1, điểm 7.
- Memory game-3 chứa bài học của game-5 (`one-unity-run-after-agents-split`, `compare-before-claiming-visual`): memory theo thư mục làm việc nên bài học đi sai chỗ. Xử lý: PLAN mục 1, điểm 9.
- Chưa kiểm: `game-3/docs/colony_flow/STATUS.md` mục 6 còn bài nào ngoài C1-C8 không.
