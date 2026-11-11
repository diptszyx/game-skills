Vai: phản biện CHỈ ĐỌC (không sửa file nào). Trả lời tiếng Việt, ngắn, đánh số.
Đọc: docs/OVERNIGHT_PLAN.md, docs/PLAN.md, docs/SHELL_COMPARE.md. Dữ liệu để kiểm: ../game-5/unity/HoleItClone/Assets/Scripts/Rules/*.cs (SaveData, Economy, DailyChallengeRules, NotificationRules), ../game-3/unity/ColonyFlowClone/Assets/Scripts/Home/*.cs, ../game-2/unity/Assets/Scripts/View/Leaderboard/*.cs.
Câu hỏi:
1. Mục tiêu T1 (>=10 module, >=100 test, lõi C# thuần, kiểm bằng dotnet test) có khả thi và đo đúng thứ cần đo không? Liệt kê 10 module cụ thể nên có, dựa vào mã thật đã đọc, và module nào nên bỏ.
2. Cách tổ chức: package Unity (com.indiegame.shell) với Runtime/Core thuần C# (asmdef noEngineReferences) + dự án dotnet test nạp cùng file nguồn: có bẫy nào (Newtonsoft, System.Text.Json, netstandard2.1, Unity 6)? Đề xuất cấu trúc thư mục.
3. Mục tiêu nào trong T1..T7 là số tự đặt vô nghĩa hoặc dễ gian lận (đạt số mà chất lượng kém)? Đề xuất số/lệnh kiểm thay thế.
4. Thứ tự và phụ thuộc: chỗ nào hai việc song song sẽ đụng nhau?
5. Rủi ro lớn nhất khi chạy cả đêm không người; điều gì nên làm đầu tiên để lộ rủi ro sớm.
Với mỗi ý, ghi nguồn (file:dòng) hoặc ghi "suy đoán".
