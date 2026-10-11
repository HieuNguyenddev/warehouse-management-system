# Frontend

Giao diện quản trị viết bằng Next.js. Trình duyệt chỉ gọi cùng origin `/api/...`. Next.js chuyển tiếp sang API trong `API_URL`.

## Cài đặt

Cài [Node.js 22](https://nodejs.org/) trở lên. API phải đang chạy. Cách bật API nằm trong [backend/README.md](../backend/README.md).

```bash
cd frontend
cp .env.example .env
npm install
```

## Biến môi trường

| Biến | Việc |
| --- | --- |
| `API_URL` | Địa chỉ API khi chạy `npm run dev` hoặc `npm start` trên máy. Mặc định `http://localhost:5161`. |
| `API_URL_DOCKER` | Địa chỉ API khi giao diện chạy trong container. Mặc định `http://host.docker.internal:5161`, tức API đang mở cổng 5161 trên máy host. |
| `WEB_PORT` | Cổng giao diện trên máy khi chạy Compose. Mặc định `3000`. |

Đổi `API_URL` rồi khởi động lại `npm run dev`. Container đọc `API_URL_DOCKER` lúc chạy, không cần build lại image chỉ để đổi địa chỉ API.

## Chạy trên máy

```bash
cd frontend
npm run dev
```

Giao diện: [http://localhost:3000](http://localhost:3000)

Trang chính:

| Đường dẫn | Việc |
| --- | --- |
| `/` | Tổng quan số vai trò và người dùng. |
| `/roles` | Thêm, sửa, xóa, tìm và phân trang vai trò. |
| `/users` | Thêm, sửa, xóa, tìm và phân trang người dùng. Gán vai trò đã có. |

## Chạy bằng Docker

Dùng khi deploy hoặc khi không muốn cài Node trên máy đích. API vẫn phải reachable từ container.

```bash
cd frontend
docker compose up -d --build
```

Giao diện: [http://localhost:3000](http://localhost:3000)

Nếu API cũng nằm trong Docker và hai Compose không chung mạng, giữ `API_URL_DOCKER=http://host.docker.internal:5161`. Khi đưa cả hai lên một mạng Docker, đặt `API_URL_DOCKER` thành địa chỉ service API, ví dụ `http://warehouse-api:8080`, rồi tạo lại container:

```bash
cd frontend
docker compose up -d
```

Dừng và giữ image:

```bash
cd frontend
docker compose down
```

`Dockerfile` build ba giai đoạn: `npm ci`, `npm run build` với output `standalone`, rồi image chạy `node server.js`. File `.env` không được copy vào image. Compose truyền `API_URL` lúc chạy.

## Luồng gọi API

1. Trang gọi `fetch("/api/roles")` hoặc `fetch("/api/users")`.
2. Route `src/app/api/[...path]/route.ts` đọc `API_URL` của process và chuyển method, query, body sang API.
3. Trả nguyên status và body của API. API tắt thì route trả `502` với `{ "message": "Không kết nối được API." }`.

Vì trình duyệt không gọi thẳng cổng 5161, CORS của API không tham gia luồng này.

## Thư mục và file

| File | Nhiệm vụ |
| --- | --- |
| `package.json` | Next.js, React, Tailwind, lucide-react. |
| `next.config.ts` | Bật `standalone` để image Docker chỉ chứa server đã build. |
| `Dockerfile` | Build và chạy giao diện. |
| `.dockerignore` | Không đưa `node_modules`, `.next`, `.env` vào ngữ cảnh build. |
| `.env.example` | Mẫu biến môi trường. |
| `.env` | Giá trị trên máy. Không đưa vào Git. |
| `docker-compose.yml` | Service `web`, cổng `WEB_PORT`, `API_URL` lấy từ `API_URL_DOCKER`. |
| `src/app/layout.tsx` | Khung trang, font, bọc `AdminShell`. |
| `src/app/page.tsx` | Trang tổng quan. |
| `src/app/roles/page.tsx` | Trang vai trò. |
| `src/app/users/page.tsx` | Trang người dùng. |
| `src/app/api/[...path]/route.ts` | Proxy sang API. |
| `src/components/admin-shell.tsx` | Sidebar, thu gọn, ô tìm. |
| `src/components/dashboard.tsx` | Thẻ tổng quan. |
| `src/components/role-manager.tsx` | Danh sách và form vai trò. |
| `src/components/user-manager.tsx` | Danh sách và form người dùng. |
| `src/lib/roles.ts` | Gọi `/api/roles`. |
| `src/lib/users.ts` | Gọi `/api/users`. |
