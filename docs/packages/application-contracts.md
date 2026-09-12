# NoelleNet.Application.Contracts

应用层 DTO：分页 / 排序 / 列表结果，以及实体与审计 DTO 基类。纯 POCO，没有依赖框架的任何运行时行为 ✧

```bash
dotnet add package NoelleNet.Application.Contracts
```

命名空间：`NoelleNet.Application.Dtos`。本包依赖 `NoelleNet.Auditing`（DTO 基类要实现它的审计接口）。

---

## 实体 DTO 基类

| 类型 | 成员 |
|------|------|
| `IEntityDto` | 空标记接口 |
| `IEntityDto<TKey>` | `TKey Id { get; set; }` |
| `EntityDto` | 重写 `ToString()` → `[DTO: 类型名]` |
| `EntityDto<TKey>` | 继承 `EntityDto`，增加 `Id`，`ToString()` 带上 `Id` |
| `CreationAuditedEntityDto` / `<TKey>` | 增加 `CreatedAt`（`DateTime`）、`CreatedBy`（`string?`） |
| `AuditedEntityDto` / `<TKey>` | 在创建审计之上增加 `LastModifiedAt`（`DateTime?`）、`LastModifiedBy`（`string?`） |

非泛型与泛型版本的差别只有「有没有 `Id`」：非泛型继承 `EntityDto`，泛型继承 `EntityDto<TKey>`。两个版本都实现了对应的审计接口（`ICreationAudited` / `IAudited`），所以能直接承接 [NoelleNet.Ddd.Domain](ddd-domain.md) 里的审计实体。

```csharp
public class TodoItemDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
```

---

## 分页与排序（请求侧）

请求 DTO 继承 `PagingDto` 即可同时拿到分页与排序：

```csharp
public class TodoItemPagingDto : PagingDto
{
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}
```

| 类型 | 成员 |
|------|------|
| `IHasLimit` | `int Limit { get; set; }` |
| `IHasSort` | `string? Sort { get; set; }` |
| `IPaging` | `IHasLimit` + `IHasSort` + `int Offset { get; set; }` |
| `LimitDto` | 实现 `IHasLimit`，`Limit` 带钳制 |
| `PagingDto` | 继承 `LimitDto`，实现 `IPaging`，`Offset` 带钳制 |

分页模型是 offset-based：跳过 `Offset` 条后取 `Limit` 条，对应 SQL 的 `LIMIT/OFFSET` 与 OData 的 `$skip/$top`。

### 钳制规则

`Limit` 与 `Offset` 都不是简单属性，赋值时会自动收敛：

| 属性 | 规则 |
|------|------|
| `LimitDto.Limit` | 小于等于 0 → `DefaultLimit`；大于 `MaxLimit` → `MaxLimit`；其余保持原值 |
| `PagingDto.Offset` | 小于 0 → `0` |

```csharp
LimitDto.DefaultLimit = 20;   // 默认 10，必须 > 0 且 <= MaxLimit，否则抛 ArgumentOutOfRangeException
LimitDto.MaxLimit = 200;      // 默认 1000，必须 >= DefaultLimit，否则抛 ArgumentOutOfRangeException

var dto = new PagingDto { Limit = int.MaxValue, Offset = -5 };
dto.Limit;    // 200
dto.Offset;   // 0
```

⚠️ `DefaultLimit` / `MaxLimit` 是**静态**属性，改动会影响整个进程内之后创建的所有实例，建议在启动时设置一次。单元测试里改过记得还原（框架自己的测试就是这么做的）。

两个属性都是 `virtual`，需要端点级规则时在派生类里重写。框架默认「宽容钳制」，派生类可以改成严格校验：

```csharp
public class StrictPagingDto : PagingDto
{
    private int _limit = LimitDto.DefaultLimit;

    public override int Limit
    {
        get => _limit;
        set => _limit = value is <= 0 or > 100
            ? throw new ArgumentOutOfRangeException(nameof(Limit), "每页最多 100 条。")
            : value;
    }

    public override int Offset
    {
        get => base.Offset;
        set => base.Offset = Math.Min(value, 1000);   // 限制最大偏移，防止深分页
    }
}
```

### Sort 只是一个约定字符串

`Sort` 框架不解析、不排序，只作为约定格式传递：

```
"CreatedAt desc"
"CreatedAt desc, Title asc"
```

需要字符串排序时自行解析映射为表达式（或引入 `System.Linq.Dynamic.Core`）。查询实现示例见 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)。

---

## 列表与分页结果（响应侧）

| 类型 | 成员 / 构造 |
|------|-------------|
| `IListResult<T>` | `IReadOnlyList<T> Items { get; set; }` |
| `IHasTotalCount` | `long TotalCount { get; set; }` |
| `IPagedResult<T>` | `IListResult<T>` + `IHasTotalCount` |
| `ListResultDto<T>` | `ListResultDto()` / `ListResultDto(IReadOnlyList<T> items)`，`Items` 默认 `[]` |
| `PagedResultDto<T>` | 继承 `ListResultDto<T>`；`PagedResultDto(long totalCount, IReadOnlyList<T> items)` |

```csharp
var query = dbContext.TodoItems
    .AsNoTracking()
    .WhereIf(!string.IsNullOrWhiteSpace(dto.Title), x => x.Title.Contains(dto.Title!))
    .WhereIf(dto.IsCompleted.HasValue, x => x.IsCompleted == dto.IsCompleted!.Value);

var totalCount = await query.CountAsync(cancellationToken);
var items = await query
    .OrderByDescending(x => x.CreatedAt)          // Sort 字符串需自行解析
    .Skip(dto.Offset)
    .Take(dto.Limit)
    .ToListAsync(cancellationToken);

return new PagedResultDto<TodoItemDto>(totalCount, items);
```

`WhereIf` 来自 [NoelleNet.Core](core.md)。

---

## 已过时的类型

早期命名以 `Pagination` 开头，现已统一为 `Paging`。以下类型标记 `[Obsolete]`，仅用于兼容，新代码不要使用：

| 已过时 | 替换为 |
|--------|--------|
| `IPagination` | `IPaging` |
| `IPaginationAndSort` | `IPaging`（已并入） |
| `PaginationDto` | `PagingDto` |
| `PaginationAndSortDto` | `PagingDto`（已并入） |
| `IPaginationResult<T>` | `IPagedResult<T>` |
| `PaginationResultDto<T>` | `PagedResultDto<T>` |

---

## 相关包

- [NoelleNet.Auditing](auditing.md) — DTO 基类实现的审计接口
- [NoelleNet.Core](core.md) — `WhereIf` 等查询扩展方法
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — 分页查询与仓储实现
