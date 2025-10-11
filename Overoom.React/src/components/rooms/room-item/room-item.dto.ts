/** Интерфейс элемента комнаты просмотра */
export interface RoomItemDto {
  /** Уникальный идентификатор комнаты */
  id: string;
  /** Название фильма/сериала */
  title: string;
  /** Год выпуска */
  year: number;
  /** URL постера */
  posterUrl: string;
  /** Рейтинг КиноПоиск */
  ratingKp: number | null;
  /** Рейтинг IMDb */
  ratingImdb: number | null;
  /** Описание фильма/сериала */
  description: string;
  /** Флаг сериала */
  isSerial: boolean;
  /** Жанры */
  genres: string[];
  /** Флаг приватной комнаты */
  isPrivate: boolean;
  /** Количество зрителей */
  viewersCount: number;
}
