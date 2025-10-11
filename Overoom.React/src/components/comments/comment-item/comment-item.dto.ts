/** DTO элемента комментария */
export interface CommentItemDto {
  /** Идентификатор комментария */
  id: string;

  /** Имя пользователя */
  userName: string;

  /** Текст комментария */
  text: string;

  /** URL фотографии пользователя */
  photoUrl: string | null;

  /** Время создания */
  createdAt: Date;

  /** Флаг принадлежности текущему пользователю */
  isUserComment: boolean;
}
