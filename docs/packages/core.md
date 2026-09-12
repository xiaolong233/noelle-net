# NoelleNet.Core

基础层：异常类型、当前用户、Guid 生成、缓存与配置扩展、一堆小工具。包内**没有任何 DI 注册扩展方法**，装进来直接用 ✧

```bash
dotnet add package NoelleNet.Core
```

依赖：`Microsoft.Extensions.Caching.Abstractions`、`Configuration`、`Logging.Abstractions`、`Options`。

---

## 异常

### BusinessException

业务异常，携带错误码、详情与日志等级。构造函数 5 个参数**全部可选**：

```csharp
public BusinessException(
    string? errorCode = null,
    string? message = null,
    string? details = null,
    Exception? innerException = null,
    LogLevel logLevel = LogLevel.Warning)
```

```csharp
throw new BusinessException("A01108", "待办事项标题不能为空");
```

实现 `IBusinessException`、`IHasErrorCode`、`IHasErrorDetails`、`IHasLogLevel`。属性都有 setter，可在抛出后改写。⚠️ 它**不实现** `IHasHttpStatusCode`——HTTP 状态码由 [NoelleNet.AspNetCore](aspnetcore.md) 按异常类型映射。

### EntityNotFoundException / `EntityNotFoundException<TEntityType>`

```csharp
throw new EntityNotFoundException(typeof(TodoItem), itemId);
throw new EntityNotFoundException<TodoItem>(itemId);
```

| 构造函数 | `Message` |
|----------|-----------|
| `()` / `(string message)` / `(string, Exception?)` | 用你传入的文案；`EntityType`、`Id` 保持 `null` |
| `(Type entityType)` / `(Type, object? id)` / `(Type, object?, Exception?)` | `id == null` → `未指定实体的标识符。实体类型：{FullName}`；否则 → `未找到指定标识符的实体。实体类型：{FullName}，标识符：{id}` |

泛型版只有 3 个构造函数（`()`、`(object? id)`、`(object? id, Exception? innerException)`），因为构造函数不被继承。属性 `EntityType`、`Id` 可读写，`LogLevel` 默认 `Information`。

⚠️ 它**不实现** `IBusinessException`，也没有错误码。

### 其他异常与能力接口

| 类型 | 说明 |
|------|------|
| `IBusinessException` | 空标记接口 |
| `NoelleValidationException` | 模型验证失败异常，构造需传 `IEnumerable<ValidationResult>`，属性 `ValidationResults`、`LogLevel`（默认 `Information`）。由 [NoelleNet.AspNetCore](aspnetcore.md) 的验证过滤器抛出 |
| `IHasErrorCode` | `string? ErrorCode { get; }` |
| `IHasErrorDetails` | `string? Details { get; set; }` |
| `IHasHttpStatusCode` | `int StatusCode { get; }`，本包**没有实现类**，供业务方自己的异常实现 |
| `IHasLogLevel` | `LogLevel LogLevel { get; }` |
| `IHasValidationResults` | `IEnumerable<ValidationResult> ValidationResults { get; }` |

接口的 getter 都是只读的，包内实现类额外加了 setter（接口实现允许加宽）。

### WithData

`Exception` 的扩展方法，为本地化占位符附加参数：

```csharp
throw new BusinessException("A01124").WithData(nameof(parentId), parentId);
```

```csharp
public static T WithData<T>(this T e, string name, object? value) where T : Exception
```

写入 `Exception.Data`，返回**同一个实例**，可链式调用。`value` 允许为 `null`。

---

## Guid 生成

```csharp
public interface IGuidGenerator { Guid Generate(); }
```

`NoelleGuidGenerator` 是默认实现，返回 `Guid.CreateVersion7()`——**UUID v7，时间有序**，对数据库索引友好，不是 `Guid.NewGuid()`。无状态、线程安全。

```csharp
services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();
```

[NoelleNet.EntityFrameworkCore](entity-framework-core.md) 的 `NoelleAutoSetGuidKeyInterceptor` 也走这个接口。

---

## 当前用户

### ICurrentUser

`NoelleNet.Security` 命名空间，读取声明里的用户信息：

