import { RoomHub } from './room.hub.ts';

/** Фабрика для создания экземпляров RoomHub */
export class RoomHubFactory {
  /** Функция для получения JWT токена аутентификации */
  private readonly tokenFactory: () => Promise<string>;

  /** URL endpoint SignalR хаба */
  private readonly url: string;

  /** Формат URL для миниатюр пользовательских аватарок */
  private readonly thumbnailUrlFormat: string;

  /**
   * Создает экземпляр RoomHubFactory
   * @param tokenFactory - функция для получения JWT токена аутентификации
   * @param url - URL endpoint SignalR хаба
   * @param thumbnailUrlFormat - формат URL для миниатюр пользовательских аватарок
   */
  constructor(tokenFactory: () => Promise<string>, url: string, thumbnailUrlFormat: string) {
    this.tokenFactory = tokenFactory;
    this.url = url;
    this.thumbnailUrlFormat = thumbnailUrlFormat;
  }

  /**
   * Создает новый экземпляр RoomHub
   * @returns Экземпляр RoomHub с настроенными параметрами
   */
  create(): RoomHub {
    return new RoomHub(this.url, this.tokenFactory, this.thumbnailUrlFormat);
  }
}
