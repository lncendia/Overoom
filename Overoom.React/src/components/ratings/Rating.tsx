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
  count = count % 10;
  if (count === 1) return 'оценка';
  if (count > 1 && count < 5) return 'оценки';
  return 'оценок';
};

/**
 * Формирует строку с количеством оценок и средним рейтингом
 * @param rating - Данные рейтинга
 * @returns {string} Строка вида "8 ★, 20 оценок" или "Нет оценок"
 */
const getScoresString = (rating: RatingDto): string => {
  if (!rating.userRating) return 'Нет оценок';
  return `${rating.userRating} ★, ${rating.userRatingsCount} ${getScoresCountString(rating.userRatingsCount)}`;
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
   * Используем useCallback для оптимизации коллбэка изменения оценки
   * @param newValue - Новое значение рейтинга
   * @returns {void}
   */
  const onScoreChanged = useCallback(
    (newValue: number | null) => {
      if (!newValue && !rating.userScore && rating.userRating) {
        scoreChanged(rating.userRating);
      }
      if (!newValue) return;
      scoreChanged(newValue);
    },
    [rating.userRating, rating.userScore, scoreChanged]
  );

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
      {/* Блок с рейтингом и текстовой подсказкой */}
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
            '& .MuiRating-icon': {
              color: rating.userScore ? 'grey.500' : 'secondary.light', // Серый цвет для userScore, основной цвет для остальных
            },
          }}
        />

        {/* Отображаем текстовую подсказку при наведении */}
        {hoverRating && (
          <Typography variant="body2" sx={{ minWidth: 150 }}>
            {getReviewLabel(hoverRating)}
          </Typography>
        )}
      </Box>

      {/* Блок с количеством оценок */}
      <Typography variant="caption" color="text.secondary">
        {getScoresString(rating)}
      </Typography>
    </Box>
  );
};

export default Rating;
