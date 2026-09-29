/** Модель данных для события выхода пользователя из комнаты */
export default interface LeaveEvent {
  /** Идентификатор пользователя, который вышел */
  viewer: string;
}
