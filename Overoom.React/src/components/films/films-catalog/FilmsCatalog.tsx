import { Grid } from '@mui/material';
import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { FilmItemDto } from '../film-item/film-item.dto.ts';
import FilmItem from '../film-item/FilmItem.tsx';

/**
 * Пропсы компонента каталога фильмов
 */
export interface FilmsCatalogProps {
  /** Список фильмов для отображения */
  films: FilmItemDto[];
  /** Выбранный жанр для фильтрации (необязательный) */
  genre?: string;
  /** Флаг выбранного типа контента (фильм или сериал) */
  typeSelected: boolean;
  /** Коллбэк для обработки выбора фильма */
  onSelect: (id: string) => void;
  /** Флаг наличия дополнительных фильмов для загрузки */
  hasMore: boolean;
  /** Функция для подгрузки следующей порции фильмов */
  next: () => void;
}

/**
 * Компонент каталога фильмов с поддержкой бесконечной прокрутки
 * @param props - объект с пропсами FilmsCatalogProps
 * @param props.films - список фильмов для отображения
 * @param props.genre - выбранный жанр для фильтрации
 * @param props.typeSelected - флаг выбранного типа контента
 * @param props.onSelect - функция выбора фильма
 * @param props.hasMore - флаг наличия дополнительных элементов
 * @param props.next - функция подгрузки следующей порции фильмов
 * @returns {ReactElement} JSX элемент каталога фильмов
 */
const FilmsCatalog = ({
  films,
  genre,
  typeSelected,
  onSelect,
  hasMore,
  next,
}: FilmsCatalogProps): ReactElement => {
  return (
    <InfiniteScroll
      dataLength={films.length}
      next={next}
      hasMore={hasMore}
      loader={<Spinner />}
      style={{ overflow: 'visible' }}
    >
      <Grid container spacing={5} sx={{ mt: 4 }}>
        {films.map((film) => (
          <Grid
            size={{ xs: 12, sm: 6, md: 4, lg: 6, xl: 4 }}
            key={film.id}
            sx={{ display: 'flex', justifyContent: 'center' }}
          >
            {/* Карточка отдельного фильма/сериала */}
            <FilmItem
              selectedGenre={genre}
              film={film}
              onClick={() => onSelect(film.id)}
              typeSelected={typeSelected}
            />
          </Grid>
        ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default FilmsCatalog;
