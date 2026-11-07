# Viết skill theo tài liệu chính thức (đọc 2026-11-07, qua agent claude-code-guide)
Nguồn: code.claude.com/docs/en/skills · code.claude.com/docs/en/plugins · platform.claude.com/docs/en/agents-and-tools/agent-skills/{overview,best-practices} · agentskills.io/specification · github.com/anthropics/skills (skill-creator). Công cụ tải trang trả bản rút gọn, câu trích không đối chiếu từng ký tự; chưa đọc trang `plugin-evals`, `plugins/create`.

## Quy tắc (mỗi dòng = một điều kiểm được)
1. Thư mục `<name>/SKILL.md`, frontmatter YAML ở dòng đầu. `name` ≤ 64 ký tự, chữ thường/số/gạch ngang, khớp tên thư mục, không chứa `anthropic`/`claude`. `description` 1–1024 ký tự (Skills API), Claude Code cắt gộp description + when_to_use ở 1536.
2. Chỉ `name` + `description` luôn nằm trong context (~100 token/skill); thân chỉ nạp khi kích hoạt. Mô tả quyết định kích hoạt: ngôi thứ ba, nói việc làm + KHI NÀO dùng + từ khóa thật, use case chính đứng đầu; hơi "pushy" vì Claude hay under-trigger.
3. Thân < 500 dòng (khuyến nghị < 5000 token); chi tiết tách `references/`, `scripts/`, `assets/`; tham chiếu CHỈ MỘT cấp từ SKILL.md; file > 100 dòng có mục lục.
4. Chỉ ghi điều Claude chưa biết. Giải thích lý do, không MUST/ALWAYS/NEVER viết hoa. Mức tự do theo độ mong manh: văn bản (cao) → template → script cố định (thấp).
5. Workflow có checklist; vòng phản hồi validate → sửa → lặp; ví dụ input/output cụ thể; thuật ngữ nhất quán; không thông tin theo thời gian; đường dẫn `/`.
6. Script cho thao tác xác định: xử lý lỗi rõ, không hằng số vô cớ; ghi rõ "chạy" hay "đọc".
7. Test: tối thiểu 3 tình huống (gồm tình huống KHÔNG nên kích hoạt), chạy baseline không skill, thử với model sẽ dùng, đánh giá trong phiên mới (mô hình Claude A viết / Claude B dùng). Kích hoạt đúng ≠ làm đúng. `claude plugin validate <dir>` kiểm cấu trúc.
8. Quy tắc PHẢI đúng mọi lúc: dùng hook, không chỉ ghi trong skill. Claude Code không đọc lại file skill ở các lượt sau; khi compact chỉ giữ 5000 token đầu mỗi skill.
9. Bộ nhiều skill: tên theo một mẫu; description khác nhau rõ (hàng trăm skill chọn bằng mô tả); gerund (`processing-pdfs`), tránh `helper/utils`; theo miền trong `references/`; plugin là đơn vị đóng gói. Quy tắc chính thức về chồng lấn và số lượng tối đa: KHÔNG tìm thấy.
10. Skill nguồn lạ phải kiểm trước khi dùng.
