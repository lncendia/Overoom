/** Сводка по одной реакции на комментарий */
export interface CommentReactionResponse {
  /** Код реакции */
  reaction: string;
  /** Количество пользователей, поставивших реакцию */
  count: number;
  /** Поставил ли реакцию текущий пользователь */
  reacted: boolean;
}

/** Данные комментария к фильму */
export interface CommentResponse {
  /** Идентификатор комментария */
  id: string;
  /** Имя пользователя */
  userName: string;
  /** Текст комментария */
  text: string;
  /** Ключ фотографии пользователя (опционально) */
  photoKey: string | null;
  /** URL фотографии пользователя (опционально) */
  photoUrl: string | null;
  /** Время создания комментария */
  createdAt: string;
  /** Идентификатор пользователя */
  userId: string;
  /** Реакции на комментарий */
  reactions: CommentReactionResponse[];
}
