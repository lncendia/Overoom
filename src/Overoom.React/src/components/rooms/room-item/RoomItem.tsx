import { Lock, LockOpen, People } from '@mui/icons-material';
import { Typography, Chip, Box, Stack, Avatar } from '@mui/material';
import { ReactElement } from 'react';

import { RoomItemDto } from './room-item.dto.ts';
import FilmCard from '../../../ui/film-card/FilmCard.tsx';
import GenresList from '../../../ui/genres-list/GenresList.tsx';

/** Пропсы компонента RoomItem */
interface RoomItemProps {
  /** Данные о комнате */
  room: RoomItemDto;
  /** Адрес страницы комнаты */
  href: string;
}

/**
 * Компонент карточки комнаты с информацией о фильме/сериале
 * @param props - Пропсы компонента
 * @param props.room - Данные о комнате
 * @param props.href - Адрес страницы комнаты
 * @returns {ReactElement} JSX элемент карточки комнаты
 */
const RoomItem = ({ room, href }: RoomItemProps): ReactElement => {
  return (
    <FilmCard {...room} href={href} header={room.title}>
      <Typography
        variant="body2"
        sx={{
          color: 'text.secondary',
          mb: 3,
          display: '-webkit-box',
          WebkitLineClamp: 3,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
        }}
      >
        {room.description}
      </Typography>

      <Box sx={{ mb: 4 }}>
        <GenresList genres={room.genres} />
      </Box>

      <Stack
        direction="row"
        spacing={1}
        sx={{
          alignItems: 'center',
          position: 'absolute',
          right: (theme) => theme.spacing(2),
          bottom: (theme) => theme.spacing(2),
        }}
      >
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          {room.isPrivate ? (
            <Lock color="error" fontSize="small" />
          ) : (
            <LockOpen color="success" fontSize="small" />
          )}
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
            <People fontSize="small" />
            <Typography variant="caption">{room.viewersCount}</Typography>
          </Stack>
        </Stack>

        <Chip
          avatar={<Avatar alt={room.userName} src={room.photoUrl ?? undefined} />}
          label={room.userName}
          size="small"
        />

        <Chip label={room.isSerial ? 'Сериал' : 'Фильм'} size="small" />
      </Stack>
    </FilmCard>
  );
};

export default RoomItem;
