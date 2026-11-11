# Kết quả thử skill 1 và 2 (2026-11-11)

Cách thử: `tests/skill_eval.py`. Mỗi đề chạy 2 lần bằng `claude -p` (model sonnet, phiên mới, thư mục trống): có skill (`.claude/skills/`) và không skill (`--disable-slash-commands`). Điểm = số ý trong bảng chấm mà câu trả lời nhắc tới (khớp regex, chấm thô). Kết quả thô: `work/skill_eval.json`. Mỗi đề chạy 1 lần, chưa đo độ dao động.

| Đề | Skill | Nên kích hoạt | Có skill | Không skill |
|---|---|---|---|---|
| V1 báo xong popup thua, chỉ có test xanh | kiểm tra kết quả | có | kích hoạt, 5/5 | 0/5 |
| V2 xác nhận màn Thắng sẵn sàng | kiểm tra kết quả | có | kích hoạt, 5/5 | 1/5 |
| V3 hỏi asmdef là gì | kiểm tra kết quả | không | không kích hoạt | không kích hoạt |
| O1 đầu phiên, plan nói làm cửa hàng | làm việc với AI | có | kích hoạt, 4/4 | 2/4 |
| O2 hỏi còn job nền nào chạy không | làm việc với AI | có | kích hoạt, 3/3 | 1/3 |
| O3 hỏi Sprite khác Texture2D | làm việc với AI | không | không kích hoạt | không kích hoạt |

Giới hạn: regex chấm thô (có thể trúng nhầm/trượt); đề chỉ là văn bản, chưa có dự án thật; chưa thử "phiên mới thật trong repo game".

## Skill 3 (using-game-shell), 2026-11-12
| Đề | Nên kích hoạt | Có skill | Không skill |
|---|---|---|---|
| S1 thêm thưởng cuối tuần | có | kích hoạt, 6/6 | 3/6 |
| S2 thêm popup kho đồ | có | kích hoạt, 6/6 | 2/6 |
| S3 hỏi struct và class | không | không kích hoạt | không kích hoạt |

Lưu ý: đề S1, S2 đều nhắc "shared shell package" nên có thể giúp kích hoạt; bảng chấm là regex thô; mỗi đề chạy 1 lần.
