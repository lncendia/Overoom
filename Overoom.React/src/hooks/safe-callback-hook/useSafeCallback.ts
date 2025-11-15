/* eslint-disable @typescript-eslint/no-explicit-any,react-hooks/exhaustive-deps */
import React, { useCallback } from 'react';

import { useNotify } from '../../contexts/notify-context/useNotify.tsx';

/** Извлекает тип результата из асинхронной функции */
type AwaitedReturn<T> = T extends (...args: any[]) => Promise<infer R> ? R : never;

/**
 * Хук, возвращающий мемоизированную функцию с обработкой ошибок
 * @param callback - Асинхронная функция, которую нужно обернуть
 * @param deps - Список зависимостей для мемоизации, аналогично useCallback
 * @returns Мемoизированная версия функции с автоматической обработкой ошибок через notify
 */
export function useSafeCallback<T extends (...args: any[]) => Promise<any>>(
  callback: T,
  deps: React.DependencyList
): (...args: Parameters<T>) => Promise<AwaitedReturn<T>> {
  /** Извлекаем функцию setError из контекста уведомлений */
  const { setError } = useNotify();

  /** Мемoизированная функция с обработкой ошибок */
  return useCallback(
    async (...args: Parameters<T>) => {
      try {
        // Выполняем исходный callback
        return await callback(...args);
      } catch (error) {
        // В случае ошибки показываем уведомление
        setError(error instanceof Error ? error : new Error(String(error)));
      }
    },
    [...deps, setError] // Зависимости useCallback
  );
}
