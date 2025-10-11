import { Box } from '@mui/material';
import { ReactElement } from 'react';
import Carousel from 'react-multi-carousel';

import GenreSliderItem from './GenreSliderItem.tsx';
import { useResponsiveSlider } from '../../../hooks/responsive-slider-hook/useResponsiveSlider.ts';

/** Пропсы компонента слайдера жанров */
interface GenreSliderProps {
  /** Текущий выбранный жанр */
  genre: string | undefined;
  /** Коллбэк для выбора жанра */
  onSelect: (genre?: string) => void;
  /** Список доступных жанров */
  genres: string[];
}

/**
 * Компонент карусели жанров
 * @param props - объект с пропсами
 * @param props.genre - текущий выбранный жанр
 * @param props.onSelect - функция выбора жанра
 * @param props.genres - список доступных жанров
 * @returns {ReactElement} JSX элемент карусели жанров
 */
const GenreSlider = ({ genre, onSelect, genres }: GenreSliderProps): ReactElement => {
  /** Хук для адаптивного расчета количества элементов слайдера по ширине */
  const responsive = useResponsiveSlider({
    startMin: 0,
    startMax: 630,
    startItems: 2,
    step: 380,
    maxWidth: 4000,
  });

  const carouselProps = {
    infinite: true,
    autoPlay: genre === undefined,
    autoPlaySpeed: 5000,
    keyBoardControl: true,
    responsive: responsive,
  };

  /**
   * Переключает выбранный жанр
   * @param newGenre - жанр для выбора
   */
  const toggleGenre = (newGenre: string) => {
    if (genre === newGenre) onSelect(undefined);
    else onSelect(newGenre);
  };

  return (
    <Carousel {...carouselProps}>
      {genres.map((g) => {
        return (
          <Box key={g} sx={{ display: 'flex', alignItems: 'center', m: 2 }}>
            {/* Элемент жанра с возможностью выбора */}
            <GenreSliderItem genre={g} selected={genre === g} onSelect={() => toggleGenre(g)} />
          </Box>
        );
      })}
    </Carousel>
  );
};

export default GenreSlider;
