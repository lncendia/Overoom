/** Реакция зрителя на сообщение */
export default interface MessageReactionResponse {
  /** Идентификатор зрителя */
  userId: string;
  /** Код реакции */
  reaction: string;
}
