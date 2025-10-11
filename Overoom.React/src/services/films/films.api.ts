import { AxiosInstance } from 'axios';

import { SearchFilmsRequest } from './requests/search-films.request.ts';
import { FilmShortResponse } from './responses/film-short.response.ts';
import { FilmResponse } from './responses/film.response.ts';
import { CountResult } from '../common/count-result.ts';

/** Класс для работы с API фильмов */
export class FilmsApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /** Формат URL для генерации полных путей к постерам фильмов */
  private readonly posterUrlFormat: string;

  /**
   * Создает экземпляр FilmsApi
   * @param axiosInstance - экземпляр Axios для HTTP-запросов
   * @param posterUrlFormat - формат URL для постеров фильмов
   */
  constructor(axiosInstance: AxiosInstance, posterUrlFormat: string) {
    this.axiosInstance = axiosInstance;
    this.posterUrlFormat = posterUrlFormat;
  }

  /**
   * Выполняет поиск фильмов по заданным критериям
   * @param query - параметры поискового запроса
   * @returns Promise с результатом поиска, содержащим список фильмов и общее количество
   */
  public async search(query: SearchFilmsRequest): Promise<CountResult<FilmShortResponse>> {
    const response = await this.axiosInstance.get<CountResult<FilmShortResponse>>('films/search', {
      params: query,
    });

    // Добавляем URL постеров к фильмам
    for (const film of response.data.list) {
      film.posterUrl = this.posterUrlFormat + film.posterKey;
    }

    return response.data;
  }

  /**
   * Получает список популярных фильмов
   * @param take - количество фильмов для возврата (опционально)
   * @returns Promise со списком популярных фильмов
   */
  public async getPopular(take?: number): Promise<FilmShortResponse[]> {
    const response = await this.axiosInstance.get<FilmShortResponse[]>('films/popular', {
      params: { take: take },
    });

    // Добавляем URL постеров к фильмам
    for (const film of response.data) {
      film.posterUrl = this.posterUrlFormat + film.posterKey;
    }

    return response.data;
  }

  /**
   * Получает подробную информацию о фильме по идентификатору
   * @param id - идентификатор фильма
   * @returns Promise с подробной информацией о фильме
   */
  public async get(id: string): Promise<FilmResponse> {
    const response = await this.axiosInstance.get<FilmResponse>(`films/${id}`);
    response.data.posterUrl = this.posterUrlFormat + response.data.posterKey;
    return response.data;
  }

  /**
   * Устанавливает рейтинг для фильма
   * @param id - идентификатор фильма
   * @param score - оценка фильма
   * @returns Promise, который разрешается после установки рейтинга
   */
  public rateFilm(id: string, score: number): Promise<void> {
    return this.axiosInstance.post(`films/${id}/ratings`, { Score: score });
  }
}
