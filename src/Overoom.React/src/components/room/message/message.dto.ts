/** Сводка по одной реакции на сообщение */
export interface MessageReactionDto {
  /** Код реакции */
  code: string;
  /** Эмодзи реакции */
  emoji: string;
  /** Количество зрителей, поставивших реакцию */
  count: number;
  /** Флаг: реакцию поставил текущий зритель */
  reacted: boolean;
  /** Имена зрителей, поставивших реакцию */
  userNames: string[];
}

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
  /** Реакции на сообщение (только поставленные, в порядке отображения) */
  reactions: MessageReactionDto[];
}
