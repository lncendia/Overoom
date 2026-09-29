/** Ответ с данными комнаты */
export interface RoomResponse {
  /** Идентификатор комнаты */
  id: string;
  /** Идентификатор фильма */
  filmId: string;
  /** Количество зрителей в комнате */
  viewersCount: number;
  /** Флаг приватности комнаты */
  isPrivate: boolean;
  /** Флаг присутствия пользователя в комнате */
  isUserIn: boolean;
}
