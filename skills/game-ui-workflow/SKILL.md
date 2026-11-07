---
name: game-ui-workflow
description: Process rules and kit usage for building, changing or reviewing game UI (popups, modals, sheets, buttons, HUD, Theme/art pieces, UiTheme, UiBuilder, UiModal, UiValidator, golden screenshots) in the indie-game Unity projects, where Claude orchestrates Codex. Use this whenever a task touches UI in game-2 or later games, before writing any Codex brief for UI work, when a UI result "passes tests" but may look wrong (doubled ribbon, label off the button face, stretched 9-slice), when comparing screenshots, or when timing how long UI work takes, even if the user does not say "workflow" or "skill". Complements unity:ui-ugui, which covers how to edit uGUI hierarchies.
---

# game-ui-workflow (quy trình + cách dùng kit `com.indiegame.ui`)

Lý do tồn tại: mọi cổng tự động của game-2 có thể xanh trong khi UI nhìn sai, vì brief sai hoặc không ai xem ảnh (K0b). Skill này đặt chỗ phải NHÌN và phải ĐO. Khi sửa hierarchy/prefab uGUI, dùng thêm skill `unity:ui-ugui` (cách sửa Canvas, RectTransform, layout); skill này chỉ lo quy trình và nghiệm thu.

Nguồn: `knowledge/game-kit/GAME_KIT_PLAN.md` (K0, K0b), retro game-2. Mỗi quy tắc ghi bằng chứng; không có bằng chứng thì không phải quy tắc.

