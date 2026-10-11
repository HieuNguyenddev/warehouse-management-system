# warehouse-management-system

Hệ thống quản lý kho. Backend là ASP.NET Core Web API trên .NET 10 và SQL Server. Frontend là Next.js, chạy riêng và không nằm trong Compose của backend.

## Mục lục

- [Chạy nhanh](#chạy-nhanh)
- [Backend](backend/README.md)
  - [Cài đặt](backend/README.md#cài-đặt)
  - [Biến môi trường](backend/README.md#biến-môi-trường)
  - [Chỉ SQL Server](backend/README.md#chỉ-sql-server)
  - [API trên máy](backend/README.md#api-trên-máy)
  - [API và SQL Server trong Docker](backend/README.md#api-và-sql-server-trong-docker)
  - [Dừng](backend/README.md#dừng)
  - [Nguyên lý hoạt động](backend/README.md#nguyên-lý-hoạt-động)
  - [Luồng một request](backend/README.md#luồng-một-request)
  - [API](backend/README.md#api)
  - [Thư mục và file](backend/README.md#thư-mục-và-file)
- [Frontend](frontend/README.md)
  - [Cài đặt](frontend/README.md#cài-đặt)
  - [Biến môi trường](frontend/README.md#biến-môi-trường)
  - [Chạy trên máy](frontend/README.md#chạy-trên-máy)
  - [Chạy bằng Docker](frontend/README.md#chạy-bằng-docker)
  - [Luồng gọi API](frontend/README.md#luồng-gọi-api)
  - [Thư mục và file](frontend/README.md#thư-mục-và-file)

## Chạy nhanh

Mỗi phần có file `.env` riêng. Lần đầu, copy từ mẫu.

```bash
cd backend
cp .env.example .env
docker compose up -d
dotnet ef database update -p src/Warehouse.Infrastructure -s src/Warehouse.Api
dotnet run --project src/Warehouse.Api --launch-profile http
```

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

Giao diện: [http://localhost:3000](http://localhost:3000)  
API: [http://localhost:5161](http://localhost:5161)

API và giao diện trong Docker, mỗi thư mục một Compose:

```bash
cd backend
docker compose --profile api up -d --build
```

```bash
cd frontend
docker compose up -d --build
```

Chi tiết cổng, migration, biến môi trường và lý do từng lệnh nằm trong README của [backend](backend/README.md) và [frontend](frontend/README.md).
