/** DTO краткой информации о фильме */
export interface FilmShortDto {
  /** Идентификатор фильма */
  id: string;
  /** Название фильма */
  title: string;
  /** URL постера фильма */
  posterUrl: string;
  /** Рейтинг КиноПоиск */
  ratingKp: number | null;
  /** Рейтинг IMDB */
  ratingImdb: number | null;
  /** Оценка пользователя */
  score?: number | null;
}