## 1. Trước khi viết brief: tìm cái có sẵn và NHÌN art
1. `rg` thử xem đã có gì: `UiSheet`, `UiPanel.prefab`, `UiButton`, `Editor/UiKit.cs`, các `*SheetPrefabBuilder`. Game-2 có 10 builder cũ, chỉ 3 dùng `Editor/UiKit.cs` (khuôn cũ của game-2, còn tới K6); popup MỚI dùng kit (mục 6), không thêm cách thứ ba (K0).
2. Mở contact sheet của mảnh art (`game-2/art/ui/pieces/contact_sheet.png`) và NHÌN trước khi tả bố cục. `panel_popup_plain` đã có sẵn ruy-băng tiêu đề; brief viết "dùng ribbon_label làm tiêu đề" khiến Codex thêm ruy-băng thứ hai, mọi cổng xanh (K0b).
3. Dùng mảnh art đã duyệt; không sinh art mới khi bộ đã có (HOME_RETRO #18). "Đẹp" do người làm game phán; số đo chỉ chặn lỗi (lệch, méo, cắt chữ), không làm đích (HOME_RETRO #20).

## 2. Brief: một bước, một mục tiêu, một cổng
- Mục tiêu một dòng; đọc trước; danh sách file ĐƯỢC tạo/sửa (còn lại cấm); việc đánh số; lệnh kiểm kèm kết quả mong đợi; cấm làm gì; trần thời gian; mẫu báo cáo.
- Không gộp nhiều bước (PENGUIN3D_RETRO #28: ~4 giờ mất vì brief gộp).
- Chỗ docs im lặng: ghi `AMBIG`, đừng để Codex tự chọn lặng lẽ.
- Ngưỡng nào không có nguồn: ghi "TỰ ĐẶT" trong code và báo cáo (PENGUIN2D_RETRO #2).

## 3. Cổng nghiệm thu (đủ cả ba, không cái nào thay cái nào)
1. Test xanh VÀ không có lỗi biên dịch MỚI: `game-2/tests/support/cs_errors.sh mark` trước việc, `check` sau (đếm `error CS` từ mốc; đếm cả Editor.log luôn dính lỗi cũ, K1a/K1c), hoặc `unity command eval -- 'return UnityEditor.EditorUtility.scriptCompilationFailed.ToString();'` phải ra False. Unity có thể chạy test trên assembly cũ và báo xanh (BOARD3D_RETRO #11).
2. Người điều phối (Claude) tự XEM ảnh chụp ở 20:9 (720x1600) và 16:9 (720x1280) trước khi cho qua; ảnh ghép một khung, không gửi nhiều ảnh (PENGUIN3D_RETRO #36). Báo cáo "0 vòng sửa" của Codex không thay cho việc xem (K0b: lỗi ruy-băng đôi lọt với "0 vòng sửa").
3. `UiValidator` và ảnh golden phải xanh (đã có; cách chạy ở mục 6).
Chạy lại độc lập kết quả của Codex trước khi báo; không tin báo cáo công cụ (PENGUIN2D_RETRO #10).

## 4. Chạy Unity
- Một Editor mỗi project. Editor đang mở thì dùng `unity recompile` và `unity command ...`; không chạy `unity test/run/build` kiểu cũ (khoá project). Chi tiết: `game-2/docs/UNITY_LIVE.md`.
- `run_tests --filter` không dùng `|`: trả 0/0. Chạy từng lớp test riêng, `--async_tests true`, rồi `test_status` tới `completed`.
- Khi Codex đang giữ Unity, Claude không biên dịch hay chạy test (domain reload làm hỏng test của Codex).

## 5. Đo thời gian
- Đo bằng `date +%s` trong file, không ước lượng. Ghi ai đo, có sẵn mẫu hay không, có vòng duyệt của người làm game hay không. Số lạc quan phải ghi là lạc quan (K0).
- Mốc đo nền hiện có: popup dựng với mẫu sẵn 85–98 s (Claude, Codex); sửa lỗi nhìn thấy 29 s. Thời gian dựng popup không còn là nút thắt; lỗi lọt cổng mới là nút thắt (GAME_KIT_PLAN K0b).

## 6. Cách dùng kit `com.indiegame.ui` (đã kiểm ở K3a–K4, 2026-11-06)
Kit là package UPM nhúng ở `game-2/unity/Packages/com.indiegame.ui` (namespace `GameKit.UI`, Editor: `GameKit.UI.Editor`; không `Game2.*`; game phụ thuộc kit, một chiều). Mục tiêu: popup mới = một `ModalSpec` + một Theme, không viết mã UI mới.
**Thành phần:** `UiTheme` (ScriptableObject chỉ dữ liệu: sprite panel/nút/đóng, `UiButtonStyle` Primary/Secondary/Neutral kèm `FaceCentreFromTop`, font, material, màu, `CurvedTitle`, `Motion`, `ScrimColor`); `UiBuilder.BuildModal(theme, ModalSpec, prefabPath)` (Editor, dựng prefab: scrim gốc, `Panel`, `Title`, `Body`, tối đa 3 nút, `Close`, gắn `UiModal`; thiếu trường Theme thì ném lỗi nêu tên trường); `UiModal` (mở/đóng theo `Theme.Motion`, hủy chuyển động đối nghịch, ngăn xếp, Back chỉ đóng modal trên cùng); `UiSafeArea`; `UiValidator` (Editor, mức Error/Warning, option `ManagedNamespacePrefixes`, `SkipLabelCheck`, `FaceCentreFromTop`, `MinTouchPx`); `DemoThemeFactory.Create(folder, font)` (Theme sinh bằng mã, dùng làm Theme B hoặc mẫu).
**Quy trình popup mới:**
1. Có Theme của game chưa? game-2: `Assets/Art/UI/Themes/Game2Theme.asset` (tạo bởi `Game2ThemeBuilder`, art có sẵn). Game mới: viết builder tương tự, tuân `Documentation~/art-standard.md` (sprite Single, border đúng, mặt nút).
2. Viết `ModalSpec` (Title, Body, Buttons[{Label, Kind}], Close) trong mã game; gọi `UiBuilder.BuildModal`. KHÔNG sửa kit; nếu thấy phải sửa kit thì dừng và báo (kết quả nghiệm thu: K4 không sửa dòng nào).
3. Validator: 0 Error (Warning in ra báo cáo, đừng ép về 0). Golden: chụp 20:9 + 16:9 bằng `UiCapture`, so `UiGolden.AssertMatches`; golden mới tạo MỘT lần bằng `UpdateMode` rồi tắt, rồi tự XEM ảnh (mục 3).
4. Đổi nhìn = đổi Theme (A→B→A phải cho ảnh giống hệt, `ThemeSwap_AThenBThenA_IsIdentical`).
**Bẫy đã gặp (đừng lặp):**
- Validator xanh mà ảnh hỏng (tiêu đề đè thân, nút đóng không có dấu X, góc răng cưa vì sprite quá nhỏ): luôn xem ảnh.
- Tên trùng giữa `GameKit.UI` và `Game2.View` (`CurvedText`, `UiSheet`): dùng alias `using CurvedText = Game2.View.CurvedText;`; kit đặt `UiModal` chứ không `UiSheet`.
- PlayMode test tạo asset trong Play Mode làm treo Editor: tạo asset bằng `IPrebuildSetup`/`IPostBuildCleanup`.
- Hộp thoại "Save / Don't Save" của scene thử chặn Editor (`test_status` kẹt `running`): `unity command cancel_tests`, bấm Don't Save (AppleScript như `game-2/tests/support/unity_dialog_guard.sh`), chạy lại.
- Package mới/asset mới: `eval 'AssetDatabase.Refresh()'` trước khi chạy test, không thì ra 0/0.
- Phím trong test: so `isPressed` giữa hai khung, không `wasPressedThisFrame` (đã dính ở Back lồng nhau).
- Brief cho Codex phải liệt kê mọi asmdef được sửa (test asmdef thiếu reference làm Codex dừng ở K3b) và nói rõ alias được phép.
- Chạy `unity command run_tests` từng lớp một trong zsh; `set -- $spec` không tách từ, dùng hàm với tham số rõ ràng.
**Điểm vào cụ thể (game-2):**
- Mã popup mới: builder Editor ở `unity/Assets/Scripts/Editor/<Tên>ModalBuilder.cs` (static `Build()` gọi `UiBuilder.BuildModal(theme, spec, "Assets/Resources/Board2D/UI/<Tên>.prefab")`), theme nạp bằng `AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Art/UI/Themes/Game2Theme.asset")`. Mẫu chạy được: `Tests/PlayMode/KitAcceptanceTests.cs` (`GiftReady`). Asmdef: `Game2.Editor` đã tham chiếu `GameKit.UI` + `GameKit.UI.Editor`; test PlayMode game-2 là `Game2.Tests.PlayMode.asmdef`.
- Hiện tại prefab tải ở runtime bằng `Resources.Load<GameObject>(...)`, instantiate dưới Canvas, `GetComponent<UiModal>().Open()` (xem `LeaveConfirmSheetTests.Setup`); nối vào luồng game (ai gọi Open) là việc riêng, ghi `AMBIG` nếu brief chưa nói.
- Validator: `GameKit.UI.Editor.UiValidator.Check(prefab, options)`; game-2 dùng adapter `Game2.EditorTools.UiValidator.Check/RunAll()` (báo cáo `unity/Logs/ui_validator.txt`); với prefab kit dựng: `new UiValidatorOptions { ManagedNamespacePrefixes = new[]{"GameKit."}, SkipLabelCheck = _ => false, FaceCentreFromTop = theme.Primary.FaceCentreFromTop }`.
- Chụp + golden: `Tests/PlayMode/UiCapture.Render(camera, w, h)` và `UiGolden.AssertMatches(image, "<tên>")`; golden ở `Tests/PlayMode/GoldenImages/`, ảnh thật ở `unity/Logs/screenshots/`; mẫu: `ResetConfirmSheetTests.Capture`.
- `Close` mặc định true (có nút X); game-2 chỉ tiếng Anh, không có bảng ngôn ngữ.
**Chưa kiểm (đừng khẳng định):** chạy trên điện thoại A07 (làm ở K6), game thứ hai thật sự dùng kit, và "đẹp" (do người làm game phán).

## 7. Project Unity MỚI dùng kit (đo ở K7, `~/project/kit-trial`, 2026-11-06)
Kit nạp được bằng `Packages/manifest.json` `"com.indiegame.ui": "file:<đường dẫn tuyệt đối tới Packages/com.indiegame.ui>"`; nó tự kéo ugui 2.0.0 và inputsystem 1.20.0. Đường nạp sạch, kit không bị đổi file. Nhưng project trống cần thêm các bước sau, thiếu bước nào là đỏ:
1. **asmdef (bắt buộc):** hai asmdef của kit để `autoReferenced=false`, nên mã project phải có asmdef tham chiếu `GameKit.UI` (+ `GameKit.UI.Editor` cho builder Editor, `includePlatforms: ["Editor"]`) và `Unity.TextMeshPro`, `UnityEngine.UI`; mã chạy thêm `Unity.InputSystem`. Mã ở `Assets/Editor` không có asmdef sẽ báo CS0246 namespace `GameKit`.
2. **TMP Essentials:** project mới chưa có, `TMP_Settings.defaultFontAsset` ném NullReferenceException. Import bằng `AssetDatabase.ImportPackage` từ `Library/PackageCache/com.unity.ugui*/Package Resources/TMP Essential Resources.unitypackage`, **không** dùng `-quit` (kết thúc trước khi nhập), thoát khi `importPackageCompleted`.
3. **Input:** sau cài Input System, `activeInputHandler` vẫn là 0 (legacy): đổi `ProjectSettings/ProjectSettings.asset` thành `activeInputHandler: 1`, mở Unity mới. Scene cần `EventSystem` + `InputSystemUIInputModule` (gọi `AssignDefaultActions()`), Canvas có `GraphicRaycaster`.
4. **Test framework:** template trống không có `com.unity.test-framework` (thêm 1.6.0, bản đi kèm Unity 6000.3); test asmdef: references `Unity.InputSystem`, `Unity.InputSystem.TestFramework`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `overrideReferences` + `nunit.framework.dll`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`.
5. **Kit không nối nút với mã game:** mỗi nút mang `ModalButton.Id` (tên Transform); mã game tự gắn listener theo Id rồi gọi `Close()`.
6. **Test input giả (bẫy):** `InputSystem.AddDevice<Mouse>` + `QueueStateEvent` thô KHÔNG tới UI (click không mở popup, `mouse.position` vẫn 0,0). Dùng `InputTestFixture` (base.Setup/TearDown, tạo thiết bị trước khi load scene), sau mỗi `QueueStateEvent` gọi `InputSystem.Update()` rồi chờ frame. Batch: `Screen.SetResolution` không đổi cỡ (vẫn 640x480); gắn `RenderTexture` 720x1600 / 720x1280 vào camera và tính toạ độ click bằng `RectTransformUtility.WorldToScreenPoint` với chính camera đó. Log điểm click, `RaycastAll`, `Mouse.current` trước khi nghi kit.
7. **Validator không bắt tương phản chữ:** Theme thử của Codex (tiêu đề tím, thân xám đậm trên panel xanh xám tối) qua 0 Error / 0 Warning nhưng gần như không đọc được. Tự kiểm bằng ảnh; luật tương phản là việc K8 (đề xuất).
Số đo thử (Codex tự đo, mẫu một người, không phải trung bình): tạo project + nạp kit ~49 s; builder 3 popup 40 dòng (9 dòng đổi dữ liệu Theme); tổng K7–K7d ~15 phút làm việc, trong đó phần lớn là sửa bootstrap ở các mục 1–4 và 6.
