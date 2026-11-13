# Script đo và ví dụ đã đo

Script `Blender -b` chỉ để đo, render so sánh, bake và xuất. Không dùng nó để chỉnh hình dạng (quyết định 2026-11-12, `../../../../CLAUDE.md` và `game-5/docs/TOOLING.md`).

## Script đo lặp lại được
- Một script `Blender -b -P check.py` (`/Applications/Blender.app/Contents/MacOS/Blender`) mở file .blend đã dựng qua MCP, đánh giá mesh cuối (sau modifier), ghi file kiểm, xuất glTF và render 3 góc. Xóa đầu ra rồi chạy lại phải ra cùng file kiểm.
- Trường của file kiểm: số tam giác so với ngân sách, boundary edges == 0, non-manifold edges == 0, modifier còn lại == 0, vật liệu thực sự dùng, số mặt shade mượt (0 cho kiểu low-poly có cạnh), bounds, origin.
- Quy ước: origin ở giữa đáy, kích thước thật theo đơn vị, glTF Y-up, shade phẳng cho kiểu có cạnh, một vật liệu cho mỗi phần màu.
- Render 3 góc (trước, trên, isometric), ánh sáng trung tính, 512x512; đặt cạnh ảnh thiết kế và ghi trung thực chỗ khác.

## Chạy dưới Codex
`codex exec -s danger-full-access -C <dir> "<task>" < /dev/null`. Cần cho cả Blender MCP (MCP không khởi động dưới `read-only`) và cho `Blender -b` (sandbox `workspace-write` của plugin che GPU). `< /dev/null` bắt buộc khi chạy nền.

## Ví dụ đã đo (phương pháp chỉ-script, đã bị thay thế cho việc dựng hình)
2026-11-12, Blender 5.2.2 LTS: ô băng có cục tuyết, 96 tam giác, 0 cạnh hở, 10 vật liệu, Codex dựng ~3,8 phút; Claude chạy lại từ đầu ra cùng file kiểm. Khác thiết kế: mặt trên mịn hơn, cục tuyết nhỏ hơn. File: `art/blender_trial/` (`build_tile.py`, `check.json`, `REPORT.md`, `tile.glb`, 3 render). Ngân sách 2000 tam giác không có trong `check.json` (chưa kiểm nguồn trong `briefs/T5_blender.md`). Cách script mất nhiều vòng cho vịt (`docs/MESH_LAB_RETRO.md` #1), nên giờ dựng bằng MCP.
