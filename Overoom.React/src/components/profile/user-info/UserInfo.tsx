import { Paper, Chip, Typography, Avatar, Box } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/**
 * Интерфейс свойств компонента UserInfo
 */
interface UserInfoProps {
  /** Имя пользователя */
  userName: string;

  /** URL аватара пользователя */
  photoUrl: string | null;

  /** Список жанров пользователя */
  genres: string[];

  /** Обработчик выбора жанра */
  onGenreSelect: (genre: string) => void;
}

/** Стилизованный чип жанра */
const GenreChip = styled(Chip)(({ theme }) => ({
  marginRight: theme.spacing(1),
  marginBottom: theme.spacing(1),
  cursor: 'pointer',
  '&:hover': {
    backgroundColor: theme.palette.action.hover,
  },
}));

/**
 * Компонент информации о пользователе
 * @param props - Пропсы компонента
 * @param props.userName - Имя пользователя
 * @param props.photoUrl - URL аватара пользователя
 * @param props.genres - Список жанров пользователя
 * @param props.onGenreSelect - Обработчик выбора жанра
 * @returns {ReactElement} JSX элемент информации о пользователе
 */
const UserInfo = ({ userName, photoUrl, genres, onGenreSelect }: UserInfoProps): ReactElement => {
  return (
    <Paper
      sx={{
        display: 'flex',
        alignItems: 'center',
        padding: 2,
      }}
    >
      {/* Аватар пользователя */}
      <Avatar
        sx={{ width: 64, height: 64 }}
        alt="Аватар пользователя"
        src={photoUrl ?? undefined}
      />

      <Box sx={{ ml: 2 }}>
        {/* Имя пользователя */}
        <Typography variant="h6" component="h3" gutterBottom>
          {userName}
        </Typography>

        {/* Список жанров пользователя */}
        <Box sx={{ display: 'flex', flexWrap: 'wrap' }}>
          {genres.map((genre) => (
            <GenreChip
              key={genre}
              label={genre}
              size="small"
              onClick={() => onGenreSelect(genre)}
              sx={{ mr: 1, mb: 1 }}
            />
          ))}
        </Box>
      </Box>
    </Paper>
  );
};

export default UserInfo;
