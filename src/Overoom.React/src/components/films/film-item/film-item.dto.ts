/** DTO элемента фильма для каталога */
export interface FilmItemDto {
  /** Идентификатор фильма */
  id: string;
  /** Название фильма */
  title: string;
  /** Год выпуска */
  year: number;
  /** URL постера фильма */
  posterUrl: string;
  /** Рейтинг КиноПоиск */
  ratingKp: number | null;
  /** Рейтинг IMDB */
  ratingImdb: number | null;
  /** Описание фильма */
  description: string;
  /** Флаг сериала */
  isSerial: boolean;
  /** Жанры фильма */
  genres: string[];
}
