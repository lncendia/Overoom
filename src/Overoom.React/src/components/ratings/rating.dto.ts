/** Интерфейс рейтинга фильма/сериала */
export interface RatingDto {
  /** Средняя оценка пользователей */
  userRating: number | null;
  /** Оценка текущего пользователя */
  userScore: number | null;
  /** Количество оценок пользователей */
  userRatingsCount: number;
}
