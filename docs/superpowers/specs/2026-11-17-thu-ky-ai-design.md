# Qeq — Thư ký cá nhân AI: Spec thiết kế v1

Ngày: 2026-11-17 · Trạng thái: đã duyệt từng phần trong brainstorm, chờ review file spec

## 1. Mục tiêu

Một sản phẩm độc lập giải quyết bài toán thật: **thư ký cá nhân AI** quản lý chi tiêu, sinh hoạt/kế hoạch và idea cho một người dùng duy nhất (chính tác giả). Đồng thời là phương tiện học: thiết kế hệ thống, điều phối multi-agent (MCP, AG-UI, Microsoft Agent Framework), Agentic RAG (pgvector), và AI Evals (độ chính xác, chống ảo giác).

### Quyết định đã chốt

| Mục | Quyết định |
|---|---|
| Người dùng | Chỉ 1 người → không auth phức tạp, không multi-tenant |
| Nền tảng | Web (Next.js) + Mobile (React Native/Expo) song song, cùng gọi 1 API |
| Backend | .NET 10, Clean Architecture, modular monolith |
| Điều phối agent | Microsoft Agent Framework (LangGraph để giai đoạn 2 so sánh) |
| LLM chat chính | DeepSeek (trả phí, vài chục k VNĐ/tháng) |
| Embedding | Gemini embedding API (free tier) — DeepSeek không có API embedding |
| Vector DB | Postgres + pgvector (1 database cho tất cả; Qdrant để sau) |
| Ngân sách | Trần token/ngày cấu hình được; hết tiền = giảm chất lượng, không chết hệ thống |

### Phạm vi v1

Một khung chat thư ký 3-trong-1 (chi tiêu + kế hoạch + idea), mỗi mảng tối giản; dashboard chi tiêu; bản tin ngày 2 lần; RAG tìm ghi chú; evals cơ bản.

### Ngoài phạm vi v1 (đã chừa chỗ, làm sau)

Ghi giọng nói, tự nhập SMS ngân hàng, Google Calendar time-blocking, sub-agent OCR hóa đơn, Qdrant + GraphRAG, LangGraph, multi-user, offline queue mobile.

## 2. Kiến trúc

### 2.1 Monorepo

```
qeq/
├─ backend/                  # .NET 10 solution
├─ apps/web/                 # Next.js + TypeScript
├─ apps/mobile/              # Expo (React Native) + TypeScript
├─ packages/contracts/       # kiểu TS chung: message, event AG-UI, DTO
├─ docs/
└─ docker-compose.yml        # nginx + web + backend + postgres
```

Hai app không chia sẻ logic nghiệp vụ — chỉ chia sẻ `packages/contracts` và cùng gọi 1 API. Mọi thay đổi nghiệp vụ chỉ làm ở backend.

### 2.2 Backend — Clean Architecture, 6 project

Luật phụ thuộc: lớp trong không biết lớp ngoài tồn tại.

```
Qeq.Domain                      # Entities: Transaction, PlanTask, IdeaNote. Không phụ thuộc gì.
Qeq.Application                 # Use case + interface: ILlmClient, IEmbedder, IRepository…
                                # Agent orchestration (Secretary, Planner) nằm ở đây.
Qeq.Infrastructure.Shared       # DeepSeekClient, GeminiEmbedder, ResilientLlmClient (router), MCP plumbing
Qeq.Infrastructure.Persistence  # EF Core, Postgres + pgvector, repositories, tsvector full-text
Qeq.Infrastructure.Identity     # xác thực 1 người dùng: 1 token cố định, giữ mỏng
Qeq.Api                         # REST controllers, endpoint SSE, MCP server endpoint, BackgroundService
```

Domain tổ chức theo loại: `Entities/` và `Enums/` riêng. Các lớp còn lại chia thư mục con theo mảng nghiệp vụ (Expenses, Planning, Notes, Agents). Đổi provider LLM = thay 1 class ở Infrastructure, agent không đổi.

### 2.3 Sơ đồ tổng thể

