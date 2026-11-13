# Kế hoạch qua đêm (2026-11-12)

Bạn treo máy, tôi chạy một mạch. Mọi con số dưới đây là mục tiêu, chưa đo.

## Trạng thái lúc bắt đầu
- Code khung gameplay (nút bánh răng, khung bàn chơi) đã viết, art thật đã có. Chưa kiểm trên điện thoại.
- Kiểm tự động: shell 35/35 PlayMode đạt, EditMode 70/71 (1 test đếm file art, đã sửa, chưa chạy lại).
- Pilot (game thử) chưa chạy lại sau khi sửa. Đang chờ Unity của game-5 rảnh.

## Điều kiện để chạy được cả đêm
1. **Cắm điện thoại và bật USB debugging.** Lúc này máy chưa thấy điện thoại; không có nó thì các bước 5, 6 bên dưới sẽ bị bỏ qua.
2. **Phiên game-5 đừng mở Unity**, nếu không tôi phải chờ. Tôi chờ tối đa 30 phút mỗi lần rồi bỏ qua bước cần Unity và làm việc khác.
3. Máy đừng ngủ (`caffeinate` tôi tự bật).

## Các bước, theo thứ tự
| # | Việc | Xong khi |
|---|---|---|
| 1 | Chạy lại kiểm tự động: shell, pilot | Shell 71/71 và 35/35, pilot 325/325 và 4/4, không lỗi biên dịch |
| 2 | Sửa 3 lỗi nhìn thấy: dải xám đầu màn, ô vuông trắng ở góc icon, nhãn tab chạm mép | Ảnh chụp Home và Gameplay không còn 3 lỗi |
| 3 | Build APK, cài lên điện thoại, chụp Home và Gameplay, đối chiếu với mockup | Ảnh trên điện thoại đặt cạnh mockup, tôi ghi rõ chỗ nào còn lệch |
| 4 | Gộp code dựng style chunky đang bị lặp ở hai chỗ vào package | Kiểm tự động vẫn đạt, còn một nơi dựng style |
| 5 | Tạo thử một game nhỏ thứ hai từ tool, bấm giờ từ đầu đến lúc chạy được | Có số phút thật; ghi tool còn thiếu gì |
| 6 | Viết báo cáo sáng mai | File `docs/MORNING_2026-11-12.md` |

Nếu một bước kẹt: ghi nguyên nhân, làm tiếp bước kế, quay lại cuối đêm.

## Tôi sẽ KHÔNG làm khi bạn vắng
- Không commit, không push (tôi phải hỏi bạn trước). Mọi thay đổi để nguyên, chưa commit.
- Không cài hook hay skill toàn cục.
- Không đổi style đã chốt, không sửa game-2 gốc, game-3, game-5.
- Không viết lại phần 3D (bạn chưa chốt thời điểm).

## Sáng mai bạn sẽ có (nếu mọi thứ suôn sẻ)
1. Ảnh chụp thật trên điện thoại của Home và Gameplay với khung của tool.
2. Số kiểm tự động chạy lại đầy đủ, kèm bằng chứng.
3. Một con số thật: làm một game nhỏ mới bằng tool mất bao lâu, và tool thiếu gì.
4. Danh sách việc cần bạn quyết (commit, hình art còn xấu, font).

## Điều tôi chưa chắc
- Khung bàn và bánh răng mới có đẹp trên điện thoại không: chỉ biết sau bước 3.
- Bước 5 có thể bị kẹt nếu cần Codex mà hết hạn mức; khi đó tôi ghi giờ mở lại và không thử lặp.
