using System.Linq.Expressions;

namespace WildcardsUtilities.Scanning;

public static class EntityFrameworkCoreExtensions
{
    public static ModelBuilder Link<TEntity, TRelatedEntity>
    (
        this ModelBuilder builder,
        Expression<Func<TEntity, object?>> foreignKeyExpression
    )
    where TEntity : class
    where TRelatedEntity : class
    {
        builder
            .Entity<TEntity>()
            .HasOne<TRelatedEntity>()
            .WithMany()
            .HasForeignKey(foreignKeyExpression);

        return builder;
    }
}
