using System;
using System.Linq;
using System.Linq.Expressions;

namespace Passingwind.LibraryGallery.Extensions
{
    public static class LinqExtensions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> expression)
        {
            if (condition)
                return source.Where(expression);
            return source;
        }
    }
}
