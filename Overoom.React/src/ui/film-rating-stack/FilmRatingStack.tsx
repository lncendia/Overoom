import { Chip, Stack, SxProps } from '@mui/material';
import { ReactElement } from 'react';

/** Свойства для компонента FilmRatingStack */
export interface FilmRatingStackProps {
  /** Рейтинг Кинопоиска, может быть null */
  kp: number | null;
  /** Рейтинг IMDB, может быть null */
  imdb: number | null;
  /** Дополнительные стили для Stack */
  sx?: SxProps;
}

/**
 * Компонент для отображения рейтингов фильма в виде чипов
 * @param props - Свойства компонента
 * @param props.kp - Рейтинг Кинопоиска
 * @param props.imdb - Рейтинг IMDB
 * @param props.sx - Дополнительные стили для Stack
 * @returns {ReactElement} JSX элемент с чипами рейтингов
 */
const FilmRatingStack = ({ kp, imdb, sx }: FilmRatingStackProps): ReactElement => {
  return (
    <Stack
      direction="row"
      gap={1}
      sx={{
        position: 'absolute',
        zIndex: 1,
        flexWrap: 'wrap',
        justifyContent: 'end',
        ...sx,
      }}
    >
      {/* Чип с рейтингом Кинопоиска */}
      {kp && <Chip label={`КП: ${kp}`} color="primary" size="small" sx={{ fontWeight: 'bold' }} />}

      {/* Чип с рейтингом IMDB */}
      {imdb && (
        <Chip label={`IMDB: ${imdb}`} color="secondary" size="small" sx={{ fontWeight: 'bold' }} />
      )}
    </Stack>
  );
};

export default FilmRatingStack;
