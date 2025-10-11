/** DTO информации о фильме */
export interface FilmInfoDto {
  /** Идентификатор фильма */
  id: string;
  /** Название фильма */
  title: string;
  /** Описание фильма */
  description: string;
  /** Год выпуска */
  year: number;
  /** URL постера фильма */
  posterUrl: string;
  /** Рейтинг КиноПоиск */
  ratingKp: number | null;
  /** Рейтинг IMDB */
  ratingImdb: number | null;
  /** Флаг сериала */
  isSerial: boolean;
  /** Жанры фильма */
  genres: string[];
  /** Страны производства */
  countries: string[];
  /** Режиссеры */
  directors: string[];
  /** Сценаристы */
  screenWriters: string[];
  /** Актеры */
  actors: ActorDto[];
}

/** DTO актера */
export interface ActorDto {
  /** Имя актера */
  name: string;
  /** Роль актера */
  role: string | null;
}
