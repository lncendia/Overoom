import { useEffect, useRef, useState, useCallback } from 'react';

import { useNotify } from '../../contexts/notify-context/useNotify.tsx';
import { CountResult } from '../../services/common/count-result.ts';

/**
 * Хук для пагинированной загрузки данных с поддержкой добавления, удаления и сброса
 * @param fetchFn - Функция для загрузки данных, возвращающая результат с общим количеством
 * @param pageSize - Размер страницы (по умолчанию 20 элементов)
 * @returns Объект с состоянием пагинации, списком элементов, ошибкой и функциями управления
 */
export function usePaginatedFetch<T>(
  fetchFn: (skip: number, take: number) => Promise<CountResult<T> | null>,
  pageSize: number = 20
) {
  /** Состояние массива загруженных элементов */
  const [items, setItems] = useState<T[]>([]);

  /** Состояние первоначальной загрузки данных */
  const [isLoading, setIsLoading] = useState(true);

  /** Ошибка последней загрузки (null, если ошибки нет) */
  const [error, setError] = useState<Error | null>(null);

  /** Функция показа уведомления об ошибке */
  const { setError: notifyError } = useNotify();

  /** Ссылка на общее количество элементов (для предотвращения лишних ререндеров) */
  const totalCount = useRef(0);

  /**
   * Номер «поколения» запросов. Увеличивается при смене fetchFn и сбросе,
   * чтобы ответы устаревших запросов игнорировались
   */
  const generation = useRef(0);

  /** Флаг выполняющегося запроса (защита от параллельной загрузки одной и той же страницы) */
  const inFlight = useRef(false);

  /** Флаг наличия дополнительных элементов для загрузки */
  const hasMore = !error && items.length < totalCount.current;

  /**
   * Функция загрузки страницы данных
   * @param skip - Количество элементов, которые нужно пропустить
   * @returns {Promise<void>}
   */
  const fetchPage = useCallback(
    async (skip: number) => {
      if (inFlight.current) return;

      const current = generation.current;
      inFlight.current = true;
      setError(null);

      try {
        const response = await fetchFn(skip, pageSize);
        if (current !== generation.current || !response) return;
        totalCount.current = response.totalCount;
        setItems((prev) => [...prev, ...response.list]);
      } catch (e) {
        if (current !== generation.current) return;
        const err = e instanceof Error ? e : new Error(String(e));
        setError(err);
        notifyError(err);
      } finally {
        if (current === generation.current) {
          inFlight.current = false;
          setIsLoading(false);
        }
      }
    },
    [fetchFn, pageSize, notifyError]
  );

  /** Эффект для первоначальной загрузки данных и очистки при смене fetchFn или размонтировании */
  useEffect(() => {
    fetchPage(0).then();
    return () => {
      generation.current += 1;
      inFlight.current = false;
      totalCount.current = 0;
      setItems([]);
      setError(null);
      setIsLoading(true);
    };
  }, [fetchPage]);

  /**
   * Функция сброса состояния хука к начальному
   * @returns void
   */
  const reset = useCallback(() => {
    generation.current += 1;
    inFlight.current = false;
    totalCount.current = 0;
    setItems([]);
    setError(null);
    setIsLoading(true);
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
   * Функция изменения элементов списка без перезагрузки
   * @param updater - Функция, возвращающая новый элемент (или тот же, если он не меняется)
   * @returns void
   */
  const update = useCallback((updater: (item: T) => T) => {
    setItems((prev) => prev.map(updater));
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

  /**
   * Функция повторной загрузки после ошибки: догружает следующую страницу
   * или заново загружает первую, если элементов еще нет
   * @returns {Promise<void>}
   */
  const retry = useCallback(() => {
    if (items.length === 0) setIsLoading(true);
    return fetchPage(items.length);
  }, [fetchPage, items.length]);

  return {
    items,
    isLoading,
    hasMore,
    fetchMore,
    reset,
    removeWhere,
    add,
    update,
    error,
    retry,
  };
}
