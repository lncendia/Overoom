import { AxiosInstance } from 'axios';

import { SearchPlaylistsRequest } from './requests/search-playlists.request.ts';
import { PlaylistResponse } from './responses/playlist.response.ts';
import { CountResult } from '../common/count-result.ts';

/** Класс для работы с API плейлистов */
export class PlaylistsApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /** Формат URL для генерации полных путей к постерам фильмов */
  private readonly posterUrlFormat: string;

  /**
   * Создает экземпляр PlaylistsApi
   * @param axiosInstance - экземпляр Axios для HTTP-запросов
   * @param posterUrlFormat - формат URL для постеров плейлистов
   */
  constructor(axiosInstance: AxiosInstance, posterUrlFormat: string) {
    this.axiosInstance = axiosInstance;
    this.posterUrlFormat = posterUrlFormat;
  }

  /**
   * Получает информацию о плейлисте по идентификатору
   * @param id - идентификатор плейлиста
   * @returns Promise с информацией о плейлисте
   */
  async get(id: string): Promise<PlaylistResponse> {
    const response = await this.axiosInstance.get<PlaylistResponse>(`playlists/${id}`);
    response.data.posterUrl = this.posterUrlFormat + response.data.posterKey;
    return response.data;
  }

  /**
   * Выполняет поиск плейлистов по заданным критериям
   * @param query - параметры поискового запроса
   * @returns Promise с результатом поиска, содержащим список плейлистов и общее количество
   */
  public async search(query: SearchPlaylistsRequest): Promise<CountResult<PlaylistResponse>> {
    const response = await this.axiosInstance.get<CountResult<PlaylistResponse>>('playlists', {
      params: query,
    });

    // Добавляем URL постеров к плейлистам
    for (const playlist of response.data.list) {
      playlist.posterUrl = this.posterUrlFormat + playlist.posterKey;
    }

    return response.data;
  }
}
