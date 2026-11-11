# IndieGame Shell
- `Packages/com.indiegame.shell`: Unity 6000.3 package; Core C# thuần, Newtonsoft.Json.
- `Runtime/Core/Save`: SaveDocument chứa các phần có khóa; SaveStore đọc/ghi qua ISaveFileSystem.
- `dotnet`: cùng nguồn Core netstandard2.1/C# 9 và xUnit net10.0; `fixtures/save`: dữ liệu thử.
- Build: `dotnet build dotnet/Shell.Core -c Release` (từ framework).
- Test: `dotnet test dotnet/Shell.Core.Tests -c Release` (từ framework).
- AMBIG: thiếu SchemaVersion = v1; thiếu/null phần dùng factory mặc định; migration đăng ký bước n→n+1, không tự lưu.
- AMBIG: một writer mỗi đường dẫn; thư mục cha phải tồn tại; ghi đầu dùng Move, lần sau File.Replace + .bak.
- Giữ .bak tốt khi chính hỏng; lỗi I/O/migration không đặt lại; future ở chính hoặc .bak chặn ghi; đọc phục hồi/đặt lại trả mã lỗi và cờ.
- Chưa kiểm Unity/IL2CPP, mất điện thật hay nhiều writer; kiểu DTO do game giữ để tránh stripping.
