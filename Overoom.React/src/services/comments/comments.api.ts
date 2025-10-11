import { AxiosInstance } from 'axios';

import { GetCommentsRequest } from './requests/get-comments.request.ts';
import { CommentResponse } from './responses/comment.response.ts';
import { CountResult } from '../common/count-result.ts';

/** Класс для работы с API комментариев */
export class CommentsApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /** Формат URL для миниатюр пользовательских аватарок */
  private readonly thumbnailUrlFormat: string;

  /**
   * Создает экземпляр CommentsApi
   * @param axiosInstance - экземпляр Axios для HTTP-запросов
   * @param thumbnailUrlFormat - формат URL для миниатюр изображений
   */
  constructor(axiosInstance: AxiosInstance, thumbnailUrlFormat: string) {
    this.axiosInstance = axiosInstance;
    this.thumbnailUrlFormat = thumbnailUrlFormat;
  }

  /**
   * Получает список комментариев для указанного фильма
   * @param filmId - идентификатор фильма
   * @param query - параметры запроса (пагинация, фильтрация)
   * @returns Promise с результатом запроса, содержащим список комментариев и общее количество
   */
  async get(filmId: string, query: GetCommentsRequest): Promise<CountResult<CommentResponse>> {
    const response = await this.axiosInstance.get<CountResult<CommentResponse>>(
      `/films/${filmId}/comments`,
      { params: query }
    );

    // Добавляем URL фотографий к комментариям
    response.data.list.forEach((c) => {
      if (c.photoKey) c.photoUrl = this.thumbnailUrlFormat + c.photoKey;
    });

    return response.data;
  }

  /**
   * Удаляет комментарий
   * @param filmId - идентификатор фильма
   * @param id - идентификатор комментария
   * @returns Promise, который разрешается после удаления комментария
   */
  delete(filmId: string, id: string): Promise<void> {
    return this.axiosInstance.delete(`/films/${filmId}/comments/${id}`);
  }

  /**
   * Добавляет новый комментарий к фильму
   * @param filmId - идентификатор фильма
   * @param text - текст комментария
   * @returns Promise с идентификатором созданного комментария
   */
  async add(filmId: string, text: string): Promise<string> {
    const response = await this.axiosInstance.post(`/films/${filmId}/comments`, { text: text });
    return response.data.id;
  }
}
