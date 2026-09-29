/** Результат получения элементов. */
export interface CountResult<T> {
  /** Коллекция элементов. */
  list: T[];

  /** Общее количество элементов. */
  totalCount: number;
}
