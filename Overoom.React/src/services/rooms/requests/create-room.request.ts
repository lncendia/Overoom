/** Запрос на создание новой комнаты */
export interface CreateRoomRequest {
  /** Флаг открытости комнаты (true - публичная, false - приватная) */
  open: boolean;
  /** Идентификатор фильма для просмотра в комнате */
  filmId: string;
}
