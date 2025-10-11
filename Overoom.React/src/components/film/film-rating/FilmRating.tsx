import { Paper, SxProps, Theme, Typography } from '@mui/material';
import { ReactElement } from 'react';

import { RatingDto } from '../../ratings/rating.dto.ts';
import Rating from '../../ratings/Rating.tsx';

/** Пропсы компонента рейтинга фильма */
export interface FilmRatingProps {
  /** Данные рейтинга */
  rating: RatingDto;
  /** Флаг, указывающий, что контент является сериалом */
  isSerial: boolean;
  /** Имя пользователя (необязательное) */
  userName?: string;
  /** Коллбэк для обработки изменения оценки */
  onScoreChanged: (value: number) => void;
  /** Дополнительные стили для обертки Paper */
  sx?: SxProps<Theme>;
}

/**
 * Компонент отображения и управления рейтингом фильма или сериала
 * @param props - объект с пропсами FilmRatingProps
 * @param props.rating - данные рейтинга
 * @param props.isSerial - флаг, является ли контент сериалом
 * @param props.userName - имя пользователя для персонализированного текста
 * @param props.sx - дополнительные стили для компонента Paper
 * @param props.onScoreChanged - коллбэк для изменения оценки
 * @returns {ReactElement} JSX элемент компонента рейтинга
 */
const FilmRating = ({
  rating,
  isSerial,
  userName,
  sx,
  onScoreChanged,
}: FilmRatingProps): ReactElement => {
  // Определяем тип контента для отображения в тексте (фильм или сериал)
  const mediaType = isSerial ? 'сериал' : 'фильм';

  // Формируем персонализированное приветствие для пользователя
  const userPrefix = userName ? `${userName.split(' ')[0]}, к` : 'К';

  return (
    <Paper sx={sx}>
      {/* Заголовок с вопросом о рейтинге */}
      <Typography variant="h6" component="span">
        {userPrefix}
        ак вам&nbsp;
        {mediaType}?
      </Typography>

      {/* Компонент рейтинга с передачей данных и коллбэка */}
      <Rating rating={rating} scoreChanged={onScoreChanged} />
    </Paper>
  );
};

export default FilmRating;
