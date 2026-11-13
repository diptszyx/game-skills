# Đưa bàn chơi thật của game-2 vào khung của tool (2026-11-13)

Mục tiêu: trên điện thoại, game-2 chạy trong khung của tool với bàn thật (chim, băng, cá, hố, hiệu ứng), thay cho bàn giả. Game-2 gốc **không bị đụng**; mọi việc làm trong `game-2-v2`.

Nguồn số liệu: `work/inv/report.md` (Codex đọc, tôi đã kiểm các điểm chính: 514 / 331 / 341 dòng, 28 chỗ dính giao diện cũ, canvas của tool phủ lên trên).

## Vì sao không chỉ copy được
Bàn thật vẽ bằng sprite trong thế giới 3D qua camera chính. Khung của tool là canvas vẽ **đè lên** hình camera, và khung có phần ruột đặc màu xanh, nên nó sẽ che mất bàn. Cũng không cắt được phần tràn ra ngoài khung.

## Cách làm đề xuất
Cho camera của bàn vẽ vào một **ảnh trung gian** (render texture), rồi hiện ảnh đó bên trong khung. Khi đó bàn tự co theo khung, bị cắt đúng mép, và tool không đổi gì. Chạm vào bàn được đổi từ tọa độ màn hình sang tọa độ trong khung.

## Trạng thái 2026-11-13 (đợt 1 xong)
- Tool: `GameViewportView` (camera + ảnh trung gian + đổi tọa độ chạm hai chiều), `GameHud.Viewport`. Shell EditMode 71/71, PlayMode 36/36, error CS 0.
- Game-2-v2: bàn thật hiện trong khung (chim, băng, nước, cơ chế đứng yên), vuốt chạy qua luật. Pilot EditMode 325/325, PlayMode 4/4. Điện thoại A07 720x1600: ảnh `work/ref/p_play.png`, `p_swipe.png` (vuốt chim, 3 lượt thành 2).
- Chưa làm (đợt 2): hoạt cảnh trượt, bắt cá, băng nứt, tụm đàn, hiệu ứng thắng, âm thanh/rung, gợi ý, hoàn tác.
- Bài học kỹ thuật: mã bàn đặt sprite bằng tọa độ thế giới tuyệt đối; muốn đưa vào vùng chơi phải đổi sang `localPosition`. Camera cảnh khác cũng thấy sprite, nên vùng chơi đặt ở gốc cách xa (offset 10000).
- Test pilot phải đổi một chỗ: bấm vào vị trí chim khi popup Pause mở sẽ đóng popup (bấm nền), nên test chỉ kiểm `InputAllowed()==false`.

## Thêm vào tool: "vùng chơi" (GameViewport)
Bảng đối chiếu bài học: `docs/LESSON_VS_TOOL.md`. Tool chưa có camera / vùng chơi / đổi tọa độ chạm. Làm một lần trong package `com.indiegame.shell` (mọi game dùng), không để trong game-2-v2:
- `GameViewportView`: RawImage nằm trong khung bàn, cỡ theo khung; sở hữu camera + RenderTexture.
- `ScreenToViewport(point)`: đổi tọa độ chạm sang tọa độ thế giới của camera vùng chơi.
- Game chỉ cắm nội dung vào một gốc (`Content`); khe bảng màu / đèn để sau.
- Đo ms/khung trên A07 (bài học: hậu kỳ URP từng +9 ms).
Bước 4 bên dưới dùng nó. Test: EditMode cho đổi tọa độ, PlayMode chạm vào đúng ô.

## Các bước
| # | Việc | Xong khi |
|---|---|---|
| 1 | Chốt danh sách file mang sang và file bỏ (bỏ: HUD, popup thắng/thua/pause/rời ván, home, bảng xếp hạng, quảng cáo, gallery) | Danh sách nằm trong báo cáo của bước |
| 2 | Cắt Board2DView khỏi giao diện cũ, bàn chỉ nhận lệnh và phát sự kiện (vuốt, kết thúc ván) | Biên dịch được, không còn tham chiếu HUD / sheet / SaveStore / SceneManager |
| 3 | Mang art và dữ liệu của bàn (`Resources/Board2D`, không lấy thư mục UI) | Các lệnh nạp bằng tên chuỗi đều tìm thấy file |
| 4 | Vẽ bàn vào ảnh trung gian, đặt trong khung, đổi tọa độ chạm | Ảnh chụp: bàn nằm gọn trong khung, vuốt trúng đúng chim |
| 5 | Nối với luật, popup thắng/thua và pause của tool, khóa input khi đang hoạt cảnh | Thắng, thua, hồi sinh chạy qua khung của tool |
| 6 | Kiểm tự động và trên điện thoại | Pilot vẫn đạt (325 và 4), ảnh chụp 720x1600 đối chiếu với ảnh chốt |

## Rủi ro lớn nhất
Hoạt cảnh của bàn (bắt cá, băng nứt, tụm đàn) có thể gọi code cũ ở nhiều chỗ nhỏ. Chỉ biết chắc sau khi biên dịch ở bước 2.

## Điều cần bạn quyết
1. Đồng ý cách "vẽ vào ảnh trung gian trong khung"?
2. Làm hết hiệu ứng ngay hay chỉ phần cơ bản trước (chim, ô, vuốt) rồi thêm hiệu ứng sau? Tôi đề xuất **cơ bản trước**: bước 1 đến 4 chỉ cần bàn hiện đúng và vuốt chạy được.
