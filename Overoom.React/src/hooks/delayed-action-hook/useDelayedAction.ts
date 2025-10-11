import { useCallback, useRef, useEffect } from 'react';

/**
 * Кастомный хук для отложенного выполнения действия с использованием таймера.
 * Полезен для debounce-like сценариев или отложенных эффектов.
 * @param action - Функция, которая будет выполнена через указанную задержку
 * @param delay - Задержка в миллисекундах (по умолчанию 1000)
 * @returns {() => void} Callback функцию, которая запускает таймер
 */
const useDelayedAction = (action: () => void, delay = 1000): (() => void) => {
  /** Ref для хранения идентификатора таймера */
  const timerRef = useRef<number | null>(null);

  /** Функция-триггер для запуска отложенного действия */
  const trigger = useCallback(() => {
    // Очищаем предыдущий таймер, если он есть
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }

    // Запускаем новый таймер
    timerRef.current = window.setTimeout(() => {
      action();
      timerRef.current = null; // очищаем ref после выполнения
    }, delay);
  }, [action, delay]);

  /** Эффект для очистки таймера при размонтировании компонента */
  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, []);

  return trigger;
};

export default useDelayedAction;
