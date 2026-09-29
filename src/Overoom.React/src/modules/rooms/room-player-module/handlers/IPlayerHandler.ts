import PlayerEventContainer from './events/player-event.container.ts';

/** Интерфейс обработчика видеоплеера. */
export interface IPlayerHandler {
  /**
   * Добавляет обработчик событий плеера.
   * @param handler - Функция-обработчик, которая будет вызываться при событиях плеера.
   */
  addHandler(handler: (event: PlayerEventContainer) => void): void;

  /**
   * Удаляет ранее добавленный обработчик событий плеера.
   * @param handler - Функция-обработчик, которую необходимо удалить.
   */
  removeHandler(handler: (event: PlayerEventContainer) => void): void;

  /**
   * Инициализирует обработчик событий плеера.
   */
  mount(): void;

  /**
   * Очищает ресурсы обработчика событий плеера.
   */
  unmount(): void;

  /**
   * Устанавливает текущую позицию воспроизведения.
   * @param ticks - Позиция в тиках (100-наносекундных интервалах, как .NET TimeSpan.Ticks),
   * на которую нужно перемотать видео.
   */
  setTimeLine(ticks: number): void;

  /**
   * Устанавливает состояние паузы/воспроизведения.
   * @param pause - true, чтобы поставить на паузу, false, чтобы продолжить воспроизведение.
   */
  setPause(pause: boolean): void;

  /**
   * Устанавливает скорость воспроизведения видео.
   * @param speed - Скорость воспроизведения (1.0 - нормальная скорость).
   */
  setSpeed(speed: number): void;

  /**
   * Переключает на указанный эпизод (для сериалов и плейлистов).
   * @param season - Номер сезона (начинается с 1).
   * @param episode - Номер эпизода в сезоне (начинается с 1).
   */
  setEpisode(season: number, episode: number): void;
}
