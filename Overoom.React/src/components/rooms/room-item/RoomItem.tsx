import { Lock, LockOpen, People } from '@mui/icons-material';
import { Typography, Chip, Box, Stack } from '@mui/material';
import { ReactElement } from 'react';

import { RoomItemDto } from './room-item.dto.ts';
import FilmCard from '../../../ui/film-card/FilmCard.tsx';
import GenresList from '../../../ui/genres-list/GenresList.tsx';

/** Пропсы компонента RoomItem */
interface RoomItemProps {
  /** Данные о комнате */
  room: RoomItemDto;
  /** Обработчик клика по карточке */
  onClick: () => void;
}

/**
 * Компонент карточки комнаты с информацией о фильме/сериале
 * @param props - Пропсы компонента
 * @param props.room - Данные о комнате
 * @param props.onClick - Обработчик клика по карточке
 * @returns {ReactElement} JSX элемент карточки комнаты
 */
const RoomItem = ({ room, onClick }: RoomItemProps): ReactElement => {
  return (
    <FilmCard {...room} onClick={onClick} header={room.title}>
      {/* Описание комнаты */}
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
        {room.description}
      </Typography>

      {/* Список жанров */}
      <Box sx={{ mb: 4 }}>
        <GenresList genres={room.genres} />
      </Box>

      {/* Информация о приватности и числе зрителей */}
      <Stack
        direction="row"
        spacing={1}
        alignItems="center"
        sx={{
          position: 'absolute',
          right: (theme) => theme.spacing(2),
          bottom: (theme) => theme.spacing(2),
        }}
      >
        <Stack direction="row" spacing={1} alignItems="center">
          {room.isPrivate ? (
            <Lock color="error" fontSize="small" />
          ) : (
            <LockOpen color="success" fontSize="small" />
          )}
          <Stack direction="row" spacing={0.5} alignItems="center">
            <People fontSize="small" />
            <Typography variant="caption">{room.viewersCount}</Typography>
          </Stack>
        </Stack>

        {/* Индикатор типа контента (фильм/сериал) */}
        <Chip label={room.isSerial ? 'Сериал' : 'Фильм'} size="small" />
      </Stack>
    </FilmCard>
  );
};

export default RoomItem;
