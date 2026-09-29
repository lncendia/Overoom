import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsCatalogSkeleton from '../../../components/films/films-catalog/FilmsCatalog.skeleton.tsx';
import PlaylistsCatalog from '../../../components/playlists/playlists-list/PlaylistsCatalog.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { PlaylistsApi } from '../../../services/playlists/playlists.api.ts';
import { PlaylistResponse } from '../../../services/playlists/responses/playlist.response.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/** Пропсы компонента PlaylistsModule */
interface PlaylistsModuleProps {
  /** Жанр для фильтрации подборок (опционально) */
  genre?: string;
}

/**
 * Компонент для отображения каталога подборок с возможностью фильтрации по жанру
 * и пагинацией. Обрабатывает загрузку данных, отображение состояний загрузки и пустого списка,
 * а также навигацию к детальной странице подборки.
 * @param props - Свойства компонента
 * @param props.genre - Жанр для фильтрации подборок
 * @returns {ReactElement} JSX элемент модуля каталога подборок
 */
const PlaylistsModule = ({ genre }: PlaylistsModuleProps): ReactElement => {
  /** Сервис для работы с API подборок */
  const playlistsApi = useInjection<PlaylistsApi>('PlaylistsApi');

  /**
   * Функция поиска подборок по заданным параметрам с пагинацией
   * @param skip - Количество пропускаемых элементов
   * @param take - Количество загружаемых элементов
   * @returns {Promise<PlaylistResponse[]>} Promise с массивом подборок
   */
  const fetch = useCallback(
    (skip: number, take: number) => {
      return playlistsApi.search({
        genre: genre,
        skip: skip,
        take: take,
      });
    },
    [genre, playlistsApi]
  );

  /** Хук для порционной загрузки подборок с пагинацией */
  const {
    items: playlists,
    isLoading,
    hasMore,
    fetchMore,
    error,
    retry,
  } = usePaginatedFetch<PlaylistResponse>(fetch, 20);

  if (isLoading) return <FilmsCatalogSkeleton />;
  if (error && playlists.length === 0) return <LoadError onRetry={retry} />;
  if (playlists.length === 0) return <NoData text="Подборки не найдены" />;

  return (
    <>
      <PlaylistsCatalog
        hasMore={hasMore}
        next={fetchMore}
        genre={genre}
        playlists={playlists}
        getLink={routes.playlist}
      />
      {error && <LoadError onRetry={retry} />}
    </>
  );
};

export default PlaylistsModule;
