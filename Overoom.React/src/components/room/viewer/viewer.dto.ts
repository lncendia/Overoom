import { ViewerTagDto } from './viewer-tag.dto.ts';

/** Интерфейс зрителя в комнате просмотра */
export interface ViewerDto {
  /** Уникальный идентификатор зрителя */
  id: string;
  /** Имя пользователя */
  userName: string;
  /** URL аватара пользователя */
  photoUrl: string | null;
  /** Флаг паузы воспроизведения */
  onPause: boolean;
  /** Флаг полноэкранного режима */
  fullScreen: boolean;
  /** Флаг онлайн-статуса */
  online: boolean;
  /** Текущая позиция воспроизведения */
  timeLine: number;
  /** Флаг возможности отправки звукового сигнала */
  canBeep: boolean;
  /** Флаг возможности отправки крика */
  canScream: boolean;
  /** Флаг возможности исключения из комнаты */
  canKick: boolean;
  /** Флаг возможности синхронизации */
  canSync: boolean;
  /** Номер сезона */
  season: number | null;
  /** Номер серии */
  episode: number | null;
  /** Флаг владельца комнаты */
  isOwner: boolean;
  /** Флаг текущего пользователя */
  isCurrent: boolean;
  /** Теги зрителя */
  tags: ViewerTagDto[];
  /** Флаг набора сообщения */
  typing: boolean;
}