```
[Next.js web]        [Expo mobile]
      \                  /
       \   HTTPS + SSE (sự kiện AG-UI)
        \                /
         [nginx: TLS, rate limit, reverse proxy]
                  |
      [.NET 10 Backend — 1 tiến trình]
      ├─ Api: REST CRUD + /chat SSE + MCP server
      ├─ Application: Secretary agent, Planner agent, use cases
      ├─ Infrastructure.Shared: LLM router → DeepSeek / Gemini / Groq
      └─ Infrastructure.Persistence → [Postgres + pgvector]
```

## 3. Thành phần AI

### 3.1 Hai agent

| Agent | Kích hoạt | Việc |
|---|---|---|
| Secretary | Tin nhắn chat | Hiểu ý → chọn tool MCP → trả lời. Agent duy nhất người dùng nói chuyện trực tiếp. Ủy quyền Planner khi được hỏi về kế hoạch |
| Planner | BackgroundService 07:00 + khi Secretary gọi | Đọc việc tồn + chi tiêu hôm qua qua tool → soạn kế hoạch ngày; 22:00 soạn tổng kết ngày |

Cả hai dùng chung hộp tool MCP và chung `ILlmClient`. Không thêm sub-agent theo chủ đề trong v1 — chỉ tách khi có quy trình thật sự khác biệt (GĐ2: OCR hóa đơn).

### 3.2 Bảy tool MCP (v1)

Mỗi tool là lớp vỏ mỏng bọc một Application use case; schema tham số khai báo rõ để LLM biết cách gọi. MCP server đặt trong `Qeq.Api` nên client MCP bên ngoài (Claude Desktop, Qoder…) cắm vào dùng được.

```
Chi tiêu:  add_transaction, query_transactions
Kế hoạch:  add_task, list_tasks, complete_task
Idea:      add_idea, search_notes   # search_notes = RAG qua pgvector
```

Tool không nhận SQL thô — chỉ nhận filter trong danh sách cho phép. Mọi tham số validate lại tại use case.

### 3.3 LLM Router (ResilientLlmClient, Infrastructure.Shared)

Agent chỉ biết `ILlmClient` (theo `IChatClient` của Microsoft.Extensions.AI — nền tảng của Agent Framework). Phía sau:

```
1. DeepSeek        (trả phí, mạnh nhất)
2. Gemini Flash    (free tier)
3. Groq/OpenRouter free (chốt chặn cuối, 0đ)
```

- Circuit breaker: provider lỗi 3 lần liên tiếp hoặc trả 402/429 (hết tiền/hết lượt) → ngắt mạch, chuyển provider kế tiếp; vài phút sau thăm lại provider chính.
- Trần ngân sách ngày theo provider (cấu hình trong Cài đặt, ví dụ 2.000đ/ngày cho DeepSeek): chạm trần → tự tụt xuống free tier.
- UI hiện badge "đang dùng model dự phòng" khi không chạy provider chính.
- Khác biệt API giữa các hãng nhốt trong Infrastructure; agent không đổi.

### 3.4 Tự sửa lỗi của agent — 3 lớp

1. Chặn tại use case: tham số sai → tool trả lỗi có cấu trúc (`{error: "amount_must_be_positive"}`); agent đọc lỗi, tự gọi lại với tham số sửa. Validate không tốn lần gọi LLM nào.
2. Thiếu thông tin bắt buộc → agent hỏi lại người dùng, không đoán (ví dụ: "35k là ăn trưa hay ăn tối?").
3. Tối đa 5 vòng tool-call mỗi tin nhắn; chạm trần → dừng, báo rõ đang kẹt ở đâu.

Mọi vòng phát qua SSE theo sự kiện AG-UI (`TOOL_CALL_START/END`, `TEXT_MESSAGE_CONTENT`) — người dùng thấy agent đang nghĩ gì, gọi tool nào.

### 3.5 Guard bảo mật

