/** Модель данных события установки или снятия реакции на сообщение */
export default interface MessageReactionEvent {
  /** Идентификатор сообщения */
  messageId: string;
  /** Идентификатор зрителя */
  viewerId: string;
  /** Код реакции */
  reaction: string;
  /** true, если реакция поставлена, false, если снята */
  added: boolean;
}
