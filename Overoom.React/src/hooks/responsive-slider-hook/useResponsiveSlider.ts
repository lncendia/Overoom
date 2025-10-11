import { useMemo } from 'react';

/**
 * Хук для генерации объекта responsive-настроек для карусели
 * @param options - Объект параметров
 * @param options.startMin - Минимальное значение первой зоны (по умолчанию 0)
 * @param options.startMax - Максимум первой зоны
 * @param options.startItems - Количество элементов в первой зоне
 * @param options.step - Шаг увеличения ширины для каждой следующей зоны
 * @param options.maxWidth - Максимальная ширина, до которой продолжается генерация
 * @returns Объект responsive-настроек для компонента карусели
 */
export function useResponsiveSlider({
  startMin,
  startMax,
  startItems,
  step,
  maxWidth,
}: {
  startMin: number;
  startMax: number;
  startItems: number;
  step: number;
  maxWidth: number;
}) {
  /** Генерируем responsive-настройки для карусели с использованием useMemo для оптимизации */
  return useMemo(() => {
    const result: Record<
      string,
      {
        breakpoint: { max: number; min: number };
        items: number;
        slidesToSlide: number;
      }
    > = {};

    let min = startMin;
    let max = startMax;
    let items = startItems;
    let index = 0;

    // Генерируем диапазоны, пока не достигнем максимальной ширины
    while (min < maxWidth) {
      const key = `bp${index++}`;
      result[key] = {
        breakpoint: { max, min },
        items,
        slidesToSlide: 1,
      };

      // Смещаем диапазон и увеличиваем количество элементов
      min = max;
      max += step;
      items++;
    }

    return result;
  }, [startMin, startMax, startItems, step, maxWidth]);
}
