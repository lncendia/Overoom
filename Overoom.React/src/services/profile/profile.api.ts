import { AxiosInstance } from 'axios';

import { CountResult } from '../common/count-result.ts';
import { GetRatingsRequest } from './requests/get-ratings.request.ts';
import { UpdateRoomSettings } from './requests/update-room.settings.ts';
import { ProfileResponse } from './responses/profile.response.ts';
import { RatingResponse } from './responses/rating.response.ts';
import { FilmShortResponse } from '../films/responses/film-short.response.ts';

/** Класс для работы с API профиля пользователя */
export class ProfileApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /** Формат URL для генерации полных путей к постерам фильмов */
  private readonly posterUrlFormat: string;

  /** Формат URL для миниатюр пользовательских аватарок */
  private readonly thumbnailUrlFormat: string;

  /**
   * Создает экземпляр ProfileApi
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
   * Получает информацию о профиле текущего пользователя
   * @returns Promise с информацией о профиле пользователя
   */
  async getProfile(): Promise<ProfileResponse> {
    const response = await this.axiosInstance.get<ProfileResponse>('profile');
    if (response.data.photoKey)
      response.data.photoUrl = this.thumbnailUrlFormat + response.data.photoKey;
    return response.data;
  }

  /**
   * Получает список рейтингов пользователя
   * @param query - параметры запроса (пагинация, фильтрация)
   * @returns Promise с результатом запроса, содержащим список рейтингов и общее количество
   */
  async getRatings(query: GetRatingsRequest): Promise<CountResult<RatingResponse>> {
    const response = await this.axiosInstance.get<CountResult<RatingResponse>>('profile/ratings', {
      params: query,
    });

    // Добавляем URL постеров к фильмам
    for (const film of response.data.list) {
      film.posterUrl = this.posterUrlFormat + film.posterKey;
    }

    return response.data;
  }

  /**
   * Получает историю просмотров пользователя
   * @returns Promise со списком фильмов из истории просмотров
   */
  async getHistory(): Promise<FilmShortResponse[]> {
    const response = await this.axiosInstance.get<FilmShortResponse[]>('profile/history');
    for (const film of response.data) {
      film.posterUrl = this.posterUrlFormat + film.posterKey;
    }
    return response.data;
  }

  /**
   * Получает watchlist пользователя
   * @returns Promise со списком фильмов из watchlist
   */
  async getWatchlist(): Promise<FilmShortResponse[]> {
    const response = await this.axiosInstance.get<FilmShortResponse[]>('profile/watchlist');
    for (const film of response.data) {
      film.posterUrl = this.posterUrlFormat + film.posterKey;
    }
    return response.data;
  }

  /**
   * Добавляет фильм в историю просмотров
   * @param filmId - идентификатор фильма
   * @returns Promise, который разрешается после добавления фильма в историю
   */
  async addToHistory(filmId: string): Promise<void> {
    await this.axiosInstance.post(`profile/history/${filmId}`);
  }

  /**
   * Переключает состояние фильма в watchlist (добавляет/удаляет)
   * @param filmId - идентификатор фильма
   * @returns Promise, который разрешается после изменения watchlist
   */
  async toggleWatchlist(filmId: string): Promise<void> {
    await this.axiosInstance.post(`profile/watchlist/${filmId}`);
  }

  /**
   * Обновляет настройки уведомлений пользователя
   * @param body - новые настройки уведомлений
   * @returns Promise, который разрешается после обновления настроек
   */
  async updateRoomSettings(body: UpdateRoomSettings): Promise<void> {
    await this.axiosInstance.put('profile/settings', body);
  }
}
