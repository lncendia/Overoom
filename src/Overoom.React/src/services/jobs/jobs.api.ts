import { AxiosInstance } from 'axios';

import { SubmitJobRequest } from './requests/submit-job.request.ts';
import { JobResponse } from './responses/job.response.ts';
import { CountResult } from '../common/count-result.ts';

/** Класс для работы с API фоновых задач сервиса загрузки фильмов */
export class JobsApi {
  /** Экземпляр Axios для выполнения HTTP-запросов к API */
  private readonly axiosInstance: AxiosInstance;

  /**
   * Создает экземпляр JobsApi
   * @param axiosInstance - экземпляр Axios для HTTP-запросов к сервису загрузки
   */
  constructor(axiosInstance: AxiosInstance) {
    this.axiosInstance = axiosInstance;
  }

  /**
   * Получает страницу задач, новые сначала
   * @param skip - количество пропускаемых задач
   * @param take - количество возвращаемых задач
   * @returns Promise со списком задач и их общим количеством
   */
  async get(skip: number, take: number): Promise<CountResult<JobResponse>> {
    const response = await this.axiosInstance.get<CountResult<JobResponse>>('jobs', {
      params: { skip, take },
    });
    return response.data;
  }

  /**
   * Ставит задачу загрузки фильма в очередь
   * @param request - данные задачи
   * @returns Promise с идентификатором задачи
   */
  async submit(request: SubmitJobRequest): Promise<string> {
    const response = await this.axiosInstance.post<{ id: string }>('queue', request);
    return response.data.id;
  }

  /**
   * Отменяет задачу
   * @param id - идентификатор задачи
   * @returns Promise, который разрешается после отправки команды
   */
  async cancel(id: string): Promise<void> {
    await this.axiosInstance.post(`jobs/${id}/cancel`);
  }

  /**
   * Повторно запускает упавшую или отменённую задачу
   * @param id - идентификатор задачи
   * @returns Promise, который разрешается после отправки команды
   */
  async retry(id: string): Promise<void> {
    await this.axiosInstance.post(`jobs/${id}/retry`);
  }

  /**
   * Удаляет задачу
   * @param id - идентификатор задачи
   * @returns Promise, который разрешается после отправки команды
   */
  async delete(id: string): Promise<void> {
    await this.axiosInstance.delete(`jobs/${id}`);
  }
}
