# NoelleNet.AspNetCore

ASP.NET Core 增强：全局异常处理（ProblemDetails）、模型验证过滤器、kebab-case 路由、当前用户的 HTTP 实现 ✧

```bash
dotnet add package NoelleNet.AspNetCore
```

依赖 FluentValidation 12.1.1 与 [NoelleNet.Core](core.md)。命名空间：`.ExceptionHandling`、`.ExceptionHandling.Localization`、`.Validation`、`.Validation.Localization`、`.Routing`、`.Mvc`、`.Security.Claims`。

⚠️ 本包**没有任何 DI 扩展方法**（没有 `AddNoelleXxx()` / `UseNoelleXxx()`），全部靠手工注册，照着下面的代码块抄即可。

---

## 全局异常处理

三种接入方式，**选一种**即可，它们共用同一个响应写入器：

| 方式 | 扩展点 | 覆盖范围 |
|------|--------|----------|
| `NoelleExceptionHandler` | `IExceptionHandler`（.NET 8+ 推荐） | `UseExceptionHandler()` 之后的**整条管道** |
| `NoelleExceptionHandlingFilter` | `IAsyncExceptionFilter` | **仅 MVC 管道内** |
| `NoelleExceptionHandlingMiddleware` | 普通中间件 | `UseMiddleware` 之后的整条管道，位置自控 |

### 注册（推荐方式）

```csharp
builder.Services.AddLocalization();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();
builder.Services.AddExceptionHandler<NoelleExceptionHandler>();

// 管道
app.UseExceptionHandler();
```

⚠️ `AddProblemDetails()` 是**必需**的：写入器内部通过 `RequestServices.GetRequiredService<IProblemDetailsService>()` 取服务，没有它就拿不到序列化入口。

调试信息默认不返回，需要时按环境打开：

```csharp
builder.Services.Configure<NoelleExceptionHandlingOptions>(options =>
{
    options.IncludeExceptionDetails = env.IsDevelopment();   // 响应里带异常类型与消息
    options.IncludeStackTrace = env.IsDevelopment();         // 再带上堆栈（需同时开上面一项）
    options.IncludeExceptionData = env.IsDevelopment();      // 响应里带 Exception.Data
});
```

三个开关都只影响**响应体的调试字段**，不影响状态码。

另外两种方式的注册：

```csharp
// MVC 过滤器
builder.Services.AddControllers(options => options.Filters.Add<NoelleExceptionHandlingFilter>());

// 中间件
app.UseMiddleware<NoelleExceptionHandlingMiddleware>();
```

三者的失败语义不同：`NoelleExceptionHandler` 写入失败时 `catch { return false; }` 不抛；过滤器和中间件是 `try/finally` **没有 catch，异常向上抛**。

### 异常 → HTTP 状态码

按顺序匹配，先命中先返回：

| 顺序 | 条件 | 状态码 |
|------|------|--------|
| 1 | 异常实现 `IHasHttpStatusCode` 且 `StatusCode > 0` | 该状态码 |
| 2 | `IBusinessException` | 400 |
| 3 | `ValidationException`（DataAnnotations）或 `IHasValidationResults` | 400 |
| 4 | `EntityNotFoundException` | 404 |
| 5 | `DBConcurrencyException` | 409 |
| 6 | `NotImplementedException` | 501 |
| 7 | 其它 | 500 |

⚠️ **没有配置开关**能改这套映射。要定制只有三条路：让异常自己实现 `IHasHttpStatusCode`；继承 `ProblemDetailsErrorResponseWriter` 重写它的 `protected virtual` 方法（`GetStatusCode`、`CreateProblemDetails`、`GetDefaultErrorMessage`、`GetProblemDetailsType` 等）；或整个替换 `IErrorResponseWriter`。

`BusinessException` **不实现** `IHasHttpStatusCode`，所以固定走 400。

### 响应体

