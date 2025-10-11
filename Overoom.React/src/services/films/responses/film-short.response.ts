/** Краткие данные фильма */
export interface FilmShortResponse {
  /** Идентификатор фильма */
  id: string;
  /** Название фильма */
  title: string;
  /** Год выпуска */
  year: number;
  /** URL постера фильма */
  posterUrl: string;
  /** Ключ постера в хранилище */
  posterKey: string;
  /** Рейтинг КиноПоиск (опционально) */
  ratingKp: number | null;
  /** Рейтинг IMDB (опционально) */
  ratingImdb: number | null;
  /** Описание фильма */
  description: string;
  /** Флаг сериала */
  isSerial: boolean;
  /** Жанры фильма */
  genres: string[];
}
