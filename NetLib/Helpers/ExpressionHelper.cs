using System;
using System.Linq.Expressions;

namespace NetLib.Helpers
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// 创建setter方法
        /// </summary>
        public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(string name) where TEntity : class
        {
            var instance = Expression.Parameter(typeof(TEntity), "instance");
            var propertyValue = Expression.Parameter(typeof(TProperty), "propertyValue");
            var body = Expression.Assign(Expression.Property(instance, name), propertyValue);
            return Expression.Lambda<Action<TEntity, TProperty>>(body, instance, propertyValue).Compile();
        }

        /// <summary>
        /// 创建getter方法
        /// </summary>
        public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(string name) where TEntity : class
        {
            var instance = Expression.Parameter(typeof(TEntity), "instance");

            var body = Expression.Property(instance, name);

            return Expression.Lambda<Func<TEntity, TProperty>>(body, instance).Compile();
        }
    }
}
