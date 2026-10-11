# Backend

API ASP.NET Core trên .NET 10 và SQL Server. Vai trò và người dùng nằm trong database `WarehouseDb`.

## Cài đặt

Cài hai phần mềm:

1. [Docker Desktop](https://www.docker.com/products/docker-desktop/) và bật Docker trước khi chạy Compose.
2. [.NET SDK 10](https://dotnet.microsoft.com/download) nếu muốn chạy API trên máy, hoặc tạo migration mới.

Kiểm tra:

```bash
docker info
dotnet --version
```

Công cụ migration chỉ cần khi chạy API trên máy, không cần khi API chạy trong container:

```bash
dotnet tool install --global dotnet-ef
```

Tạo file môi trường từ mẫu. Compose và `dotnet run` đều đọc `backend/.env`.

```bash
cd backend
cp .env.example .env
```

Mật khẩu SQL Server mặc định là `Warehouse@Dev123`, user `sa`, database `WarehouseDb`. Máy Apple Silicon chạy image SQL Server qua lớp giả lập `linux/amd64`, nên lần đầu tải image và khởi động chậm.

## Biến môi trường

| Biến | Việc |
| --- | --- |
| `MSSQL_SA_PASSWORD` | Mật khẩu user `sa`. Compose đưa vào container SQL Server. |
| `MSSQL_PORT` | Cổng SQL Server trên máy. Mặc định `1433`. |
| `API_HTTP_PORT` | Cổng API trên máy khi chạy trong Docker. Mặc định `5161`. |
| `MSSQL_DATABASE` | Tên database. Mặc định `WarehouseDb`. |
| `ConnectionStrings__DefaultConnection` | Chuỗi kết nối khi API chạy bằng `dotnet run`. Trỏ `localhost`. |
| `Database__ApplyMigrations` | `true` thì API tự tạo bảng lúc khởi động. Trên máy để `false` và chạy `database update`. |
| `Cors__Origins` | Origin được gọi API trực tiếp từ trình duyệt. Nhiều origin cách nhau bằng dấu phẩy. |

Trong container API, Compose ghi đè chuỗi kết nối: host là `sqlserver`, không phải `localhost`, và bật `Database__ApplyMigrations=true`.

## Chỉ SQL Server

Dùng khi API chạy bằng `dotnet run` trên máy. Compose không dựng API.

```bash
cd backend
docker compose up -d
```

`docker compose up -d` chỉ bật SQL Server trống, chưa có database `WarehouseDb` hay các bảng.

## API trên máy

Lệnh `database update` đọc migration trong `src/Warehouse.Infrastructure/Persistence/Migrations` và tạo các bảng. Bỏ qua bước này thì API vẫn chạy, nhưng gọi `/api/roles` hoặc `/api/users` sẽ lỗi vì bảng chưa tồn tại.

Đứng ở `backend` thì giữ nguyên `src/Warehouse.Api`. Chỉ bỏ đường dẫn đó khi đã `cd` vào đúng thư mục project.

```bash
cd backend
dotnet ef database update -p src/Warehouse.Infrastructure -s src/Warehouse.Api
dotnet run --project src/Warehouse.Api --launch-profile http
```

API: [http://localhost:5161/api/roles](http://localhost:5161/api/roles)  
OpenAPI: [http://localhost:5161/openapi/v1.json](http://localhost:5161/openapi/v1.json)  
HTTPS thêm: [https://localhost:7110](https://localhost:7110)

`dotnet run` nạp `.env` ở thư mục `backend` trước `appsettings.json`. Biến đã export trong shell không bị file này ghi đè.

## API và SQL Server trong Docker

Profile `api` mới dựng container backend. Service `api` chờ SQL Server khỏe rồi mới chạy, và tự áp migration.

```bash
cd backend
docker compose --profile api up -d --build
```

API vẫn là [http://localhost:5161/api/roles](http://localhost:5161/api/roles).

Xem log API:

```bash
cd backend
docker compose logs -f api
```

## Dừng

```bash
cd backend
docker compose --profile api down
```

Lệnh này dừng cả API và SQL Server, giữ volume dữ liệu. Thêm `-v` thì xóa database đã lưu.

## Nguyên lý hoạt động

Backend tách hai process:

| Thành phần | Chạy bằng | Việc |
| --- | --- | --- |
| `sqlserver` | `docker compose up -d` | SQL Server 2025, cổng lấy từ `.env`, dữ liệu trong volume `warehouse-sqlserver`. |
| `api` | profile `api` | Image build từ `Dockerfile`, lắng nghe cổng 8080 trong container, map ra `API_HTTP_PORT` trên máy. |

`Dockerfile` build hai giai đoạn. Giai đoạn `sdk` restore và publish `Warehouse.Api`. Giai đoạn `aspnet` chỉ chứa bản đã publish, không chứa SDK. File `.env` không được copy vào image.

Code theo Clean Architecture. Phụ thuộc đi từ ngoài vào trong.

```text
HTTP :5161
    |
    v
Warehouse.Api              nhận request, nạp .env, trả HTTP
    |
    +--> Warehouse.Application     hợp đồng, DTO, phân trang
    |
    +--> Warehouse.Infrastructure  EF Core, SQL Server
              |
              +--> Warehouse.Application
              |
              +--> Warehouse.Domain       Role, User
```

`Program.cs` là chỗ ghép các lớp. Controller chỉ gọi `IRoleService` hoặc `IUserService`. Service trong Infrastructure mới mở kết nối SQL Server. Domain không biết HTTP và database.

Khi container API khởi động:

1. Kestrel nghe `http://+:8080`.
2. Nếu `Database:ApplyMigrations` là true, EF áp các migration trong `Persistence/Migrations` vào `WarehouseDb`.
3. Ứng dụng map controller và bắt đầu nhận request.

## Luồng một request

Ví dụ `GET /api/roles?search=quan ly&page=1&pageSize=10`. `GET /api/users` đi cùng các bước, khác service và các cột được tìm.

1. Request vào cổng 5161. Nếu API chạy trên máy, Kestrel nhận trực tiếp. Nếu API chạy trong Docker, Docker chuyển cổng 5161 sang 8080 của container.
2. ASP.NET tạo controller và tiêm service tương ứng.
3. `Search` đọc `search`, `page`, `pageSize` rồi gọi `SearchAsync`.
4. `PageRequest.Normalize` giữ trang 1 và 10 bản ghi. `page` nhỏ hơn 1 được đưa về 1. `pageSize` nhỏ hơn 1 được đưa về 10. `pageSize` lớn hơn 100 bị cắt còn 100.
5. `TextSearch.ToContainsPattern` đổi `quan ly` thành `%quan ly%`, và thoát `%`, `_`, `[` để người dùng không điều khiển ký tự đại diện của `LIKE`.
6. EF Core dịch thành SQL. `LIKE` dùng collation `Latin1_General_100_CI_AI`, nên không phân biệt hoa thường và dấu. `quan ly` khớp `Quản lý kho`. Từ khóa trống thì không lọc.
7. Service đếm tổng số dòng, sắp xếp theo tên, rồi `Skip` và `Take` theo trang.
8. Controller trả `200` với `items`, `page`, `pageSize`, `totalCount`, `totalPages`.

`POST /api/roles` đi tới `RoleService.CreateAsync`:

1. Cắt khoảng trắng. Mô tả chỉ gồm khoảng trắng được lưu `null`.
2. Tên rỗng hoặc dài hơn 150 ký tự, hoặc mô tả dài hơn 500 ký tự, thì ném `AppException` mã 400.
3. Tên đã có trong bảng `Roles` thì ném `AppException` mã 409.
4. Hợp lệ thì tạo `Role` với `Guid` mới và `SaveChanges`.
5. Controller trả `201` và header `Location` trỏ tới `GET /api/roles/{id}`.

`POST /api/users` kiểm tra họ tên, email, số điện thoại và vai trò được gán. Email trùng trả 409. Vai trò không tồn tại trả 400. Xóa vai trò thì `RoleId` của người dùng về `null`, người dùng không bị xóa.

Controller bắt `AppException` và trả `{ "message": "..." }` với mã trong exception. Không thấy bản ghi khi xem, sửa hoặc xóa thì trả `404`. Xóa xong trả `204`.

## API

| Method | Đường dẫn | Kết quả |
| --- | --- | --- |
| `GET` | `/api/roles?search=&page=1&pageSize=10` | Danh sách vai trò đã lọc và phân trang. |
| `GET` | `/api/roles/{id}` | Một vai trò, hoặc 404. |
| `POST` | `/api/roles` | Tạo vai trò. Body: `name`, `description`. |
| `PUT` | `/api/roles/{id}` | Sửa vai trò. Body giống lúc tạo. |
| `DELETE` | `/api/roles/{id}` | Xóa vai trò. |
| `GET` | `/api/users?search=&page=1&pageSize=10` | Danh sách người dùng. Tìm theo tên, email, số điện thoại, tên vai trò. |
| `GET` | `/api/users/{id}` | Một người dùng, hoặc 404. |
| `POST` | `/api/users` | Tạo người dùng. Body: `fullName`, `email`, `phone`, `roleId`, `isActive`. |
| `PUT` | `/api/users/{id}` | Sửa người dùng. Body giống lúc tạo. |
| `DELETE` | `/api/users/{id}` | Xóa người dùng. |

```bash
curl -X POST http://localhost:5161/api/roles \
  -H 'Content-Type: application/json' \
  -d '{"name":"Quản lý kho","description":"Điều hành kho"}'
```

## Thư mục và file

| File | Nhiệm vụ |
| --- | --- |
| `Warehouse.slnx` | Solution của bốn project trong `src`. |
| `Dockerfile` | Build và chạy API trong image `aspnet:10.0`. |
| `.dockerignore` | Không đưa `bin`, `obj`, `.env` vào ngữ cảnh build. |
| `.env.example` | Mẫu biến môi trường. |
| `.env` | Giá trị thật trên máy. Không đưa vào Git. |
| `docker-compose.yml` | Service `sqlserver` luôn có. Service `api` chỉ chạy khi bật profile `api`. Đọc biến từ `.env`. |
| `src/Warehouse.Domain/Roles/Role.cs` | Thực thể vai trò: `Id`, `Name`, `Description`. |
| `src/Warehouse.Domain/Users/User.cs` | Thực thể người dùng, khóa ngoại tùy chọn tới `Role`. |
| `src/Warehouse.Application/Common/` | `AppException`, `PagedResult`, `PageRequest`. |
| `src/Warehouse.Application/Roles/RoleContracts.cs` | DTO và `IRoleService`. |
| `src/Warehouse.Application/Users/UserContracts.cs` | DTO và `IUserService`. |
| `src/Warehouse.Infrastructure/DependencyInjection.cs` | Đăng ký DbContext, `RoleService`, `UserService`. |
| `src/Warehouse.Infrastructure/Persistence/WarehouseDbContext.cs` | `DbSet` vai trò và người dùng. |
| `src/Warehouse.Infrastructure/Persistence/TextSearch.cs` | Mẫu `LIKE` cho tìm gần đúng. |
| `src/Warehouse.Infrastructure/Persistence/Configurations/` | Bảng, độ dài cột, index, khóa ngoại. |
| `src/Warehouse.Infrastructure/Persistence/Migrations/` | Lịch sử schema. `AddRoles` tạo `Roles`. `AddUsers` tạo `Users`. |
| `src/Warehouse.Infrastructure/Services/RoleService.cs` | Nghiệp vụ vai trò. |
| `src/Warehouse.Infrastructure/Services/UserService.cs` | Nghiệp vụ người dùng. |
| `src/Warehouse.Api/Program.cs` | Dựng host, CORS, áp migration khi được bật, map controller. |
| `src/Warehouse.Api/EnvFile.cs` | Đọc `.env` khi chạy API trên máy. |
| `src/Warehouse.Api/Controllers/RolesController.cs` | Endpoint `/api/roles`. |
| `src/Warehouse.Api/Controllers/UsersController.cs` | Endpoint `/api/users`. |
| `src/Warehouse.Api/appsettings.json` | Log và host. Chuỗi kết nối lấy từ `.env`. |
| `src/Warehouse.Api/Properties/launchSettings.json` | Cổng 5161 và 7110 cho `dotnet run`. |
