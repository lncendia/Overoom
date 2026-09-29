import { Box, Paper } from '@mui/material';
import { ReactElement } from 'react';

import { useResponsiveSlider } from '../../../hooks/responsive-slider-hook/useResponsiveSlider.ts';
import Carousel from '../../../ui/carousel/Carousel.ts';
import { FilmShortDto } from '../film-short-item/film-short.dto.ts';
import FilmShortItem from '../film-short-item/FilmShortItem.tsx';

/** Пропсы компонента слайдера фильмов */
interface FilmsSliderProps {
  /** Список фильмов для отображения в слайдере */
  films: FilmShortDto[];
  /** Коллбэк для обработки выбора фильма (используется, если не задан getLink) */
  onSelect?: (id: string) => void;
  /** Функция построения адреса страницы (элементы становятся ссылками) */
  getLink?: (id: string) => string;
}

/**
 * Компонент карусели с фильмами
 * @param props - объект с пропсами
 * @param props.films - список фильмов для слайдера
 * @param props.onSelect - функция выбора фильма при клике на карточку
 * @param props.getLink - функция построения адреса страницы
 * @returns {ReactElement} JSX элемент слайдера фильмов
 */
const FilmsSlider = ({ films, onSelect, getLink }: FilmsSliderProps): ReactElement => {
  /** Хук для адаптивного расчета количества элементов слайдера по ширине */
  const responsive = useResponsiveSlider({
    startMin: 0,
    startMax: 500,
    startItems: 2,
    step: 250,
    maxWidth: 4000,
  });

  const carouselProps = {
    infinite: true,
    autoPlay: true,
    autoPlaySpeed: 5000,
    keyBoardControl: true,
    responsive: responsive,
  };

  return (
    <Paper>
      <Carousel {...carouselProps}>
        {films.map((film) => {
          return (
            <Box
              key={film.id}
              sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', py: '5px' }}
            >
              <FilmShortItem
                film={film}
                href={getLink?.(film.id)}
                onClick={onSelect ? () => onSelect(film.id) : undefined}
              />
            </Box>
          );
        })}
      </Carousel>
    </Paper>
  );
};

export default FilmsSlider;
