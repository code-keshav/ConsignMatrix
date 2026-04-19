using Acl.Entities;
using Microsoft.EntityFrameworkCore;

namespace Acl.Configuration;

public static class EntityRegisterer
{
    public static ModelBuilder AddAcl(this ModelBuilder builder)
    {
        builder.Entity<RolePermission>();
        return builder;
    }
}