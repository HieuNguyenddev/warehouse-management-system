# warehouse-management-system

Hệ thống quản lý kho. Backend là ASP.NET Core Web API trên .NET 10, lưu vai trò trong SQL Server. Frontend để trống, chạy riêng và không nằm trong Compose của backend.

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

Mật khẩu SQL Server là `Warehouse@Dev123`, user `sa`, database `WarehouseDb`. Máy Apple Silicon chạy image SQL Server qua lớp giả lập `linux/amd64`, nên lần đầu tải image và khởi động chậm.

## Chạy riêng backend

Mọi lệnh Compose chạy trong thư mục `backend`.

### Chỉ SQL Server

Dùng khi API chạy bằng `dotnet run` trên máy. Compose không dựng API.

```bash
cd backend
docker compose up -d
```

`docker compose up -d` chỉ bật SQL Server trống, chưa có database `WarehouseDb` hay bảng `Roles`. Lệnh `database update` đọc migration trong `backend/src/Warehouse.Infrastructure/Persistence/Migrations` và tạo các bảng đó. Bỏ qua bước này thì API vẫn chạy, nhưng gọi `/api/roles` sẽ lỗi vì bảng chưa tồn tại.

Đứng ở `backend` thì giữ nguyên `src/Warehouse.Api`. Chỉ bỏ đường dẫn đó khi đã `cd` vào đúng thư mục project.

```bash
cd backend
dotnet ef database update -p src/Warehouse.Infrastructure -s src/Warehouse.Api
dotnet run --project src/Warehouse.Api --launch-profile http
```

