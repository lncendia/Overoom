/** Фильтры страницы поиска, передаются через query-параметры */
export interface SearchFilters {
  /** Жанр */
  genre?: string;
  /** Участник съемок */
  person?: string;
  /** Страна */
  country?: string;
  /** Год выпуска */
  year?: number;
  /** true — только сериалы, false — только фильмы */
  serial?: boolean;
}

/** Построители адресов страниц приложения */
export const routes = {
  home: '/',
  catalog: '/catalog',
  playlists: '/playlists',
  profile: '/profile',
  rooms: '/rooms',
  adminJobs: '/admin/jobs',

  /**
   * Страница фильма
   * @param id - Идентификатор фильма
   * @returns {string} Адрес страницы
   */
  film: (id: string): string => `/film/${encodeURIComponent(id)}`,

  /**
   * Страница подборки
   * @param id - Идентификатор подборки
   * @returns {string} Адрес страницы
   */
  playlist: (id: string): string => `/playlist/${encodeURIComponent(id)}`,

  /**
   * Страница комнаты
   * @param id - Идентификатор комнаты
   * @param code - Код доступа к приватной комнате
   * @returns {string} Адрес страницы
   */
  room: (id: string, code?: string | null): string =>
    `/room/${encodeURIComponent(id)}${code ? `?code=${encodeURIComponent(code)}` : ''}`,

  /**
   * Страница поиска по фильтрам
   * @param filters - Фильтры поиска
   * @returns {string} Адрес страницы
   */
  search: (filters: SearchFilters): string => {
    const params = new URLSearchParams();
    if (filters.genre) params.set('genre', filters.genre);
    if (filters.person) params.set('person', filters.person);
    if (filters.country) params.set('country', filters.country);
    if (filters.year !== undefined) params.set('year', String(filters.year));
    if (filters.serial !== undefined) params.set('serial', String(filters.serial));
    return `/search?${params.toString()}`;
  },
};

/**
 * Читает фильтры поиска из query-параметров
 * @param params - Query-параметры адреса
 * @returns {SearchFilters} Фильтры поиска
 */
export const parseSearchFilters = (params: URLSearchParams): SearchFilters => {
  const year = Number(params.get('year'));
  const serial = params.get('serial');
  return {
    genre: params.get('genre') ?? undefined,
    person: params.get('person') ?? undefined,
    country: params.get('country') ?? undefined,
    year: Number.isInteger(year) && year > 0 ? year : undefined,
    serial: serial === 'true' ? true : serial === 'false' ? false : undefined,
  };
};
