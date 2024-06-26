﻿namespace Films.Domain.Ordering.Abstractions;

public interface ISortingVisitor<TVisitor, T> where TVisitor : ISortingVisitor<TVisitor, T>
{
    void Visit(DescendingOrder<T, TVisitor> order);
    void Visit(ThenByOrder<T, TVisitor> order);
    void Visit(RandomOrder<T, TVisitor> order);
}