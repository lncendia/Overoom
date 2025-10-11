import * as signalR from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr';

import { RoomEventContainer } from './events/room-event.container.ts';

/** Класс для работы с SignalR хабом комнаты */
export class RoomHub {
  /** Массив обработчиков событий комнаты */
  private readonly handlers: ((event: RoomEventContainer) => void)[];

  /** URL endpoint SignalR хаба */
  private readonly url: string;

  /** Экземпляр подключения SignalR */
  private connection?: HubConnection;

  /** Фабрика для получения JWT токена аутентификации */
  private readonly tokenFactory: () => Promise<string>;

  /** Формат URL для миниатюр пользовательских аватарок */
  private readonly thumbnailUrlFormat: string;

  /**
   * Конструктор класса RoomHub
   * @param url - URL endpoint SignalR хаба
   * @param tokenFactory - Функция для получения токена аутентификации
   * @param thumbnailUrlFormat - Формат URL для миниатюр пользователей
   */
  constructor(url: string, tokenFactory: () => Promise<string>, thumbnailUrlFormat: string) {
    this.handlers = [];
    this.url = url;
    this.tokenFactory = tokenFactory;
    this.thumbnailUrlFormat = thumbnailUrlFormat;
  }

  /**
   * Подключается к SignalR хабу комнаты
   * @returns Promise, который разрешается после успешного подключения
   */
  async start(): Promise<void> {
    // Создаем и настраиваем подключение к хабу
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.url, {
        accessTokenFactory: this.tokenFactory,
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Подписываемся на поток событий от сервера
    this.connection.on('Event', this.pushEvent.bind(this));

    // Запускаем подключение
    await this.connection.start();
  }

  /**
   * Обрабатывает входящие события и распространяет их по обработчикам
   * @param ev - Контейнер с данными события
   */
  private pushEvent(ev: RoomEventContainer): void {
    // Обрабатываем событие подключения - формируем URL аватарки
    if (ev.joinEvent) {
      if (ev.joinEvent.viewer.photoKey) {
        ev.joinEvent.viewer.photoUrl = this.thumbnailUrlFormat + ev.joinEvent.viewer.photoKey;
      }
    }
    // Обрабатываем событие комнаты - формируем URL аватарок всех пользователей
    else if (ev.roomEvent) {
      ev.roomEvent.room.viewers.forEach((v) => {
        if (v.photoKey) v.photoUrl = this.thumbnailUrlFormat + v.photoKey;
      });
    }

    // Передаем событие всем зарегистрированным обработчикам
    this.handlers.forEach((h) => h(ev));
  }

  /**
   * Добавляет обработчик событий комнаты
   * @param handler - Функция-обработчик событий
   */
  addHandler(handler: (event: RoomEventContainer) => void): void {
    this.handlers.push(handler);
  }

  /**
   * Удаляет обработчик событий комнаты
   * @param handler - Функция-обработчик для удаления
   */
  removeHandler(handler: (event: RoomEventContainer) => void): void {
    const index = this.handlers.indexOf(handler);
    if (index >= 0) this.handlers.splice(index, 1);
  }

  // Методы для отправки команд на сервер:

  /**
   * Подключается к комнате по идентификатору.
   * Отправляет сигнал на сервер о намерении подключиться к указанной комнате.
   * @param roomId - Идентификатор комнаты для подключения
   * @returns Promise, который разрешается после отправки запроса на сервер
   */
  async connect(roomId: string): Promise<void> {
    await this.connection?.send('Connect', roomId);
  }

  /**
   * Запрашивает данные текущей комнаты с сервера.
   * Используется для синхронизации состояния комнаты после подключения.
   * @returns Promise, который разрешается после отправки запроса на сервер
   */
  async getRoom(): Promise<void> {
    await this.connection?.send('GetRoom');
  }

  /**
   * Получает список сообщений чата комнаты.
   * Если указаны параметры, выборка начинается с конкретного сообщения и ограничивается указанным количеством.
   * @param fromId - Идентификатор сообщения, с которого начинать выборку (опционально)
   * @param count - Количество сообщений для получения (опционально)
   * @returns Promise, который разрешается после отправки запроса на сервер
   */
  async getMessages(fromId?: string, count?: number): Promise<void> {
    await this.connection?.send('GetMessages', fromId, count);
  }

  /**
   * Устанавливает текущий эпизод для просмотра
   * @param season - Номер сезона
   * @param series - Номер серии
   * @returns Promise, который разрешается после отправки команды
   */
  async setEpisode(season: number, series: number): Promise<void> {
    await this.connection?.send('SetEpisode', season, series);
  }

  /**
   * Отправляет текстовое сообщение в чат комнаты
   * @param message - Текст сообщения
   * @returns Promise, который разрешается после отправки сообщения
   */
  async sendMessage(message: string): Promise<void> {
    await this.connection?.send('SendMessage', message);
  }

  /**
   * Устанавливает позицию воспроизведения видео
   * @param ticks - Позиция в тиках
   * @returns Promise, который разрешается после отправки команды
   */
  async setTimeLine(ticks: number): Promise<void> {
    await this.connection?.send('SetTimeLine', ticks);
  }

  /**
   * Устанавливает состояние паузы/воспроизведения
   * @param pause - Флаг паузы (true - пауза, false - воспроизведение)
   * @param ticks - Текущая позиция в тиках
   * @param buffering - Флаг буферизации
   * @returns Promise, который разрешается после отправки команды
   */
  async setPause(pause: boolean, ticks: number, buffering: boolean): Promise<void> {
    await this.connection?.send('SetPause', pause, ticks, buffering);
  }

  /**
   * Устанавливает полноэкранный режим
   * @param fullScreen - Флаг полноэкранного режима
   * @returns Promise, который разрешается после отправки команды
   */
  async setFullScreen(fullScreen: boolean): Promise<void> {
    await this.connection?.send('SetFullScreen', fullScreen);
  }

  /**
   * Устанавливает скорость воспроизведения видео
   * @param speed - Скорость воспроизведения
   * @returns Promise, который разрешается после отправки команды
   */
  async setSpeed(speed: number): Promise<void> {
    await this.connection?.send('SetSpeed', speed);
  }

  /**
   * Устанавливает уровень громкости (заглушает/включает звук)
   * @param muted - Флаг заглушения звука (true - звук заглушен, false - звук включен)
   * @returns Promise, который разрешается после отправки команды
   */
  async setMuted(muted: boolean): Promise<void> {
    await this.connection?.send('SetMuted', muted);
  }

  /**
   * Отправляет звуковой сигнал "beep" указанному пользователю
   * @param target - Идентификатор целевого пользователя
   * @returns Promise, который разрешается после отправки сигнала
   */
  async beep(target: string): Promise<void> {
    await this.connection?.send('Beep', target);
  }

  /**
   * Отправляет звуковой сигнал "scream" указанному пользователю
   * @param target - Идентификатор целевого пользователя
   * @returns Promise, который разрешается после отправки сигнала
   */
  async scream(target: string): Promise<void> {
    await this.connection?.send('Scream', target);
  }

  /**
   * Уведомляет сервер о наборе текста пользователем
   * @returns Promise, который разрешается после отправки уведомления
   */
  async type(): Promise<void> {
    await this.connection?.send('Type');
  }

  /**
   * Синхронизирует состояние медиа-контента при подключении пользователя к комнате
   * @returns Promise, который разрешается после отправки запроса синхронизации
   */
  async sync(): Promise<void> {
    await this.connection?.send('Sync');
  }

  /**
   * Отключается от SignalR хаба комнаты
   * @returns Promise, который разрешается после отключения
   */
  async disconnect(): Promise<void> {
    await this.connection?.stop();
  }
}