| 成员 | 解析的声明 | 回退 |
|------|-----------|------|
| `ClientId` | `client_id` | 无 |
| `Subject` | `sub` | `ClaimTypes.NameIdentifier` |
| `UserId` | `user_id` | `ClaimTypes.NameIdentifier` |
| `OrganizationUnitId` | `organization_unit_id` | 无 |
| `UserName` | `preferred_username` | `ClaimTypes.Name` |
| `GivenName` | `given_name` | `ClaimTypes.GivenName` |
| `Surname` | `family_name` | `ClaimTypes.Surname` |
| `MiddleName` / `NickName` | `middle_name` / `nickname` | 无 |
| `Email` | `email` | `ClaimTypes.Email` |
| `EmailConfirmed` | `email_verified` == `"true"`（忽略大小写） | 无 |
| `PhoneNumber` | `phone_number` | 无 |
| `PhoneNumberConfirmed` | `phone_number_verified` == `"true"` | 无 |
| `Gender` | `gender` | `ClaimTypes.Gender` |
| `DateOfBirth` | `birthdate` | `ClaimTypes.DateOfBirth` |
| `Roles` | `role` **∪** `ClaimTypes.Role`，去重 | — |
| `Permissions` | `permission` | 无 |

另有 `Claims`、`FindClaim`、`FindClaimValue`、`FindClaims`、`FindClaimValues`、`IsInRole(string)`、`HasPermission(string)`。

两个细节：

- 回退靠 `??`——**短名声明不存在才回退**；若短名声明存在但值是空字符串，返回 `""` 而不回退；
- `IsInRole` / `HasPermission` 是普通 `==` 比较，**区分大小写**，不支持通配。

### NoelleClaimTypes

上面那些声明名来自 `NoelleClaimTypes` 的 **18 个静态可读写属性**，默认值是 OIDC 短名。它们是**进程级全局状态**，`CurrentUser` 每次读取时实时取用，所以启动阶段覆盖即可生效：

```csharp
NoelleClaimTypes.UserId = ClaimTypes.NameIdentifier;   // 纯 Cookie 认证场景
```

### 注册

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
```

`CurrentUser` 只依赖 `ICurrentPrincipalProvider`（`ClaimsPrincipal? Principal { get; }`）。`NoelleHttpContextCurrentPrincipalProvider` 在 [NoelleNet.AspNetCore](aspnetcore.md) 里；脱离 HTTP 场景可用本包的 `NoelleEmptyCurrentPrincipalProvider`——它的 `Principal` 返回**新的空主体而非 `null`**，所以各项解析得到 `null` / `false` / 空数组，不会抛异常。

```csharp
if (!currentUser.IsInRole("admin") && item.CreatedBy != currentUser.UserId)
    throw new EntityNotFoundException<TodoItem>(id);
```

---

## 访问令牌管理

```csharp
public interface ITokenManager
{
    Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default);
    Task ForceRefreshTokenAsync(CancellationToken cancellationToken = default);
}
```

`TokenManagerBase` 是抽象基类，只需实现 `FetchNewTokenAsync`：

```csharp
protected abstract Task<TokenResponse> FetchNewTokenAsync(CancellationToken cancellationToken = default);
```

缓存策略：快速路径（锁外）判断令牌未过期就直接返回；否则进 `SemaphoreSlim(1,1)` 并**二次检查**，避免并发重复取令牌。过期点按

```
UtcNow + (ExpiresIn - ExpirationBuffer)
```

计算——`TokenResponse.ExpiresIn` 单位是**秒**，`TokenManagerOptions.ExpirationBuffer` 默认 **5 分钟**。

⚠️ 若 `ExpirationBuffer` ≥ `ExpiresIn`，过期点会落在过去，缓存永不命中，每次调用都会重新取令牌。`ForceRefreshTokenAsync` 不做双重检查，持锁后无条件刷新且不返回令牌。

```csharp
public record TokenResponse(string AccessToken, int ExpiresIn);
```

---

## 缓存扩展

`IDistributedCache` 的泛型扩展（命名空间就在 `Microsoft.Extensions.Caching.Distributed`，无需额外 using），JSON 序列化用 `JsonSerializer` 默认选项：

```csharp
Task<TValue?> GetAsync<TValue>(this IDistributedCache cache, string key, CancellationToken ct = default)