| Mối lo | Chặn ở đâu |
|---|---|
| Loop đốt tiền | 5 vòng/tin nhắn + trần token/ngày + rate limit tin nhắn/phút |
| Prompt injection | Dữ liệu từ DB/RAG là data, không phải lệnh — tách rõ trong prompt; không tồn tại tool xóa hàng loạt để bị lợi dụng |
| LLM gọi bậy | Tool chỉ nhận filter whitelist; use case validate mọi tham số — không tin LLM |
| Lộ bí mật | API key/connection string không bao giờ vào prompt hay log gửi LLM |
| Người lạ đốt tiền | Identity bắt buộc token mọi request; nginx rate limit |
| Hành động nguy hiểm | Xóa/sửa nhiều bản ghi → thẻ xác nhận trên UI, người dùng bấm mới chạy |

## 4. Luồng dữ liệu

### 4.1 Ba luồng chính

**Ghi** — "trưa nay ăn 35k":
App → POST /chat (SSE) → SendMessageUseCase → Secretary hỏi DeepSeek → tool `add_transaction(35000, ăn uống)` qua MCP → use case → EF Core → Postgres → SSE đẩy `TOOL_CALL_START/END` + `TEXT_MESSAGE_CONTENT` xác nhận về app.

**Hỏi số liệu** — "tháng này ăn uống bao nhiêu?":
Agent → `query_transactions(filter)` → SQL SUM thật → con số từ DB, LLM chỉ diễn đạt. **LLM không bao giờ tự bịa số.** UI hiện kèm filter/nguồn số đã dùng để người dùng tự kiểm tra.

**Tìm idea (RAG)** — "hồi trước có idea gì về app quét hóa đơn?":
Câu hỏi → Gemini embedding → vector → pgvector similarity (có ngưỡng tối thiểu) → ngữ cảnh + trích dẫn ghi chú gốc → DeepSeek trả lời kèm nguồn bấm mở được. Dưới ngưỡng → "không tìm thấy", không đoán.

### 4.2 Nghiệp vụ theo thời gian

```
07:00 → Planner: việc tồn + chi tiêu hôm qua + việc quá hạn dời sang
        → kế hoạch ngày → push notification → mở app thấy kế hoạch chờ sẵn
22:00 → Tổng kết ngày: tiêu bao nhiêu, việc nào xong, gì dời sang mai
```

- Việc quá hạn: đánh dấu overdue, dồn vào bản tin 07:00 hôm sau — nhắc 1 lần, không spam notification lẻ tẻ.
- Cảnh báo quá tải: tổng giờ ước lượng của việc trong ngày > sức chứa ngày (cấu hình trong Cài đặt, mặc định 8 giờ) → đề xuất dời bớt.
- Cảnh báo ngân sách: chi tiêu danh mục vượt ngân sách tháng (đặt trong Cài đặt) → báo trong chat/bản tin.
- Job lỗi: retry tối đa 3 lần; vẫn hỏng → bản tin template thuần SQL (không cần LLM), ghi rõ "planner lỗi".
- Backend tắt lúc đúng giờ: catch-up — job lỡ lịch chạy bù khi startup; bản tin nằm trong in-app inbox, push chỉ là chuông.

### 4.3 Bảng backup / callback theo chức năng

Nguyên tắc gốc: **LLM là lớp tăng cường, không phải điều kiện sống** — mọi chức năng có đường chạy khi tất cả provider chết.

| Chức năng | Đường chính | Backup khi hỏng | Callback/xác nhận |
|---|---|---|---|
| Ghi chi tiêu/việc/idea | Chat → agent → tool | Router tụt provider; tất cả chết → form ghi tay qua REST thuần (mobile tab ＋) | SSE xác nhận + bản ghi hiện ngay ở dashboard; lỗi giữ nội dung, báo chưa lưu |
| Gửi lại do mạng chập | — | `requestId` idempotent: gửi 2 lần vẫn 1 bản ghi | "Đã lưu" chỉ hiện khi DB xác nhận thật |
| Hỏi số liệu | Agent → SQL → diễn đạt | UI hiện filter/nguồn số để tự kiểm tra | Kèm nguồn; không có dữ liệu → nói thẳng |
| Tìm idea | Gemini embedding → pgvector | Embedding chết → fallback full-text Postgres (tsvector) | Trích dẫn bản ghi gốc |
| Bản tin ngày | Planner agent | Retry 3 lần → template thuần SQL | In-app inbox + badge "bản tin hôm nay" |
| Push chết | Expo push | In-app inbox vẫn đủ | — |
| Mất dữ liệu | — | pg_dump mỗi đêm, giữ 7 bản; khôi phục thử 1 lần/tháng | — |

