# NoelleNet.Auditing

审计接口定义：谁在什么时候创建、谁在什么时候改过。只有接口，没有实现 ✧

```bash
dotnet add package NoelleNet.Auditing
```

命名空间：`NoelleNet.Auditing`。本包不依赖任何其他 NoelleNet 包，但通常与 [NoelleNet.Ddd.Domain](ddd-domain.md)（实体基类）和 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)（自动填充）一起用。

---

## 接口

| 接口 | 成员 |
|------|------|
| `IHasCreatedAt` | `DateTime CreatedAt { get; }` |
| `IMayHaveCreator` | `string? CreatedBy { get; }` |
| `ICreationAudited` | `IHasCreatedAt` + `IMayHaveCreator` |
| `IModificationAudited` | `DateTime? LastModifiedAt { get; }`、`string? LastModifiedBy { get; }` |
| `IAudited` | `ICreationAudited` + `IModificationAudited` |

组合关系：

```
IAudited
├── ICreationAudited
│   ├── IHasCreatedAt      CreatedAt
│   └── IMayHaveCreator    CreatedBy
└── IModificationAudited   LastModifiedAt / LastModifiedBy
```

注意两点：

- 所有属性**只有 getter**，填充由拦截器完成，实现类把 setter 留给自己；
- 创建时间是 `DateTime`（非空），修改时间与两个「人」都是可空——没改过就是 `null`。

---

## 谁来填充

接口本身什么都不做。注册 `NoelleAuditInterceptor`（[NoelleNet.EntityFrameworkCore](entity-framework-core.md)）后，`SaveChanges` 时自动写入：

| 时机 | 填充字段 | 取值来源 |
|------|----------|----------|
| 新增 | `CreatedAt`、`CreatedBy` | `ICurrentUser.UserId`（[NoelleNet.Core](core.md)） |
| 修改 | `LastModifiedAt`、`LastModifiedBy` | 同上 |

实体侧可以直接用 [NoelleNet.Ddd.Domain](ddd-domain.md) 的 `AuditedEntity<TKey>` / `AuditedAggregateRoot<TKey>`（同时实现 `IAudited`），或只创建不改的场景用 `CreationAuditedEntity<TKey>` / `CreationAuditedAggregateRoot<TKey>`（实现 `ICreationAudited`）。

DTO 侧对应 [NoelleNet.Application.Contracts](application-contracts.md) 的 `CreationAuditedEntityDto` / `AuditedEntityDto`。

---

## 相关包

- [NoelleNet.Ddd.Domain](ddd-domain.md) — 审计实体基类
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — `NoelleAuditInterceptor` 自动填充
- [NoelleNet.Core](core.md) — `ICurrentUser` 提供创建人 / 修改人
- [NoelleNet.Application.Contracts](application-contracts.md) — 审计 DTO 基类
