import { Divider, Paper, Stack } from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement, useMemo } from 'react';

import FilmInfoCard from '../../../ui/film-info-card/FilmInfoCard.tsx';
import KeyList from '../../../ui/key-list/KeyList.tsx';

/**
 * Интерфейс свойств компонента информации о плейлисте
 */
export interface PlaylistInfo {
  /** Обработчик выбора жанра для навигации к поиску */
  onGenreSelect: (value: string) => void;
  /** Название плейлиста */
  name: string;
  /** Описание плейлиста */
  description: string;
  /** URL постера плейлиста */
  posterUrl: string;
  /** Дата последнего обновления плейлиста */
  updated: Date;
  /** Массив жанров плейлиста */
  genres: string[];
}

/**
 * Компонент отображения детальной информации о плейлисте.
 * Включает название, описание, постер, жанры и дату обновления
 * @param props - Свойства компонента PlaylistInfo
 * @returns {ReactElement} JSX элемент информации о плейлисте
 */
const PlaylistInfo = (props: PlaylistInfo): ReactElement => {
  const formatedDate = useMemo(() => {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(props.updated);
  }, [props.updated]);

  return (
    <Paper sx={{ p: 2, my: 2 }}>
      {/** Компонент карточки фильма, переиспользуемый для плейлистов */}
      <FilmInfoCard {...props} ratingKp={null} ratingImdb={null}>
        {/* Заголовок плейлиста */}
        <Typography variant="h4" component="h1">
          {props.name}
        </Typography>

        <Divider sx={{ my: 2, borderWidth: 1 }} />

        {/* Описание плейлиста */}
        <Typography variant="body1">{props.description}</Typography>

        <Divider sx={{ my: 2, borderWidth: 1 }} />

        {/* Списки информации о плейлисте */}
        <Stack spacing={1}>
          {/** Список жанров плейлиста с возможностью выбора для поиска */}
          <KeyList title="Жанр:" values={props.genres} onKeySelect={props.onGenreSelect} />

          {/** Дата последнего обновления плейлиста */}
          <KeyList title="Обновлена:" values={[formatedDate]} />
        </Stack>
      </FilmInfoCard>
    </Paper>
  );
};

export default PlaylistInfo;
