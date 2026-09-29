/* eslint-disable @typescript-eslint/no-explicit-any,react-hooks/exhaustive-deps */
import React, { useCallback } from 'react';

import { useNotify } from '../../contexts/notify-context/useNotify.tsx';

/**
 * Хук, возвращающий мемоизированную функцию с обработкой ошибок
 * @param callback - Асинхронная функция, которую нужно обернуть
 * @param deps - Список зависимостей для мемоизации, аналогично useCallback
 * @returns Мемoизированная версия функции с автоматической обработкой ошибок через notify.
 * При ошибке промис разрешается значением undefined.
 */
export function useSafeCallback<A extends any[], R>(
  callback: (...args: A) => Promise<R>,
  deps: React.DependencyList
): (...args: A) => Promise<R | undefined> {
  /** Извлекаем функцию setError из контекста уведомлений */
  const { setError } = useNotify();

  /** Мемoизированная функция с обработкой ошибок */
  return useCallback(
    async (...args: A): Promise<R | undefined> => {
      try {
        return await callback(...args);
      } catch (error) {
        setError(error instanceof Error ? error : new Error(String(error)));
        return undefined;
      }
    },
    [...deps, setError] // Зависимости useCallback
  );
}