Task<TValue?> GetOrCreateAsync<TValue>(this IDistributedCache cache, string key,
    Func<DistributedCacheEntryOptions, Task<TValue>> factory, CancellationToken ct = default)

Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value, CancellationToken ct = default)
Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value,
    DistributedCacheEntryOptions options, CancellationToken ct = default)
Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value,
    Action<DistributedCacheEntryOptions> configure, CancellationToken ct = default)
```

`GetOrCreateAsync` 未命中时先 new 一个 `DistributedCacheEntryOptions` 交给 `factory` 配置过期策略，再写入缓存——**方法本身不设默认过期时间**，你要在 factory 里配置：

```csharp
var item = await cache.GetOrCreateAsync("todo:1", async options =>
{
    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
    return await repository.FindByIdAsync(id);
});
```

⚠️ 没有防击穿保护（无锁、无单飞），并发未命中会重复执行 factory。

---

## 配置扩展

```csharp
string GetRequiredValue(this IConfiguration configuration, string key)
string GetRequiredConnectionString(this IConfiguration configuration, string name)
```

取值缺失时抛 `InvalidOperationException`，消息里带上完整路径；当 `IConfiguration` 本身是 `GetSection("App")` 得到的节时，路径形如 `App:MyKey`。

```csharp
var connectionString = configuration.GetRequiredConnectionString("Default");
var groupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
```

⚠️ 只有 `null` 算缺失，**空字符串不算**。

---

## JSON 序列化

| 转换器 | 行为 |
|--------|------|
| `DateTimeConverter` | 默认格式 `yyyy-MM-dd HH:mm:ss`，用 `InvariantCulture` |
| `NullableDateTimeConverter` | 同上，无值时写出 JSON `null` |

⚠️ 反序列化用的是 `TryParse` 且**不抛异常**：解析失败时 `DateTimeConverter` 返回 `DateTime.MinValue`，可空版返回 `null`。也不做时区 / `Kind` 转换（`DateTimeStyles.None`），反序列化得到的 `Kind` 是 `Unspecified`。

---

## LINQ 扩展

```csharp
IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
IQueryable<T>  WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
```

条件为真才过滤，为假时原样返回 `source`。两个重载的谓词类型不同——`IQueryable` 版用 `Expression<Func<T, bool>>`，所以能翻译成 SQL：

```csharp
var query = dbContext.TodoItems
    .WhereIf(!string.IsNullOrWhiteSpace(dto.Title), x => x.Title.Contains(dto.Title!))
    .WhereIf(dto.IsCompleted.HasValue, x => x.IsCompleted == dto.IsCompleted!.Value);
```

---

## 工具

| 类型 | 成员 | 备注 |
|------|------|------|
| `NoelleStringExtensions` | `IsNullOrEmpty()`、`IsNullOrWhiteSpace()` | 转发 `string` 的同名静态方法 |
| `NoelleObjectExtensions` | `To<T>()` | 走 `Convert.ChangeType`，目标类型需实现 `IConvertible`；`Guid` 单独走 `TypeDescriptor`；`source` 为 `null` 且目标非可空类型时抛 `ArgumentNullException` |
| `NoelleObjectHelper` | `TrySetProperty<TObject, TValue>(...)` | ⚠️ **静态方法，不是扩展方法**，须写 `NoelleObjectHelper.TrySetProperty(...)` |
| `NoelleReflectionExtensions` | `GetGenericTypeName()`（`Type` / `object` 两个重载） | `Dictionary<string,int>` → `Dictionary<String,Int32>`；嵌套泛型不递归展开 |
| `IdNumberUtil` | `Validate`、`ConvertTo18DigitIdCard`、`GetBirthday`、`GetGender`、`GetRegionCode` | 静态工具类，18 位身份证校验；`GetGender` 返回 `1` 男 / `2` 女 / `0` 未知 |
| `ProblemDetailsResponse` | RFC 7807 错误响应模型 | 独立类型，不继承 ASP.NET Core 的 `ProblemDetails`；`Extensions` 标了 `[JsonExtensionData]` |

---

## 相关包

- [NoelleNet.AspNetCore](aspnetcore.md) — 把异常转成 ProblemDetails、`NoelleHttpContextCurrentPrincipalProvider`
- [NoelleNet.Auditing](auditing.md) — 审计接口，填充时读 `ICurrentUser.UserId`
- [NoelleNet.Application.Contracts](application-contracts.md) — 查询里常用 `WhereIf`