API: [http://localhost:5161/api/roles](http://localhost:5161/api/roles)  
OpenAPI: [http://localhost:5161/openapi/v1.json](http://localhost:5161/openapi/v1.json)  
HTTPS thêm: [https://localhost:7110](https://localhost:7110)

Chuỗi kết nối trên máy nằm trong `backend/src/Warehouse.Api/appsettings.json` và trỏ `localhost,1433`.

### Cả API và SQL Server trong Docker

Profile `api` mới dựng container backend. Service `api` chờ SQL Server khỏe rồi mới chạy, và tự áp migration.

```bash
cd backend
docker compose --profile api up -d --build
```

API vẫn là [http://localhost:5161/api/roles](http://localhost:5161/api/roles). Trong mạng Docker, API nối tới host `sqlserver`, không phải `localhost`. Biến `ConnectionStrings__DefaultConnection` trong Compose ghi đè `appsettings.json`. Biến `Database__ApplyMigrations=true` bảo `Program.cs` gọi `Database.Migrate()` lúc khởi động.

Xem log API:

```bash
cd backend
docker compose logs -f api
```



### Dừng

```bash
cd backend
docker compose --profile api down
```

Lệnh này dừng cả API và SQL Server, giữ volume dữ liệu. Thêm `-v` thì xóa database đã lưu.

## Nguyên lý hoạt động

Backend tách hai process:


| Thành phần  | Chạy bằng              | Việc                                                                                            |
| ----------- | ---------------------- | ----------------------------------------------------------------------------------------------- |
| `sqlserver` | `docker compose up -d` | SQL Server 2025, cổng 1433, dữ liệu trong volume `warehouse-sqlserver`.                         |
| `api`       | profile `api`          | Image build từ `backend/Dockerfile`, lắng nghe cổng 8080 trong container, map ra 5161 trên máy. |


`Dockerfile` build hai giai đoạn. Giai đoạn `sdk` restore và publish `Warehouse.Api`. Giai đoạn `aspnet` chỉ chứa bản đã publish, không chứa SDK.

Code theo Clean Architecture. Phụ thuộc đi từ ngoài vào trong.

```text
HTTP :5161
    |
    v
Warehouse.Api              nhận request, trả HTTP
    |
    +--> Warehouse.Application     hợp đồng, DTO, phân trang
    |
    +--> Warehouse.Infrastructure  EF Core, SQL Server
              |
              +--> Warehouse.Application
              |
              +--> Warehouse.Domain       thực thể Role
```

`Program.cs` là chỗ ghép các lớp. Controller chỉ gọi `IRoleService`. `RoleService` trong Infrastructure mới mở kết nối SQL Server. Domain không biết HTTP và database.

Khi container API khởi động:

1. Kestrel nghe `http://+:8080`.
2. Nếu `Database:ApplyMigrations` là true, EF áp các migration trong `Persistence/Migrations` vào `WarehouseDb`.
3. Ứng dụng map controller và bắt đầu nhận request.



## Luồng một request

Ví dụ `GET /api/roles?search=quan ly&page=1&pageSize=10`.

1. Request vào cổng 5161. Nếu API chạy trên máy, Kestrel nhận trực tiếp. Nếu API chạy trong Docker, Docker chuyển cổng 5161 sang 8080 của container.
2. ASP.NET tạo `RolesController` và tiêm `RoleService`.
3. `Search` đọc `search`, `page`, `pageSize` rồi gọi `IRoleService.SearchAsync`.
4. `PageRequest.Normalize` giữ trang 1 và 10 bản ghi. `page` nhỏ hơn 1 được đưa về 1. `pageSize` nhỏ hơn 1 được đưa về 10. `pageSize` lớn hơn 100 bị cắt còn 100.
5. `TextSearch.ToContainsPattern` đổi `quan ly` thành `%quan ly%`, và thoát `%`, `_`, `[` để người dùng không điều khiển ký tự đại diện của `LIKE`.
6. EF Core dịch thành SQL. `LIKE` dùng collation `Latin1_General_100_CI_AI`, nên không phân biệt hoa thường và dấu. `quan ly` khớp `Quản lý kho`. Từ khóa trống thì không lọc.
7. Service đếm tổng số dòng, sắp xếp theo `Name`, rồi `Skip` và `Take` theo trang.
8. Controller trả `200` với `items`, `page`, `pageSize`, `totalCount`, `totalPages`.

`POST /api/roles` đi tới `RoleService.CreateAsync`:

1. Cắt khoảng trắng. Mô tả chỉ gồm khoảng trắng được lưu `null`.
2. Tên rỗng hoặc dài hơn 150 ký tự, hoặc mô tả dài hơn 500 ký tự, thì ném `AppException` mã 400.
3. Tên đã có trong bảng `Roles` thì ném `AppException` mã 409.
4. Hợp lệ thì tạo `Role` với `Guid` mới và `SaveChanges`.
5. Controller trả `201` và header `Location` trỏ tới `GET /api/roles/{id}`.

`RolesController.Execute` bắt `AppException` và trả `{ "message": "..." }` với mã trong exception. Không thấy bản ghi khi xem, sửa hoặc xóa thì trả `404`. Xóa xong trả `204`.

`PUT /api/roles/{id}` sửa tên và mô tả. Kiểm tra trùng tên bỏ qua chính vai trò đang sửa.

## API vai trò


| Method   | Đường dẫn                               | Kết quả                                   |
| -------- | --------------------------------------- | ----------------------------------------- |
| `GET`    | `/api/roles?search=&page=1&pageSize=10` | Danh sách đã lọc và phân trang.           |
| `GET`    | `/api/roles/{id}`                       | Một vai trò, hoặc 404.                    |
| `POST`   | `/api/roles`                            | Tạo vai trò. Body: `name`, `description`. |
| `PUT`    | `/api/roles/{id}`                       | Sửa vai trò. Body giống lúc tạo.          |
| `DELETE` | `/api/roles/{id}`                       | Xóa vai trò.                              |


```bash
curl -X POST http://localhost:5161/api/roles \
  -H 'Content-Type: application/json' \
  -d '{"name":"Quản lý kho","description":"Điều hành kho"}'
```



## Thư mục và file



### Gốc kho chứa


| File         | Nhiệm vụ                                                                       |
| ------------ | ------------------------------------------------------------------------------ |
| `README.md`  | Cài đặt, cách chạy riêng, nguyên lý và luồng request.                          |
| `.gitignore` | Loại `bin`, `obj` và file tạm của .NET khỏi Git.                               |
| `frontend/`  | Ứng dụng giao diện, hiện chưa có mã. Không được Compose của backend khởi động. |




### `backend`


| File                 | Nhiệm vụ                                                                   |
| -------------------- | -------------------------------------------------------------------------- |
| `Warehouse.slnx`     | Solution của bốn project trong `backend/src`.                              |
| `Dockerfile`         | Build và chạy API trong image `aspnet:10.0`.                               |
| `.dockerignore`      | Không đưa `bin`, `obj` vào ngữ cảnh build.                                 |
| `docker-compose.yml` | Service `sqlserver` luôn có. Service `api` chỉ chạy khi bật profile `api`. |




### `backend/src/Warehouse.Domain`


| File                      | Nhiệm vụ                                             |
| ------------------------- | ---------------------------------------------------- |
| `Warehouse.Domain.csproj` | Class library .NET 10, không phụ thuộc project khác. |
| `AssemblyReference.cs`    | Giữ `Assembly` của Domain để lớp khác quét khi cần.  |
| `Roles/Role.cs`           | Thực thể vai trò: `Id`, `Name`, `Description`.       |




### `backend/src/Warehouse.Application`


| File                           | Nhiệm vụ                                                                     |
| ------------------------------ | ---------------------------------------------------------------------------- |
| `Warehouse.Application.csproj` | Class library, chỉ tham chiếu Domain.                                        |
| `AssemblyReference.cs`         | Giữ `Assembly` của Application.                                              |
| `Common/AppException.cs`       | Lỗi nghiệp vụ kèm mã HTTP, ví dụ 400 hoặc 409.                               |
| `Common/PagedResult.cs`        | Kết quả phân trang: `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages`. |
| `Common/PageRequest.cs`        | Chuẩn hóa số trang và kích thước trang.                                      |
| `Roles/RoleContracts.cs`       | `RoleResponse`, `UpsertRoleRequest` và interface `IRoleService`.             |




### `backend/src/Warehouse.Infrastructure`


| File                                                         | Nhiệm vụ                                                           |
| ------------------------------------------------------------ | ------------------------------------------------------------------ |
| `Warehouse.Infrastructure.csproj`                            | Tham chiếu Application và Domain. Gói EF Core SQL Server.          |
| `DependencyInjection.cs`                                     | `AddInfrastructure` đăng ký `WarehouseDbContext` và `RoleService`. |
| `Persistence/WarehouseDbContext.cs`                          | DbContext, `DbSet<Role>`, nạp cấu hình trong assembly.             |
| `Persistence/TextSearch.cs`                                  | Tạo mẫu `LIKE` cho tìm gần đúng.                                   |
| `Persistence/Configurations/RoleConfiguration.cs`            | Bảng `Roles`, độ dài cột, index duy nhất trên `Name`.              |
| `Persistence/Migrations/20261011170323_AddRoles.cs`          | Migration tạo bảng `Roles`.                                        |
| `Persistence/Migrations/20261011170323_AddRoles.Designer.cs` | Siêu dữ liệu EF của migration đó.                                  |
| `Persistence/Migrations/WarehouseDbContextModelSnapshot.cs`  | Ảnh model hiện tại để EF so sánh khi thêm migration.               |
| `Services/RoleService.cs`                                    | Kiểm tra dữ liệu, tìm kiếm, phân trang và ghi SQL Server.          |




### `backend/src/Warehouse.Api`


| File                             | Nhiệm vụ                                                                        |
| -------------------------------- | ------------------------------------------------------------------------------- |
| `Warehouse.Api.csproj`           | Web API. Tham chiếu Application và Infrastructure. Có gói OpenAPI và EF Design. |
| `Program.cs`                     | Dựng host, đăng ký dịch vụ, áp migration khi được bật, map controller.          |
| `Controllers/RolesController.cs` | Endpoint `/api/roles`.                                                          |
| `appsettings.json`               | Chuỗi kết nối `localhost` khi chạy API trên máy.                                |
| `appsettings.Development.json`   | Mức log khi môi trường là Development.                                          |
| `Properties/launchSettings.json` | Cổng 5161 và 7110 cho `dotnet run`.                                             |
| `Warehouse.Api.http`             | Mẫu request để gọi API từ IDE.                                                  |


