import { Rating as MuiRating, Typography, Box } from '@mui/material';
import { ReactElement, useCallback, useState } from 'react';

import { RatingDto } from './rating.dto.ts';

/**
 * Интерфейс пропсов компонента Rating
 */
interface RatingProps {
  /** Данные рейтинга */
  rating: RatingDto;
  /** Обработчик изменения оценки */
  scoreChanged: (score: number) => void;
}

/**
 * Возвращает текстовое описание оценки
 * @param rating - Оценка от 1 до 10
 * @returns {string} Текстовое описание
 */
const getReviewLabel = (rating: number): string => {
  switch (rating) {
    case 1:
      return 'Ужасно 🤮';
    case 2:
      return 'Плохо 🥺';
    case 3:
      return 'Удовлетворительно ☹️';
    case 4:
      return 'Хорошо 😌';
    case 5:
      return 'Очень хорошо 😃';
    case 6:
      return 'Отлично 😇';
    case 7:
      return 'Замечательно 👏';
    case 8:
      return 'Супер 😱';
    case 9:
      return 'Великолепно 🤩';
    case 10:
      return 'Превосходно 🤯';
    default:
      return '';
  }
};

/**
 * Возвращает правильную форму слова "оценка" в зависимости от числа
 * @param count - Количество оценок
 * @returns {string} Слово в нужной форме
 */
const getScoresCountString = (count: number): string => {
  const mod100 = count % 100;
  const mod10 = count % 10;
  if (mod100 >= 11 && mod100 <= 14) return 'оценок';
  if (mod10 === 1) return 'оценка';
  if (mod10 >= 2 && mod10 <= 4) return 'оценки';
  return 'оценок';
};

/**
 * Формирует строку с количеством оценок и средним рейтингом
 * @param rating - Данные рейтинга
 * @returns {string} Строка вида "8.2 ★, 21 оценка" или "Нет оценок"
 */
const getScoresString = (rating: RatingDto): string => {
  if (!rating.userRating) return 'Нет оценок';
  const count = rating.userRatingsCount;
  return `${rating.userRating.toFixed(1)} ★, ${count} ${getScoresCountString(count)}`;
};

/**
 * Компонент рейтинга с возможностью оценки пользователем
 * @param props - Пропсы, переданные в компонент.
 * @param props.rating - Данные рейтинга
 * @param props.scoreChanged - Функция для обработки изменения оценки
 * @returns {ReactElement} JSX элемент компонента рейтинга
 */
const Rating = ({ rating, scoreChanged }: RatingProps): ReactElement => {
  /** Используем хук useState для хранения рейтинга при наведении */
  const [hoverRating, setHoverRating] = useState<number | null>(null);

  /**
   * Используем useCallback для оптимизации коллбэка изменения оценки.
   * MUI Rating передает null при клике по уже выбранной звезде: если собственной оценки нет
   * (отображается средняя), то отправляем именно ту звезду, по которой кликнули, а не среднее.
   * @param newValue - Новое значение рейтинга
   * @returns {void}
   */
  const onScoreChanged = useCallback(
    (newValue: number | null) => {
      if (newValue) {
        scoreChanged(newValue);
        return;
      }
      if (!rating.userScore && hoverRating && hoverRating > 0) scoreChanged(hoverRating);
    },
    [hoverRating, rating.userScore, scoreChanged]
  );

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <MuiRating
          name="film-rating"
          size="large"
          getLabelText={getReviewLabel}
          value={rating.userScore ?? rating.userRating}
          max={10}
          precision={1}
          onChange={(_, newValue) => onScoreChanged(newValue)}
          onChangeActive={(_, newHover) => setHoverRating(newHover)}
          sx={{
            '& .MuiRating-iconFilled': {
              // Собственная оценка выделяется акцентным цветом, средняя — приглушенным
              color: rating.userScore ? 'secondary.main' : 'grey.500',
            },
          }}
        />

        {hoverRating !== null && hoverRating > 0 && (
          <Typography variant="body2" sx={{ minWidth: 150 }}>
            {getReviewLabel(hoverRating)}
          </Typography>
        )}
      </Box>

      <Typography variant="caption" color="text.secondary">
        {getScoresString(rating)}
      </Typography>
    </Box>
  );
};

export default Rating;
