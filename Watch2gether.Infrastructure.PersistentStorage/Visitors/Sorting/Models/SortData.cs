﻿using System.Linq.Expressions;

namespace Watch2gether.Infrastructure.PersistentStorage.Visitors.Sorting.Models;

public class SortData<TEntity>
{
    public SortData(Expression<Func<TEntity, dynamic>> expr, bool isDescending)
    {
        Expr = expr;
        IsDescending = isDescending;
    }

    public Expression<Func<TEntity, dynamic>> Expr { get; }
    public bool IsDescending { get; }
}