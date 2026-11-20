# Chuẩn art Theme (bản đầu)

Nguồn: `Assets/Art/UI/Pieces/*.png` (IHDR) và `.png.meta` (spriteBorder).
Border theo thứ tự trái / dưới / phải / trên; với Multiple dùng border sprite con.
Import cho art Theme mới: Sprite, Single. Không đổi import của asset hiện hữu.
Kích thước PNG hiện tại là số đo nguồn, không phải kích thước tối thiểu đã duyệt.

| Sprite / vai trò | PNG px | Border px | Import hiện tại | Tỉ lệ đích / sprite |
|---|---|---|---|---|
| panel_popup_plain | 454×486 | x: 40, y: 243, z: 40, w: 215 | 1 (1 Single, 2 Multiple) | 1.19231 / 0.93416 = 1.27635 |
| btn_close | 205×217 | x: 0, y: 0, z: 0, w: 0 | 1 (1 Single, 2 Multiple) | 1.00000 / 0.94470 = 1.05854 |
| toggle_on | 181×117 | x: 0, y: 0, z: 0, w: 0 | 1 (1 Single, 2 Multiple) | chưa đo / 1.54701 |
| toggle_off | 182×117 | x: 0, y: 0, z: 0, w: 0 | 1 (1 Single, 2 Multiple) | chưa đo / 1.55556 |
| btn_primary | 527×254 | x: 155, y: 0, z: 155, w: 0 | 2 (1 Single, 2 Multiple) | 2.79545 / 2.07480 = 1.34733 |
| btn_secondary | 415×217 | x: 132, y: 0, z: 132, w: 0 | 1 (1 Single, 2 Multiple) | 2.79545 / 1.91244 = 1.46172 |
| btn_disabled | 364×209 | x: 0, y: 0, z: 0, w: 0 | 1 (1 Single, 2 Multiple) | 2.79545 / 1.74163 = 1.60508 |
| pause_btn_green | 415×217 | x: 132, y: 0, z: 132, w: 0 | 1 (1 Single, 2 Multiple) | 2.79545 / 1.91244 = 1.46172 |
| pause_btn_blue | 415×217 | x: 132, y: 0, z: 132, w: 0 | 1 (1 Single, 2 Multiple) | 2.79545 / 1.91244 = 1.46172 |
| pause_btn_grey | 364×209 | x: 132, y: 0, z: 132, w: 0 | 1 (1 Single, 2 Multiple) | 2.79545 / 1.74163 = 1.60508 |

Kích thước tối thiểu từng sprite: **chưa đo**. Vùng mặt nút: tâm nhãn 0,41 chiều cao từ trên (`Assets/Scripts/View/FaceLabel.cs:7`); biên vùng mặt: **chưa đo**. Padding nhãn 24×16 (`Assets/Scripts/Editor/UiKit.cs:85`). Các vùng mặt của panel, close, toggle: **chưa đo**.

Nút và panel dùng Sliced (`UiKit.cs:81–82`, `LeaveConfirmSheetPrefabBuilder.cs:26`); tỉ lệ khác nhau không tự chứng minh sprite bị méo. Toggle có nhiều cỡ theo popup, chưa chốt một tỉ lệ đích.

AMBIG: FontSize nút có nhiều giá trị; mặc định style dùng 34 từ `LeaveConfirmSheetPrefabBuilder.cs:34–37` và `ResetConfirmSheetPrefabBuilder.cs:34–37`. Neutral chưa có style tương đương ở hai popup này; dùng cùng mặc định 34 (**TỰ ĐẶT** cho Neutral). Các tham chiếu font/material/sprite để null khi CreateInstance; gán bằng asset Theme phía host ở bước sau. Đường dẫn nguồn font/material: `Assets/Scripts/Editor/UiKit.cs:12–14`.
