import PlayerEventContainer from './events/player-event.container.ts';
import { IPlayerHandler } from './IPlayerHandler.ts';

/**
 * Константа, определяющая количество тиков (100-наносекундных интервалов) в одной секунде.
 * Соответствует .NET TicksPerSecond (10,000,000 тиков в секунду).
 */
const TICKS_PER_SECOND = 10_000_000;

/**
 * Обработчик событий плеера, реализующий интерфейс IPlayerHandler.
 */
export class PlayerJsHandler implements IPlayerHandler {
  /** Идентификатор плеера, используемый для фильтрации событий */
  private readonly playerId: string;

  /**
   * Конструктор класса
   * @param playerId Уникальный идентификатор плеера
   */
  constructor(playerId: string) {
    this.playerId = playerId;
  }

  /** Массив обработчиков событий плеера */
  private handlers: Array<(event: PlayerEventContainer) => void> = [];

  /** Инициализация обработчика событий плеера. */
  mount() {
    // Устанавливаем глобальную функцию для обработки событий от Player.js
    window.PlayerjsEvents = this.handler.bind(this);
  }

  /** Очистка обработчика событий при демонтаже компонента. */
  unmount() {
    // Убираем глобальную функцию, чтобы избежать утечек памяти
    window.PlayerjsEvents = undefined;

    // Очищаем массив обработчиков
    this.handlers = [];
  }

  /**
   * Основной обработчик событий от Player.js
   * @param event Тип события (play, pause, seek, etc.)
   * @param id Идентификатор плеера, от которого пришло событие
   * @param info Дополнительная информация о событии
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private handler(event: string, id: string, info: any) {
    // Игнорируем события от других плееров
    if (id != this.playerId) return;

    console.log(`Received Player.js event:`, event, info);

    // Создаем контейнер для события
    const ev: PlayerEventContainer = {};

    // Обрабатываем различные типы событий
    if (event === 'play' || event === 'buffered') {
      // События воспроизведения: играть или буферизация завершена
      const time = toTicksFromSeconds(window.pljssglobal[0].api('time'));
      ev.pauseEvent = {
        onPause: false,
        ticks: time,
        buffering: event === 'buffered',
      };
    } else if (event === 'pause' || event === 'buffering') {
      // События паузы: пауза или активная буферизация
      const time = toTicksFromSeconds(window.pljssglobal[0].api('time'));
      ev.pauseEvent = {
        onPause: true,
        ticks: time,
        buffering: event === 'buffering',
      };
    } else if (event === 'seek') {
      // Событие перемотки
      ev.seekEvent = { ticks: toTicksFromSeconds(info) };
    } else if (event === 'fullscreen') {
      // Вход в полноэкранный режим
      ev.fullscreenEvent = { fullscreen: true };
    } else if (event === 'exitfullscreen') {
      // Выход из полноэкранного режима
      ev.fullscreenEvent = { fullscreen: false };
    } else if (event === 'speed') {
      // Изменение скорости воспроизведения
      ev.speedEvent = { speed: parseFloat(info as string) };
    } else if (event === 'mute') {
      // Изменение громкости
      ev.muteEvent = { muted: true };
    } else if (event === 'unmute') {
      // Изменение громкости
      ev.muteEvent = { muted: false };
    } else if (event === 'new') {
      // Берём текущий id
      const id = window.pljssglobal[0].api('playlist_id');

      // Регулярка для вида s<season>e<episode>
      const match = /^s(\d+)e(\d+)/i.exec(id);

      if (match) {
        const season = parseInt(match[1], 10);
        const episode = parseInt(match[2], 10);

        ev.changeEpisodeEvent = {
          season,
          episode,
        };
      }
    } else if (event === 'init') {
      // Выход из полноэкранного режима
      ev.initEvent = {};
    } else {
      // Неизвестное событие - игнорируем
      return;
    }

    // Передаем событие всем зарегистрированным обработчикам
    this.pushEvent(ev);
  }

  /**
   * Распространение события всем зарегистрированным обработчикам
   * @param ev Контейнер с данными события
   */
  private pushEvent(ev: PlayerEventContainer) {
    this.handlers.forEach((handler) => {
      try {
        handler(ev);
      } catch (error) {
        console.error('Error in player event handler:', error);
      }
    });
  }

  /**
   * Добавление обработчика событий плеера
   * @param handler Функция-обработчик событий
   */
  addHandler(handler: (event: PlayerEventContainer) => void) {
    this.handlers.push(handler);
  }

  /**
   * Удаление обработчика событий плеера
   * @param handler Функция-обработчик для удаления
   */
  removeHandler(handler: (event: PlayerEventContainer) => void) {
    const index = this.handlers.indexOf(handler);
    if (index >= 0) {
      this.handlers.splice(index, 1);
    }
  }

  /**
   * Установка позиции воспроизведения
   * @param ticks Позиция в тиках (100-наносекундных интервалах)
   */
  setTimeLine(ticks: number): void {
    window.pljssglobal[0].api('seek', toSecondsFromTicks(ticks));
  }

  /**
   * Установка состояния паузы/воспроизведения
   * @param pause true - поставить на паузу, false - продолжить воспроизведение
   */
  setPause(pause: boolean): void {
    const message = pause ? 'pause' : 'play';
    window.pljssglobal[0].api(message);
  }

  /**
   * Переключение на другой эпизод
   * @param season Номер сезона
   * @param episode Номер эпизода
   */
  setEpisode(season: number, episode: number) {
    // Берём текущий id
    const currentId = window.pljssglobal[0].api('playlist_id');

    // Отделяем озвучку после подчёркивания
    const parts = currentId.split('_');
    const voice = parts.length > 1 ? parts[1] : '';

    // Собираем новый id
    const newId = `id:s${season}e${episode}${voice ? '_' + voice : ''}`;

    // Переключаем на него
    window.pljssglobal[0].api('play', newId);
  }

  /**
   * Установка скорости воспроизведения
   * @param speed Скорость воспроизведения (1.0 - нормальная скорость)
   */
  setSpeed(speed: number): void {
    let variant = speed / 0.25 - 1;
    if (speed === 2) variant -= 1;
    window.pljssglobal[0].api('speed', variant);
  }
}

/**
 * Конвертирует секунды в .NET Ticks (100-наносекундные интервалы)
 * @param seconds Количество секунд
 * @returns Количество тиков (целое число)
 */
function toTicksFromSeconds(seconds: number): number {
  return Math.round(seconds * TICKS_PER_SECOND);
}

/**
 * Конвертирует .NET Ticks обратно в секунды
 * @param ticks Количество тиков
 * @returns Количество секунд (дробное число)
 */
function toSecondsFromTicks(ticks: number): number {
  return ticks / TICKS_PER_SECOND;
}
