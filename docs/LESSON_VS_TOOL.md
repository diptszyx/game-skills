# Bài học đã ghi vs tool hiện có (2026-11-13)

Nguồn: `lessons/bai-hoc-chung.md` (đọc hết 248 dòng), `game-3/docs/colony_flow/LOOK_findings.md`, `game-5/docs/spec/FIDELITY.md`, mã game-2 (`Board2DView`, `Board2DSceneBuilder`).

## Trả lời câu hỏi "tool có camera, ánh sáng chưa?"
**Chưa.** Tool hiện chỉ có lớp giao diện (canvas, khung, popup, tab). Không có: camera cho vùng chơi, ánh sáng / bảng màu dùng chung, vùng chơi co theo màn hình, đổi tọa độ chạm.
Các bài học camera / ánh sáng đã ghi đều đến từ game 3D (game-3: ortho 125, nghiêng 60°, 1 đèn, toon 4 bậc; game-5: FOV 44, camera tăng độ cao theo level). Game-2 là 2D: `Board2DView` vẽ sprite qua `Camera.main` (ortho 10), không đèn. Vì vậy với 2D, "ánh sáng" gần như không cần; "camera + vùng chơi" thì cần.

## Bảng

| # | Bài học | Tool có chưa | Khoảng trống / việc |
|---|---|---|---|
| 1 | Vùng an toàn, co theo độ phân giải, nhiều tỉ lệ màn (§4) | Có (SafeRoot, canvas 720x1600) | Chưa kiểm 16:9 trong pilot |
| 2 | Bộ nút / chữ / popup làm một lần, 9-slice, khai import (§4, Colony) | Có (ShellKit, importer, popup host) | — |
| 3 | Màn mới dựng từ nền đã duyệt, không tự đặt tọa độ | Có (khung, TopBar, TabBar) | — |
| 4 | Khung hình đích đã duyệt trước khi dựng; so cả màn với ảnh chuẩn | Một phần (`reference_chunky.png`) | Chưa có cổng so tự động ảnh chụp với ảnh chuẩn |
| 5 | Một chuẩn vật liệu / ánh sáng / bảng màu cho cả cảnh | Có cho UI (ShellKitPalette) | **Chưa có cho vùng chơi** |
| 6 | Sprite 2D đặt lên nền: neo chân, bóng, góc nhìn chung (§3) | Không | Thuộc game, cần khe "bảng màu / bóng" ở vùng chơi |
| 7 | Test bằng input thật; viewport co giãn thì đưa sự kiện vào viewport (§7) | Một phần (PlayMode tap UI) | **Chưa có đổi tọa độ chạm cho vùng chơi** |
| 8 | Ảnh nghiệm thu từ asset + shader của engine, trên máy thật | Có (adb screencap) | — |
| 9 | Hiệu năng: hậu kỳ URP +9 ms trên A07, nướng vào material/nền | Không đo | Vùng chơi dùng RenderTexture phải đo ms trên A07 |
| 10 | Lỗi hình là một họ lỗi: quét tự động | Một phần (KitChunkyTests) | Chưa quét ô trắng / viền / kéo giãn |
| 11 | Chụp mọi popup của luồng từ bản chuẩn | Một phần | Popup thắng/thua/pause của tool chưa so với chuẩn |
| 12 | Cổng so với ẢNH GỐC cùng máy (game-5 R2) | Không | Như dòng 4 |
| 13 | Một luồng việc một session; Monitor; không 2 Unity | Quy trình, không phải tool | Đang tuân thủ |
| 14 | Hoạt cảnh theo chuỗi hệ quả | Không (thuộc game) | Bàn thật của game-2 đã có |

## Kết luận
Ba thứ tool thiếu thật cho "khung dùng được với game có bàn thật":
1. **Vùng chơi (game viewport)**: một ô trong khung, có camera riêng vẽ vào RenderTexture, hiện bằng RawImage, co theo khung, cắt đúng mép.
2. **Đổi tọa độ chạm** từ màn hình sang vùng chơi (để vuốt trúng chim).
3. **Khe bảng màu / ánh sáng của vùng chơi**: để game 2D chỉ cần nền + tint; game 3D cắm đèn + toon. Làm sau, chỉ khi có game 3D dùng tool.

Cổng so ảnh chụp với ảnh chuẩn (dòng 4, 12) nên thêm vào skill `verifying-game-results`.
