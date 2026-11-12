# Khung game cần những hình nào (lấy từ code, 2026-11-12)

Nguồn: `framework/Packages/com.indiegame.shell/Runtime/Unity/UI/ShellStyle.cs`, `ShellUiFactory.cs`, và `game-kit` `UiTheme.cs`, `UiButtonStyle.cs`. Cột "Bộ 15 mảnh" là `art/kit_v1/`.

## A. Khung đang đòi thật (thiếu một mục là khung báo lỗi khi khởi động)

| Chỗ cắm | Dùng ở đâu | Bộ 15 mảnh | Việc cần làm |
|---|---|---|---|
| Nền màn hình (`ScreenBackground`) | Nền mọi màn và lớp mờ phía sau popup | `bg_home` | Dùng được. Hiện khung chỉ có MỘT nền chung. Nền màn chơi là việc của từng game |
| Nền bảng popup (`PanelSprite`) | Mọi popup | `panel_popup` | Cần cắt nền, chia 9 mảnh để co giãn. Dải ruy băng đỏ phải tách riêng (chia 9 sẽ làm méo) |
| Nút chính (`Primary.Enabled`) | Nút chính mọi màn | `btn_primary` | Cắt nền, chia 9 |
| Nút phụ (`Secondary.Enabled`) | Nút phụ | `btn_secondary` | Cắt nền, chia 9 |
| Nút khóa (`LockedButtonSprite`) | Nút chưa mở | **Thiếu** | Vẽ: nút xám có ổ khóa |
| Hàng danh sách (`ListRowSprite`) | Nhiệm vụ, bảng xếp hạng, cửa hàng | **Thiếu** | Vẽ: thanh ngang trung tính |
| Icon vàng (`CurrencyIcon`) | Bắt buộc có | `icons` (đồng xu) | Cắt riêng từng icon |
| Icon tim (`HeartIcon`) | Bắt buộc có | `icons` (tim) | Cắt riêng |
| Icon booster (`BoosterIcon`) | Bắt buộc có | `icons` (búa, hoa tuyết, bom) | Cắt riêng, chọn một mặc định |
| Font (`Theme.Font`) | Mọi chữ | **Thiếu** (không phải hình) | Chọn font có sẵn miễn phí hợp phong cách, tạo font cho Unity |
| Màu (nền, chữ, thanh tiến độ, hàng) | Thanh tiến độ, hàng, chữ | Không phải hình | Lấy từ bảng màu của phong cách đã khóa |

## B. Khung có trong tùy chọn nhưng chưa có hình
| Chỗ cắm | Ghi chú |
|---|---|
| Nút đóng (`CloseSprite`) | `panel_popup` đang có nút đóng đính liền; cần tách thành mảnh riêng |
| Công tắc bật tắt (`ToggleOn/Off`) | Cho màn cài đặt. Thiếu |
| Nút trung tính (`Neutral`), nút khóa của nút chính và phụ | Thiếu |

## C. Có trong bộ 15 mảnh nhưng khung hiện chưa vẽ chỗ nào
`pill_currency`, `pill_lives` (thanh vàng và tim phía trên), `logo`, `penguin_*`, `tiles`, `fish_and_seal`, `bg_play`. Đây là hình của từng game hoặc của phần giao diện mà khung chưa có. Logo dùng cho màn tải/home của game.

## Phát hiện quan trọng
Khung hiện chỉ vẽ: nền, hàng danh sách, bảng popup, lớp mờ, nút (chính, phụ, khóa), chữ, thanh tiến độ. **Khung chưa có** thanh tiền và tim phía trên, thanh tab dưới đáy, ô ngày điểm danh, nhãn thông báo đỏ, avatar, thẻ ưu đãi. Vì vậy màn home của khung nghèo hơn nhiều so với Colony Flow. Thiếu hình chỉ là một nửa vấn đề; nửa kia là khung chưa có các thành phần đó.
