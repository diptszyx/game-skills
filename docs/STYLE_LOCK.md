# Phong cách đã chốt (2026-11-12, người làm game chọn hướng 2D)

**Hướng:** 2D cartoon "chunky" dựa trên bảng v3.2 (`art/style_lock/reference_chunky.png`). Tư thế trượt lấy ý từ bảng Candy. Ảnh game thật để đối chiếu: `art/style_lock/real_*.png`.

**Brief dùng cho Codex (system prompt, có số đo, bảng màu, điều cấm): `art/style_lock/STYLE_BRIEF.md`. Mọi lần vẽ phải dùng file đó, không dùng bản tóm tắt dưới đây.**

## Luật (rút từ 7 game thật, bài ChatGPT, số đo; chi tiết ở `knowledge/08-mau-va-anh-sang.md`)
1. Hình lớn, đơn giản, một màu chính mỗi hình, một mảng sáng mềm trên đỉnh, mép dưới tối hơn. Không vân, không nhiễu, không tuyết bông.
2. Viền dày, bo tròn, màu viền là màu đậm của chính hình đó (không đen thuần). Mọi mảnh cùng độ dày viền.
3. Một hướng sáng (trên trái, ấm). Bóng đổ nhẹ, tím xanh.
4. Mỗi màn chỉ một nút chính (lớn, đỏ cam hoặc xanh lá, viền sáng). Mọi nút khác nhỏ và dịu.
5. Nền home: cảnh vịnh băng hoàng hôn, mờ, ít chi tiết, ít tương phản. Nền gameplay: tối, trung tính, đơn giản để vật chơi nổi.
6. Nhân vật chính: chim cánh cụt đỏ, khăn, mắt to, dáng nghiêng khi trượt; có cá tính riêng. Chim xanh, vàng cùng hệ, tách nhau bằng độ sáng.
7. Chữ trắng đậm bo tròn, viền tối. Ít chữ.
8. Chi tiết dồn vào chỗ cần (nhân vật, nút chính), nơi khác giữ trống. Không làm mọi thứ bóng như nhau.
9. Mọi mảnh vẽ rời, cùng một hệ; giao diện ghép từ mảnh, không phải một tấm tranh.

## Chưa làm
Không khóa bảng màu số hex cho đến khi có bộ mảnh duyệt. Chưa có cổng tự động phân biệt "cảm giác AI" đạt chuẩn (xem `docs/AI_LOOK_GATE.md`); người làm game duyệt bằng mắt.