### 4.4 Xử lý lỗi hệ thống

- SSE đứt: app tự reconnect; `requestId` chống ghi trùng.
- DB/backend chết: API trả lỗi rõ; form giữ nội dung, cho gửi lại; không giả vờ thành công.
- Log: Serilog có cấu trúc, mỗi tin nhắn một `correlationId` xuyên suốt các vòng agent — tua lại được toàn bộ.

## 5. UX / UI

### 5.1 Mô tả công việc

**Secretary** (tham khảo Ohai, Martin, Lindy, Cleo): nhận câu nói tự nhiên → ghi chi tiêu tự phân loại; trả lời số liệu (số từ SQL); ghi việc/idea tự bóc ngày giờ; tick việc xong qua chat; tìm ghi chú ngữ nghĩa kèm trích dẫn; cảnh báo vượt ngân sách danh mục; hỏi lại khi mơ hồ; thẻ xác nhận hành động nguy hiểm. (Sau: giọng nói, SMS ngân hàng.)

**Planner** (tham khảo Motion, Reclaim, Sunsama — lấy tinh thần Sunsama): 07:00 soạn kế hoạch ngày từ việc tồn + deadline + quá hạn; cảnh báo quá tải giờ; 22:00 nghi thức kết ngày; ưu tiên theo deadline + độ quan trọng. (Sau: time-blocking lịch thật.)

### 5.2 Bản đồ màn hình

Nguyên tắc chống ngộp: mỗi màn hình một việc chính; màn hình chính chỉ hiện tóm tắt 1 dòng mỗi khối, bấm mới mở chi tiết (progressive disclosure).

**Web — sidebar 6 mục:**
- Hôm nay (mặc định): bản tin sáng/tối, việc hôm nay, chi tiêu hôm nay — nhìn 5 giây nắm hết
- Chat: hội thoại, thẻ tool, thẻ xác nhận, badge model dự phòng
- Chi tiêu: biểu đồ danh mục + bảng giao dịch (sửa/xóa tay) + ngân sách
- Kế hoạch: việc theo ngày/tuần, quá hạn, tick xong
- Idea & Ghi chú: danh sách + tìm ngữ nghĩa
- Cài đặt: ngân sách LLM/ngày, giờ bản tin, danh mục, token

**Mobile — bottom nav 5 tab:**
`[Chat (mặc định)] [Hôm nay] [＋ Ghi nhanh — nút giữa to, 2 chạm: chọn loại → 2-3 trường → xong] [Kế hoạch] [Khác: Chi tiêu, Idea, Cài đặt]`

**Chung:** dashboard lấy số thẳng từ REST (SQL thật), không qua LLM — nhanh, đúng tuyệt đối, 0đ. Push bấm vào mở đúng màn hình liên quan. Mạng đứt khi ghi: báo rõ chưa lưu, giữ nội dung. Trạng thái agent hiển thị bằng thẻ tool từng bước; lỗi nói rõ kẹt ở đâu.

### 5.3 Design system

- Phong cách: flat minimal, không gradient/shadow nặng, transition 150–200ms, light + dark.
- Màu: primary teal `#0D9488`, accent cam `#EA580C`, nền `#F0FDFA`, chữ `#134E4A`, destructive `#DC2626`. Contrast chữ ≥ 4.5:1.
- Chữ: Plus Jakarta Sans, base 16px, line-height 1.5.
- Tỉ lệ: spacing bội số 8px; web grid 12 cột; mobile 1 cột; vùng chạm ≥ 44×44px; icon SVG Lucide, không emoji.
- Stack UI: web = shadcn/ui + Tailwind; mobile = NativeWind + react-native-reusables. Cùng token màu/spacing.

## 6. Giả thuyết vận hành (phi chức năng)

