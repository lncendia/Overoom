/** Данные сообщения в чате комнаты */
export default interface MessageResponse {
  /** Идентификатор сообщения */
  id: string;
  /** Идентификатор пользователя */
  userId: string;
  /** Текст сообщения */
  text: string;
  /** Время отправки сообщения */
  sentAt: string;
}
