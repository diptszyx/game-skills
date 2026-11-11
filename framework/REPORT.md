# Báo cáo lát cắt Save

Chỉ tạo file trong `framework/`; không chạy Unity hoặc Git.

File tạo (ngoài output bin/obj):
- `Packages/com.indiegame.shell/package.json`
- `Packages/com.indiegame.shell/Runtime/Core/IndieGame.Shell.Core.asmdef`
- `Packages/com.indiegame.shell/Runtime/Core/Save/{SaveResult,SaveDocument,ISaveFileSystem,SaveStore}.cs`
- `dotnet/Shell.Core/Shell.Core.csproj`
- `dotnet/Shell.Core.Tests/{Shell.Core.Tests.csproj,SaveTests.cs}`
- `fixtures/save/{valid-v1,missing-fields,corrupt,future,future-large,invalid-section}.json`
- `README.md`, `.gitignore`, `REPORT.md`

Ba lệnh kiểm, tất cả exit code 0:

`cd framework && dotnet build dotnet/Shell.Core -c Release`
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

`cd framework && dotnet test dotnet/Shell.Core.Tests -c Release`
```text
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 57 ms - Shell.Core.Tests.dll (net10.0)
```

`grep -rn "UnityEngine" framework/Packages/com.indiegame.shell/Runtime/Core | wc -l`
```text
       0
```

Test thực sự chạy (20 phương thức, 21 ca vì theory có hai đầu vào):
- ReadsValidV1: đọc fixture v1.
- MissingFieldsAndSectionsUseDefaults: thiếu version, thuộc tính DTO, phần dùng mặc định.
- CorruptPrimaryRecoversBackup: JSON chính hỏng, khôi phục .bak, có mã lỗi/thông điệp/cờ.
- BothCorruptResetAndReport: cả hai hỏng, tài liệu mới rỗng và báo đặt lại.
- FutureVersionRefusesOverwrite (2 ca): future tại chính hoặc .bak không bị ghi đè.
- ArbitrarilyLargeFutureVersionIsNeverTreatedAsCorruption: version vượt long vẫn chặn.
- MigrationV1ToV2PreservesOtherSections: nâng cấp, giữ phần khác, ghi/đọc v2 và backup v1.
- MissingMigrationIsErrorWithoutReset: thiếu migration báo lỗi.
- ThrowingMigrationReportsErrorAndLeavesDiskUntouched: migration ném lỗi được báo, không sửa đĩa.
- InterruptedTempWriteKeepsPrimaryAndBackup: giả ném sau ghi tạm; chính và backup nguyên vẹn.
- ReplaceFailureKeepsPrimary: giả lỗi thay thế, chính còn nguyên.
- IndependentSectionsAndUnknownGameSectionSurvive: sửa một phần không mất phần khác/phần game riêng.
- RoundTripPreservesLargeNumbersAndUnicode: long.MaxValue, decimal 29 chữ số, unicode.
- FirstWriteThenReplaceCreatesBackupOfPreviousSave: Move lần đầu, File.Replace thật lần sau và .bak đúng.
- SavingRecoveredDocumentPreservesGoodBackup: không thay .bak tốt bằng chính hỏng.
- NewPathReturnsEmptyWithoutRecoveryFlags: lần đầu không báo hỏng.
- MissingPrimaryRecoversBackup: mất chính vẫn đọc .bak.
- InvalidSectionReportsConversionError: lỗi kiểu dữ liệu trả kết quả rõ.
- ReadIoFailureIsNotResetOrOverwritePermission: lỗi I/O không cấp phép đặt lại/ghi đè.
- InvalidArgumentsAndDocumentVersionReturnErrors: đối số sai, version tài liệu tương lai, đăng ký sai.

Chưa test: Unity import/EditMode, IL2CPP/stripping, mất điện/ngắt tiến trình thật, nhiều writer, mọi biến thể lỗi hệ điều hành/quyền truy cập. Các mục Unity không thuộc phạm vi được phép chạy.

AMBIG — chọn đơn giản:
- SchemaVersion thiếu coi là v1; Sections thiếu là rỗng; phần thiếu/null gọi factory; thuộc tính DTO thiếu giữ initializer của DTO.
- Phiên bản hiện tại do SaveStore cấu hình (mặc định 1); migration đăng ký từng bước n→n+1, chạy trong RAM, không tự ghi. Migration tự xử lý lỗi từ ReadSection/WriteSection nếu gọi các API đó.
- Một writer mỗi đường dẫn, thư mục cha tồn tại sẵn; .tmp cố định, có thể còn sau lỗi và được ghi lại ở lần thử sau.
- Ghi lần đầu dùng Move; khi đã có chính dùng File.Replace; giữ .bak tốt nếu chính hỏng bằng backup=null. Không dùng phương án xóa chính nếu Replace không được hệ điều hành hỗ trợ.
- Khôi phục/đặt lại có Value và cờ nhưng Error=InvalidData (Success=false), để bên gọi không bỏ qua sự cố. Lỗi I/O/migration/future trả null, không tự đặt lại.
- Constructor cấu hình sai ném ArgumentOutOfRangeException; các thao tác đọc/ghi, phần và đăng ký migration trả SaveResult có mã lỗi/thông điệp.
