/** Краткая информация о комнате */
export interface RoomShortResponse {
  /** Идентификатор комнаты */
  id: string;
  /** Количество зрителей в комнате */
  viewersCount: number;
  /** Флаг приватности комнаты */
  isPrivate: boolean;
  /** Идентификатор фильма */
  filmId: string;
  /** Название фильма */
  title: string;
  /** URL постера фильма */
  posterUrl: string;
  /** Ключ постера в хранилище */
  posterKey: string;
  /** Год выпуска фильма */
  year: number;
  /** Рейтинг КиноПоиск */
  ratingKp: number | null;
  /** Рейтинг IMDB */
  ratingImdb: number | null;
  /** Описание фильма */
  description: string;
  /** Флаг сериала */
  isSerial: boolean;
  /** Имя владельца комнаты */
  userName: string;
  /** Жанры фильма */
  genres: string[];
  /** URL фотографии пользователя */
  photoUrl: string | null;
  /** Ключ фотографии пользователя*/
  photoKey: string | null;
}
