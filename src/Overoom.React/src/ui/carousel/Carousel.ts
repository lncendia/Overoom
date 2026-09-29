import CarouselModule from 'react-multi-carousel';

/**
 * Карусель из react-multi-carousel.
 * Пакет распространяется только как CommonJS: в dev-режиме Vite импорт по умолчанию отдает
 * весь module.exports, а в production-сборке — сам компонент, поэтому берем default вручную
 */
const Carousel =
  (CarouselModule as unknown as { default?: typeof CarouselModule }).default ?? CarouselModule;

export default Carousel;
