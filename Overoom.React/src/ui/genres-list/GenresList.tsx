import { Box, Chip, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/** Свойства для компонента GenresList */
export interface GenresListProps {
  /** Массив жанров */
  genres: string[];
  /** Выбранный жанр (опционально) */
  selected?: string;
  /** Максимальное количество отображаемых жанров (опционально) */
  limit?: number;
}

/** Стилизованный чип для отображения жанра с выделением выбранного */
const GenreChip = styled(Chip)(({ theme }) => ({
  marginRight: theme.spacing(0.5),
  marginBottom: theme.spacing(0.5),
  fontSize: '0.75rem',
  height: '24px',
  '&.selected': {
    backgroundColor: theme.palette.action.selected,
    fontWeight: 500,
  },
}));

/**
 * Компонент для отображения списка жанров с возможностью выделения выбранного и ограничения по количеству
 * @param props - Свойства компонента
 * @param props.genres - Массив жанров
 * @param props.selected - Выбранный жанр
 * @param props.limit - Максимальное количество отображаемых жанров
 * @returns {ReactElement} JSX элемент со списком жанров
 */
const GenresList = ({ genres, selected, limit }: GenresListProps): ReactElement => {
  // Ограничиваем количество отображаемых жанров, если указан limit
  const displayedGenres = limit ? genres.slice(0, limit) : genres;

  // Считаем количество скрытых жанров
  const remainingCount = limit && genres.length > limit ? genres.length - limit : 0;

  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center' }}>
      {/* Чипы с жанрами */}
      {displayedGenres.map((genre) => (
        <GenreChip
          key={genre}
          label={genre}
          size="small"
          variant="outlined"
          className={genre === selected ? 'selected' : ''}
          sx={{
            mr: 0.5,
            mb: 0.5,
            borderColor: genre === selected ? 'transparent' : null,
          }}
        />
      ))}

      {/* Показываем количество скрытых жанров, если они есть */}
      {remainingCount > 0 && (
        <Typography variant="caption" color="text.secondary" sx={{ ml: 0.5 }}>
          +{remainingCount}
        </Typography>
      )}
    </Box>
  );
};

export default GenresList;
