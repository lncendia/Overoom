import { useEffect, useMemo, useRef, useState, useCallback } from 'react';

import { CountResult } from '../../services/common/count-result.ts';
import { useSafeCallback } from '../safe-callback-hook/useSafeCallback.ts';

/**
 * Хук для пагинированной загрузки данных с поддержкой добавления, удаления и сброса
 * @param fetchFn - Функция для загрузки данных, возвращающая результат с общим количеством
 * @param pageSize - Размер страницы (по умолчанию 20 элементов)
 * @returns Объект с состоянием пагинации, списком элементов и функциями управления
 */
export function usePaginatedFetch<T>(
  fetchFn: (skip: number, take: number) => Promise<CountResult<T> | null>,
  pageSize: number = 20
) {
  /** Состояние массива загруженных элементов */
  const [items, setItems] = useState<T[]>([]);

  /** Состояние загрузки данных */
  const [isLoading, setIsLoading] = useState(true);

  /** Ссылка на общее количество элементов (для предотвращения лишних ререндеров) */
  const totalCount = useRef(0);

  /** Флаг наличия дополнительных элементов для загрузки */
  const hasMore = useMemo(() => items.length < totalCount.current, [items.length]);

  /**
   * Функция загрузки страницы данных
   * @param skip - Количество элементов, которые нужно пропустить
   * @returns {Promise<void>}
   */
  const fetchPage = useSafeCallback(
    async (skip: number) => {
      const response = await fetchFn(skip, pageSize);
      if (!response) return;
      totalCount.current = response.totalCount;
      setItems((prev) => [...prev, ...response.list]);
      setIsLoading(false);
    },
    [fetchFn, pageSize]
  );

  /** Эффект для первоначальной загрузки данных и очистки при размонтировании */
  useEffect(() => {
    fetchPage(0);
    return () => {
      setItems([]);
      setIsLoading(true);
      totalCount.current = 0;
    };
  }, [fetchPage]);

  /**
   * Функция сброса состояния хука к начальному
   * @returns void
   */
  const reset = useCallback(() => {
    setItems([]);
    setIsLoading(true);
    totalCount.current = 0;
  }, []);

  /**
   * Функция удаления элементов по условию с обновлением общего количества
   * @param predicate - Функция для фильтрации элементов, которые нужно удалить
   * @returns void
   */
  const removeWhere = useCallback((predicate: (item: T) => boolean) => {
    setItems((prev) => {
      const newItems = prev.filter((item) => !predicate(item));
      const removedCount = prev.length - newItems.length;
      totalCount.current = Math.max(0, totalCount.current - removedCount);
      return newItems;
    });
  }, []);

  /**
   * Функция добавления новых элементов в начало списка
   * @param newItemOrItems - Один элемент или массив элементов для добавления
   * @returns void
   */
  const add = useCallback((newItemOrItems: T | T[]) => {
    setItems((prev) => {
      const toAdd = Array.isArray(newItemOrItems) ? newItemOrItems : [newItemOrItems];
      totalCount.current += toAdd.length;
      return [...toAdd, ...prev];
    });
  }, []);

  /** Функция загрузки следующей страницы данных */
  const fetchMore = useCallback(() => fetchPage(items.length), [fetchPage, items.length]);

  return {
    items,
    isLoading,
    hasMore,
    fetchMore,
    reset,
    removeWhere,
    add,
  };
}
