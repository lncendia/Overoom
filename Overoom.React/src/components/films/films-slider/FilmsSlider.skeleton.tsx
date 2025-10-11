import { Box, Paper } from '@mui/material';
import { ReactElement } from 'react';
import Carousel from 'react-multi-carousel';

import { useResponsiveSlider } from '../../../hooks/responsive-slider-hook/useResponsiveSlider.ts';
import FilmShortItemSkeleton from '../film-short-item/FilmShortItem.skeleton.tsx';

/**
 * Компонент скелетона карусели с фильмами
 * @returns {ReactElement} JSX элемент скелетона карусели фильмов
 */
const FilmsSliderSkeleton = (): ReactElement => {
  /** Используем хук useResponsiveSlider для получения адаптивных настроек слайдера */
  const responsive = useResponsiveSlider({
    startMin: 0,
    startMax: 500,
    startItems: 2,
    step: 250,
    maxWidth: 4000,
  });

  // Настройки карусели
  const carouselProps = {
    infinite: true,
    autoPlay: true,
    autoPlaySpeed: 5000,
    keyBoardControl: true,
    responsive: responsive,
  };

  /** Массив для генерации заглушек элементов карусели */
  const items = Array.from({ length: 12 }, (_, i) => i);

  return (
    <Paper>
      <Carousel {...carouselProps}>
        {items.map((i) => {
          return (
            <Box
              key={i}
              sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', py: '5px' }}
            >
              {/* Скелетон элемента фильма в карусели */}
              <FilmShortItemSkeleton />
            </Box>
          );
        })}
      </Carousel>
    </Paper>
  );
};

export default FilmsSliderSkeleton;
