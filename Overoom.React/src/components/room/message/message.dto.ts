/** Интерфейс сообщения в чате */
export interface MessageDto {
  /** Уникальный идентификатор сообщения */
  id: string;
  /** Текст сообщения */
  text: string;
  /** Имя пользователя отправителя */
  userName: string;
  /** Дата и время отправки сообщения */
  sentAt: Date;
  /** URL аватара пользователя */
  photoUrl: string | null;
  /** Флаг исходящего сообщения */
  isOutgoing: boolean;
  /** Флаг владельца сообщения */
  isOwner: boolean;
}
