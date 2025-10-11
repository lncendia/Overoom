/** Интерфейс рейтинга фильма/сериала */
export interface RatingDto {
  /** Рейтинг пользователя */
  userRating: number | null;
  /** Оценка пользователя */
  userScore: number | null;
  /** Количество оценок пользователя */
  userRatingsCount: number;
}
