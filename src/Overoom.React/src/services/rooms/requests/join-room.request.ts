/** Запрос на подключение к комнате */
export interface JoinRoomRequest {
  /** Код доступа для приватных комнат (опционально) */
  code?: string;
}
