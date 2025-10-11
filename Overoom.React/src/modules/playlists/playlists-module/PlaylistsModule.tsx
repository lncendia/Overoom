import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsCatalogSkeleton from '../../../components/films/films-catalog/FilmsCatalog.skeleton.tsx';
import PlaylistsCatalog from '../../../components/playlists/playlists-list/PlaylistsCatalog.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { PlaylistsApi } from '../../../services/playlists/playlists.api.ts';
import { PlaylistResponse } from '../../../services/playlists/responses/playlist.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Пропсы компонента PlaylistsModule
 * @property genre - жанр для фильтрации подборок (опционально)
 */
interface PlaylistsModuleProps {
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
const PlaylistsModule = (props: PlaylistsModuleProps): ReactElement => {
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
        genre: props.genre,
        skip: skip,
        take: take,
      });
    },
    [props, playlistsApi]
  );

  /** Хук для порционной загрузки подборок с пагинацией */
  const {
    items: playlists,
    isLoading,
    hasMore,
    fetchMore,
  } = usePaginatedFetch<PlaylistResponse>(fetch, 20);

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Обработчик выбора подборки из списка.
   * @param id - ID выбранной подборки
   * @returns {void}
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/playlist', { state: { id: id } });
    },
    [navigate]
  );

  // Отображает скелетон во время получения данных
  if (isLoading) return <FilmsCatalogSkeleton />;

  // Отображает состояние пустого списка если подборки не найдены
  if (playlists.length === 0) return <NoData text="Подборки не найдены" />;

  // Возвращает каталог подборок с поддержкой пагинации и фильтрации
  return (
    <PlaylistsCatalog
      hasMore={hasMore}
      next={fetchMore}
      genre={props.genre}
      playlists={playlists}
      onSelect={onSelect}
    />
  );
};

export default PlaylistsModule;