标准 RFC 9457 ProblemDetails：

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
  "title": "请求的资源未找到。",
  "status": 404,
  "detail": "未找到指定标识符的实体。实体类型：TodoApp.TodoItem，标识符：1",
  "traceId": "..."
}
```

- `type` 由状态码决定（`NoelleProblemDetailsTypes` 里的常量，可改），400/401/403/404/409/500/501 之外一律是 `about:blank`；
- `status` 也由状态码决定，异常自己的设置改不了它；
- 异常实现 `IHasErrorCode` 且错误码非空时，追加扩展字段 `code`；
- 开了 `IncludeExceptionData` 且有数据时追加 `data`（值做了安全化处理，基本类型原样，其余 `ToString()`）；
- 开了 `IncludeExceptionDetails` 时追加 `exception`（`type`、`message`，可选 `stackTrace`）；
- 验证失败时追加 `errors`（`字段名 → 错误消息数组`，取不到字段名时用空字符串作键）。

`AddProblemDetails(o => o.CustomizeProblemDetails = ...)` 依然生效，可以借此补 `instance` 等字段。

### 错误码本地化

```csharp
builder.Services.Configure<NoelleExceptionLocalizationOptions>(options =>
{
    options.LocalizerProvider = (exception, factory) => factory.Create(typeof(AppResource));
});
```

解析过程：异常实现 `IHasErrorCode` 且 `ErrorCode` 非空 → 用 `LocalizerProvider` 指定的资源查找该错误码 → 命中后把消息里的 `{key}` 占位符用 `Exception.Data` 里的值替换。

```csharp
throw new BusinessException("A01124").WithData(nameof(parentId), parentId);
```

`WithData` 来自 [NoelleNet.Core](core.md)。

⚠️ `NoelleExceptionLocalizationOptions.ResourceSources` 属性**已标记 `[Obsolete]` 且写入器从不读取它**——只有 `LocalizerProvider` 生效。内置资源（`NoelleExceptionHandlingResource`）只有 `en` 与 `zh-CN`，没有中性资源。

---

## 模型验证

两个 action filter，**底层机制不同，按项目选其一**：

| 过滤器 | 机制 |
|--------|------|
| `NoelleModelValidationFilter` | 读 ASP.NET Core 的 `ModelState`（DataAnnotations 绑定期验证） |
| `NoelleFluentValidationFilter` | 按参数类型解析 `IValidator<T>`（FluentValidation） |

两者都抛 `NoelleValidationException`，产出统一的 ProblemDetails 错误格式。

```csharp
builder.Services.AddControllers(options =>
{
    // 二选一
    options.Filters.Add<NoelleFluentValidationFilter>();
    // options.Filters.Add<NoelleModelValidationFilter>();
})
.ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

// 用 FluentValidation 时还必须注册验证器，否则过滤器什么也不做
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoItemValidator>();
```

⚠️ `SuppressModelStateInvalidFilter = true` 不能少：它关掉 `[ApiController]` 内置的验证短路。用 `NoelleModelValidationFilter` 时**必须**设置，否则绑定期就返回内置格式的错误、过滤器根本不会执行；只用 FluentValidation 时也建议设置，否则绑定错误仍是内置格式。

`NoelleFluentValidationFilter` 的行为细节：

- 按参数类型解析 `IValidator<T>`，**支持同一类型注册多个验证器**（用 `GetServices`），一个都没有就跳过；
- 跳过 `CancellationToken` 参数，以及绑定源为 `Services` / `Special` 的参数；
- 参数值为 `null` 且未允许空请求体时，产出「参数不能为空」的验证错误；
- 错误消息支持本地化（构造注入了 `IStringLocalizer<NoelleValidationResource>`）。

```csharp
public class CreateTodoItemValidator : AbstractValidator<CreateTodoItemDto>
{
    public CreateTodoItemValidator(IStringLocalizer<AppResource> localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(localizer["TodoItem.Title.NullErrorMessage"])
            .MaximumLength(64);
    }
}
```

---

## kebab-case 路由

```csharp
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new NoelleRouteKebabCaseTransformer()));
});
```

`TodoItems` → `todo-items`，`TodoItemDetails` → `todo-item-details`，`UserID` → `user-id`（连续大写会正确切分），已经是下划线的 `todo_items` 不变。

---

## 自动状态码

`NoelleActionResultStatusCodeFilter` 是个 `IAsyncResultFilter`，按 HTTP 方法把「裸返回」补成合适的状态码：

| 方法 | 行为 |
|------|------|
| POST | 空结果 → 204；`ObjectResult` 未设状态码 → 201 + 返回体 |
| PUT / PATCH / DELETE | 空结果 → 204；`ObjectResult` 未设状态码 → 200 + 返回体 |
| GET | 空结果 → 204 |
| CONNECT / HEAD / OPTIONS / TRACE | 不处理 |

```csharp
builder.Services.AddControllers(options => options.Filters.Add<NoelleActionResultStatusCodeFilter>());
```

---

## 当前用户（HTTP 实现）

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
```

`NoelleHttpContextCurrentPrincipalProvider` 只做一件事：把 `HttpContext.User` 作为 `Principal` 交出去（**没有 HTTP 上下文时返回 `null`**）。声明名到用户属性的映射全在 [NoelleNet.Core](core.md) 的 `CurrentUser` + `NoelleClaimTypes` 里，本包不碰那些声明名。

```csharp
public class TodoAppService(ICurrentUser currentUser, ITodoItemRepository repository)
{
    public async Task<TodoItem> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await repository.FindByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException<TodoItem>(id);

        // 不向非超管暴露他人数据的存在性
        if (!currentUser.IsInRole("admin") && item.CreatedBy != currentUser.UserId)
            throw new EntityNotFoundException<TodoItem>(id);

        return item;
    }
}
```

---

## 相关包

- [NoelleNet.Core](core.md) — `BusinessException`、`EntityNotFoundException`、`ICurrentUser`、`WithData`
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — 审计拦截器需要 `ICurrentUser`
- [NoelleNet.Application.Contracts](application-contracts.md) — 请求 / 响应 DTO
