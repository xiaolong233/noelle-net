﻿using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Auditing;

/// <summary>
/// 实现了 <see cref="IAuditableEntity{TUser}"/> 的实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的类型</typeparam>
/// <typeparam name="TUser">创建和修改实体的用户标识符的类型</typeparam>
public class AuditableEntity<TIdentifier, TUser> : Entity<TIdentifier>, IAuditableEntity<TUser>
{
    public DateTime CreatedAt { get; init; }

    public TUser? CreatedBy { get; init; }

    public DateTime? LastModifiedAt { get; protected set; }

    public TUser? LastModifiedBy { get; protected set; }
}
