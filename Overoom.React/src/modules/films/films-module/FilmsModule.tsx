import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsCatalogSkeleton from '../../../components/films/films-catalog/FilmsCatalog.skeleton.tsx';
import FilmsCatalog from '../../../components/films/films-catalog/FilmsCatalog.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/** Пропсы компонента FilmsModule */
interface FilmsModuleProps {
  /** Жанр для фильтрации фильмов */
  genre?: string;
  /** Персона для фильтрации фильмов */
  person?: string;
  /** Страна для фильтрации фильмов */
  country?: string;
  /** Флаг, указывающий на сериалы */
  serial?: boolean;
  /** Идентификатор плейлиста */
  playlistId?: string;
  /** Год выпуска фильма */
  year?: number;
}

/**
 * Компонент для отображения каталога фильмов с поддержкой фильтров и пагинации.
 * @param props - Параметры фильтрации и отображения фильмов
 * @returns {ReactElement} JSX-элемент каталога фильмов
 */
const FilmsModule = (props: FilmsModuleProps): ReactElement => {
  /** Сервис для работы с API фильмов */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Функция поиска фильмов с учетом фильтров и пагинации
   * @param skip - количество пропускаемых фильмов
   * @param take - количество загружаемых фильмов
   * @returns Promise с результатами поиска фильмов
   */
  const fetch = useCallback(
    (skip: number, take: number) => {
      return filmsApi.search({
        ...props,
        minYear: props.year,
        maxYear: props.year,
        skip: skip,
        take: take,
      });
    },
    [props, filmsApi]
  );

  /** Хук для порционно-загружаемых фильмов */
  const {
    items: films,
    isLoading,
    hasMore,
    fetchMore,
  } = usePaginatedFetch<FilmShortResponse>(fetch, 20);

  /**
   * Обработчик выбора фильма из каталога
   * @param id - ID выбранного фильма
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/film', { state: { id: id } });
    },
    [navigate]
  );

  // Показ скелетона, если данные ещё загружаются
  if (isLoading) return <FilmsCatalogSkeleton />;

  // Показ сообщения "Нет данных", если фильмы не найдены
  if (films.length === 0) return <NoData text="Фильмы не найдены" />;

  // Основной рендер: компонент каталога фильмов
  return (
    <FilmsCatalog
      hasMore={hasMore}
      next={fetchMore}
      genre={props.genre}
      films={films}
      onSelect={onSelect}
      typeSelected={props.serial !== undefined}
    />
  );
};

export default FilmsModule;
