import { Paper, Grid } from '@mui/material';
import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { FilmShortDto } from '../film-short-item/film-short.dto.ts';
import FilmShortItem from '../film-short-item/FilmShortItem.tsx';

/** Пропсы компонента списка фильмов */
interface FilmsListProps {
  /** Список фильмов для отображения */
  films: FilmShortDto[];
  /** Коллбэк для обработки выбора фильма */
  onSelect: (id: string) => void;
  /** Флаг наличия дополнительных элементов для подгрузки */
  hasMore?: boolean;
  /** Функция подгрузки следующей порции фильмов */
  next?: () => void;
}

/**
 * Компонент компактного списка фильмов с бесконечной прокруткой
 * @param props - объект с пропсами FilmsListProps
 * @param props.films - список фильмов для отображения
 * @param props.onSelect - функция выбора фильма
 * @param props.hasMore - флаг наличия дополнительных элементов
 * @param props.next - функция подгрузки следующей порции фильмов
 * @returns {ReactElement} JSX элемент списка фильмов
 */
const FilmsList = ({
  films,
  onSelect,
  hasMore = false,
  next = () => {},
}: FilmsListProps): ReactElement => {
  return (
    <Paper sx={{ overflow: 'hidden' }}>
      <InfiniteScroll
        dataLength={films.length}
        next={next}
        hasMore={hasMore}
        loader={<Spinner />}
        style={{ overflow: 'visible' }}
      >
        <Grid container spacing={2}>
          {films.map((film) => (
            <Grid size="auto" key={film.id} sx={{ display: 'flex', justifyContent: 'center' }}>
              {/* Карточка компактного фильма */}
              <FilmShortItem film={film} onClick={() => onSelect(film.id)} />
            </Grid>
          ))}
        </Grid>
      </InfiniteScroll>
    </Paper>
  );
};

export default FilmsList;
