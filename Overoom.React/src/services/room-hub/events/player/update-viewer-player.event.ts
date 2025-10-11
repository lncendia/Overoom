/** Модель данных для события паузы */
export default interface UpdateViewerPlayerEvent {
  /** Идентификатор комнаты */
  id: string;

  /** Флаг паузы */
  onPause: boolean | null;

  /** Флаг полноэкранного режима */
  fullScreen: boolean | null;

  /** Позиция на временной шкале */
  timeLine: number | null;

  /** Скорость воспроизведения */
  speed: number | null;

  /** Номер сезона */
  season: number | null;

  /** Номер эпизода */
  episode: number | null;

  /** Список обновленных полей */
  updatedFields: string[];
}