| Chiều | Giả thuyết | Hệ quả |
|---|---|---|
| Tải | 1 người dùng, ~200 tin nhắn/ngày, peak ~5 req/phút; vài nghìn bản ghi/năm | 1 instance .NET + 1 Postgres nhỏ đủ; API stateless để scale không sửa kiến trúc |
| Độ trễ | REST CRUD p95 < 200ms; chat first-token < 2s, trọn câu < 10s; SSE heartbeat 15s | nginx tắt `proxy_buffering` cho SSE |
| Bảo mật | HTTPS bắt buộc; token mọi request; nginx rate limit ~30 req/phút cho chat; CORS chỉ 2 origin; DB không public; secrets trong env, không commit | Người lạ không đốt tiền; dữ liệu chỉ đi tới provider đã chọn |
| Chi phí | Trần token/ngày/provider; eval ghi nhận tiền mỗi lần chạy | Hóa đơn tháng đoán trước được |

## 7. Triển khai

```
Internet → [nginx: TLS Let's Encrypt, rate limit, reverse proxy]
            ├─→ web Next.js
            ├─→ backend .NET (API + SSE, buffering off)
            └─→ Postgres (mạng nội bộ)
Mobile → cùng 1 endpoint nginx
```

- Docker compose: 1 lệnh dựng cả hệ thống; chạy trên máy cá nhân hoặc VPS/free tier 0đ.
- nginx hiện làm reverse proxy/TLS/rate-limit — chưa phải cân bằng tải đúng nghĩa (1 instance). Khi scale: thêm instance backend stateless phía sau nginx, không sửa code.
- Backup: pg_dump nightly giữ 7 bản; test khôi phục 1 lần/tháng.

## 8. Kiểm thử & AI Evals

### 8.1 Kim tự tháp test

- Unit (xUnit): use case validate tiền/ngày/danh mục, router fallback, idempotency — LLM mock, không tốn tiền, chạy trong CI.
- Integration (Testcontainers + Postgres thật): repository, pgvector, tsvector fallback.
- Contract: schema tool MCP + sự kiện AG-UI khớp `packages/contracts`.
- E2E (Playwright, web): golden path ghi → hỏi → thấy số đúng; chạy tay trước release.
- LLM thật không chạy trong CI mỗi commit — chất lượng đo bằng eval chạy có chủ đích.

### 8.2 AI Evals

Bộ câu hỏi chuẩn ~50–100 câu trên dữ liệu seed cố định, đáp án đúng tính sẵn từ SQL.

| Đo gì | Cách chấm | Ngưỡng v1 |
|---|---|---|
| Chọn đúng tool + tham số | Tự động: so với tool call kỳ vọng | ≥ 90% |
| Số liệu đúng | Tự động: số trong trả lời khớp SQL | 100% (lệch = bug) |
| Không bịa / biết nói không biết | LLM-as-judge model rẻ + spot-check tay | ≥ 95% trả lời "không có dữ liệu" khi thiếu |

Quy trình: đổi prompt/model/tool → chạy lại eval, so điểm trước/sau; ghi lại chi phí mỗi lần chạy.

## 9. Lộ trình

- **GĐ1 (v1):** toàn bộ spec này.
- **GĐ2:** giọng nói, SMS ngân hàng, Google Calendar time-blocking, sub-agent OCR hóa đơn, Qdrant + GraphRAG, LangGraph (dự án so sánh).
- **GĐ3:** multi-user (Identity đã chừa chỗ), offline queue mobile, tách service khi tải thật sự đòi.

## 10. Rủi ro & giả định cần kiểm chứng khi bắt đầu triển khai

1. Tương thích phiên bản .NET 10 ↔ Microsoft Agent Framework ↔ Microsoft.Extensions.AI — kiểm chứng bằng spike nhỏ đầu tiên trước khi viết nghiệp vụ.
2. DeepSeek tool-calling hoạt động ổn với schema 7 tool (chất lượng function calling của deepseek-chat) — đo ngay bằng eval tool-selection.
3. Gemini embedding free tier đủ hạn mức cho ~vài trăm ghi chú/tháng; kiểm tra hạn mức hiện hành khi triển khai.
4. Expo push cần EAS/dev build — xác nhận free tier còn đủ dùng.
5. Chất lượng embedding tiếng Việt của Gemini — kiểm bằng eval search_notes; nếu kém, đổi model embedding khác qua `IEmbedder` (không sửa agent).