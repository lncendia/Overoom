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
}
