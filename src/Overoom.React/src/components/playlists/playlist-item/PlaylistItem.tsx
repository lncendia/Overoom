import { Typography, Box } from '@mui/material';
import { ReactElement } from 'react';

import { PlaylistItemDto } from './playlist-item.dto.ts';
import FilmCard from '../../../ui/film-card/FilmCard.tsx';
import GenresList from '../../../ui/genres-list/GenresList.tsx';

/** Пропсы компонента PlaylistItem */
interface PlaylistItemProps {
  /** Данные подборки */
  playlist: PlaylistItemDto;
  /** Выбранный жанр (опционально) */
  selectedGenre?: string;
  /** Обработчик клика по карточке (используется, если не задан href) */
  onClick?: () => void;
  /** Адрес страницы подборки (карточка становится ссылкой) */
  href?: string;
}

/**
 * Компонент элемента плейлиста
 * @param props - Пропсы компонента
 * @param props.playlist - Данные плейлиста
 * @param props.selectedGenre - Выбранный жанр для подсветки
 * @param props.onClick - Обработчик клика по плейлисту
 * @param props.href - Адрес страницы подборки
 * @returns {ReactElement} JSX элемент плейлиста
 */
const PlaylistItem = ({
  playlist,
  selectedGenre,
  onClick,
  href,
}: PlaylistItemProps): ReactElement => {
  return (
    <FilmCard
      {...playlist}
      onClick={onClick}
      href={href}
      header={playlist.name}
      ratingKp={null}
      ratingImdb={null}
    >
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{
          mb: 3,
          display: '-webkit-box',
          WebkitLineClamp: 3,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
        }}
      >
        {playlist.description}
      </Typography>

      <Box sx={{ mb: 4 }}>
        <GenresList genres={playlist.genres} selected={selectedGenre} />
      </Box>
    </FilmCard>
  );
};

export default PlaylistItem;
