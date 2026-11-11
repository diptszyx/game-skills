MỤC TIÊU (một dòng): dựng lát cắt dọc đầu tiên của khung game dùng chung: package Unity + lõi C# thuần + module Lưu + test chạy bằng dotnet.

ĐỌC TRƯỚC: docs/OVERNIGHT_PLAN.md (mục T1a và "Quyết định chính sách"), docs/SHELL_COMPARE.md (mục 7), work/critique1.log (cuối file). Mã tham khảo, CHỈ ĐỌC, không sửa: ../game-5/unity/HoleItClone/Assets/Scripts/Rules/SaveData.cs, HoleIt.Rules.asmdef. Không bê nguyên SaveData: nó nhúng Quest, RollingOffer, Profile.

ĐƯỢC TẠO / SỬA (mọi thứ khác CẤM): thư mục framework/ trong repo này.
Cấu trúc:
  framework/Packages/com.indiegame.shell/package.json   (tên com.indiegame.shell, unity "6000.3", dependency com.unity.nuget.newtonsoft-json)
  framework/Packages/com.indiegame.shell/Runtime/Core/IndieGame.Shell.Core.asmdef  (noEngineReferences true, dùng Newtonsoft)
  framework/Packages/com.indiegame.shell/Runtime/Core/Save/*.cs
  framework/dotnet/Shell.Core/Shell.Core.csproj   (netstandard2.1, LangVersion 9, Compile Include từ ../../Packages/com.indiegame.shell/Runtime/Core/**/*.cs, PackageReference Newtonsoft.Json)
  framework/dotnet/Shell.Core.Tests/Shell.Core.Tests.csproj   (net10.0 hoặc net8.0, xunit)
  framework/fixtures/save/*.json   (bản lưu mẫu: hợp lệ v1, thiếu trường, hỏng cú pháp, phiên bản tương lai)
  framework/README.md  (5-10 dòng: cấu trúc, lệnh chạy test)
  framework/.gitignore (bin, obj)

VIỆC (đánh số):
1. Tạo cấu trúc trên. Namespace IndieGame.Shell.
2. Module Lưu (Save): 
   - Tệp lưu có số phiên bản (SchemaVersion) và các PHẦN theo khóa chuỗi (mỗi module sau này một phần, game riêng thêm phần của mình mà không sửa khung). Đọc/ghi phần theo kiểu (generic), có giá trị mặc định khi thiếu.
   - Hàm nâng cấp theo phiên bản (migration) đăng ký được; phiên bản tương lai hơn bản mã hiểu: không ghi đè, báo lỗi rõ.
   - Ghi NGUYÊN TỬ: ghi ra tệp tạm, rồi thay bản chính bằng File.Replace (có bản sao lưu .bak); tuyệt đối không xóa bản chính trước khi bản mới sẵn sàng. Đọc: nếu bản chính hỏng, thử .bak, rồi mới trả bản mới rỗng kèm cờ "đã khôi phục" / "đã đặt lại".
   - Lớp trung gian cho hệ thống tệp (interface) để test ngắt ghi giữa chừng bằng bản giả; bản thật dùng System.IO.
   - Không ném ngoại lệ nuốt mất lỗi: trả kết quả có mã lỗi và thông điệp.
3. Test xUnit cho từng hành vi: đọc v1 hợp lệ; thiếu trường dùng mặc định; JSON hỏng → khôi phục từ .bak; cả hai hỏng → đặt lại và báo; phiên bản tương lai → từ chối ghi đè; migration v1→v2; ghi bị ngắt giữa chừng (bản giả ném lỗi sau khi ghi tạm) → bản chính còn nguyên; hai phần độc lập không ghi đè nhau; vòng ghi-đọc giữ nguyên dữ liệu (kể cả số lớn, chuỗi unicode).
4. Dùng các fixture trong test (không viết JSON cứng trong test nếu fixture có sẵn).

LỆNH KIỂM (chạy và dán kết quả vào báo cáo):
  cd framework && dotnet build dotnet/Shell.Core -c Release
  cd framework && dotnet test dotnet/Shell.Core.Tests -c Release
  grep -rn "UnityEngine" framework/Packages/com.indiegame.shell/Runtime/Core | wc -l   (mong đợi 0)
Mong đợi: build 0 lỗi, test 0 lỗi (đếm số test), không có cảnh báo nullable bị tắt hàng loạt.

CẤM: sửa ngoài framework/; git commit/push; chạy Unity; đổi Newtonsoft sang System.Text.Json; thêm module ngoài Lưu; nuốt lỗi.
TRẦN THỜI GIAN: 45 phút. Chỗ tài liệu im lặng: ghi AMBIG trong báo cáo, chọn phương án đơn giản nhất và nói rõ.
BÁO CÁO (cuối cùng, ngắn): file đã tạo; kết quả ba lệnh kiểm (nguyên văn dòng tổng); danh sách test và hành vi nào chưa test; AMBIG.
