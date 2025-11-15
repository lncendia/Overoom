import { AxiosInstance } from 'axios';

import { JoinRoomRequest } from './requests/join-room.request.ts';
import { CountResult } from '../common/count-result.ts';
import { CreateRoomRequest } from './requests/create-room.request.ts';
import { SearchRoomsRequest } from './requests/search-rooms.request.ts';
import { RoomShortResponse } from './responses/room-short.response.ts';
import { RoomResponse } from './responses/room.response.ts';

/** Класс для работы с API комнат */
export class RoomsApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /** Формат URL для генерации полных путей к постерам фильмов */
  private readonly posterUrlFormat: string;

  /** Формат URL для миниатюр пользовательских аватарок */
  private readonly thumbnailUrlFormat: string;

  /**
   * Создает экземпляр RoomsApi
   * @param axiosInstance - экземпляр Axios для HTTP-запросов
   * @param posterUrlFormat - формат URL для постеров фильмов
   * @param thumbnailUrlFormat - формат URL для миниатюр изображений профиля
   */
  constructor(axiosInstance: AxiosInstance, posterUrlFormat: string, thumbnailUrlFormat: string) {
    this.axiosInstance = axiosInstance;
    this.posterUrlFormat = posterUrlFormat;
    this.thumbnailUrlFormat = thumbnailUrlFormat;
  }

  /**
   * Получает информацию о комнате по идентификатору
   * @param id - идентификатор комнаты
   * @returns Promise с информацией о комнате
   */
  async get(id: string): Promise<RoomResponse> {
    const response = await this.axiosInstance.get<RoomResponse>(`rooms/${id}`);
    return response.data;
  }

  /**
   * Получает код подключения к комнате
   * @param id - идентификатор комнаты
   * @returns Promise с информацией о комнате
   */
  async getCode(id: string): Promise<string | null> {
    const response = await this.axiosInstance.get<string | null>(`rooms/${id}/code`);
    return response.data;
  }

  /**
   * Выполняет поиск комнат по заданным критериям
   * @param query - параметры поискового запроса
   * @returns Promise с результатом поиска, содержащим список комнат и общее количество
   */
  async search(query: SearchRoomsRequest): Promise<CountResult<RoomShortResponse>> {
    const response = await this.axiosInstance.get<CountResult<RoomShortResponse>>('rooms', {
      params: query,
    });

    // Добавляем URL постеров к комнатам
    for (const room of response.data.list) {
      room.posterUrl = this.posterUrlFormat + room.posterKey;
    }

    // Добавляем URL аватарок к комнатам
    for (const room of response.data.list) {
      room.photoUrl = this.thumbnailUrlFormat + room.photoKey;
    }

    return response.data;
  }

  /**
   * Получает список комнат текущего пользователя
   * @returns Promise со списком комнат пользователя
   */
  async getMy(): Promise<RoomShortResponse[]> {
    const response = await this.axiosInstance.get<RoomShortResponse[]>('rooms/my');

    // Добавляем URL постеров к комнатам
    for (const room of response.data) {
      room.posterUrl = this.posterUrlFormat + room.posterKey;
    }

    // Добавляем URL аватарок к комнатам
    for (const room of response.data) {
      room.photoUrl = this.thumbnailUrlFormat + room.photoKey;
    }

    return response.data;
  }

  /**
   * Создает новую комнату
   * @param body - параметры для создания комнаты
   * @returns Promise с идентификатором созданной комнаты
   */
  public async create(body: CreateRoomRequest): Promise<string> {
    const response = await this.axiosInstance.post('rooms', body);
    return response.data;
  }

  /**
   * Подключается к существующей комнате
   * @param id - идентификатор комнаты
   * @param body - параметры для подключения к комнате
   * @returns Promise с идентификатором сессии подключения
   */
  async join(id: string, body: JoinRoomRequest): Promise<string> {
    const response = await this.axiosInstance.post(`rooms/${id}/join`, body);
    return response.data;
  }

  /**
   * Покинуть комнату
   * @param id - идентификатор комнаты, которую нужно покинуть
   * @returns Promise с результатом операции выхода из комнаты
   */
  async leave(id: string): Promise<string> {
    const response = await this.axiosInstance.post(`rooms/${id}/leave`);
    return response.data;
  }

  /**
   * Исключить пользователя из комнаты
   * @param id - идентификатор комнаты
   * @param targetId - идентификатор пользователя, которого нужно исключить
   * @returns Promise с результатом операции исключения пользователя
   */
  async kick(id: string, targetId: string): Promise<string> {
    const response = await this.axiosInstance.post(`rooms/${id}/kick/${targetId}`);
    return response.data;
  }

  /**
   * Удалить комнату
   * @param id - идентификатор комнаты, которую нужно удалить
   * @returns Promise с результатом операции удаления комнаты
   */
  async delete(id: string): Promise<string> {
    const response = await this.axiosInstance.delete(`rooms/${id}`);
    return response.data;
  }
}
