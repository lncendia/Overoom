import { SyntheticEvent } from 'react';

/** Заглушка постера (встроенное SVG-изображение), используется при ошибке загрузки */
export const POSTER_FALLBACK =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    '<svg xmlns="http://www.w3.org/2000/svg" width="200" height="300" viewBox="0 0 200 300">' +
      '<rect width="200" height="300" fill="#2b2b2b"/>' +
      '<path fill="#6b6b6b" d="M70 120h60a6 6 0 0 1 6 6v48a6 6 0 0 1-6 6H70a6 6 0 0 1-6-6v-48a6 6 0 0 1 6-6zm7 10v40l33-20z"/>' +
      '</svg>'
  );

/**
 * Обработчик ошибки загрузки постера: подставляет заглушку и снимает обработчик,
 * чтобы исключить бесконечный цикл при ошибке загрузки самой заглушки
 * @param event - Событие ошибки загрузки изображения
 * @returns {void}
 */
export const handlePosterError = (event: SyntheticEvent<HTMLImageElement>): void => {
  const image = event.currentTarget;
  image.onerror = null;
  if (image.src !== POSTER_FALLBACK) image.src = POSTER_FALLBACK;
};
