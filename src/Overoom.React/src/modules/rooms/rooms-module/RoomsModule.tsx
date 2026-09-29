import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsCatalogSkeleton from '../../../components/films/films-catalog/FilmsCatalog.skeleton.tsx';
import RoomsCatalog from '../../../components/rooms/rooms-list/RoomsCatalog.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { RoomShortResponse } from '../../../services/rooms/responses/room-short.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/** Пропсы компонента RoomsModule */
interface FilmRoomsModuleProps {
  /** Если true, отображаются только публичные комнаты */
  onlyPublic?: boolean;
  /** ID фильма для фильтрации комнат */
  filmId?: string;
}

/**
 * Компонент для отображения каталога комнат с возможностью фильтрации
 * @param props - фильтры и параметры отображения
 * @returns {ReactElement} JSX элемент каталога комнат
 */
const RoomsModule = (props: FilmRoomsModuleProps): ReactElement => {
  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /**
   * Функция для загрузки комнат с серверной пагинацией
   * @param skip - количество пропускаемых элементов
   * @param take - количество элементов для загрузки
   * @returns Promise с результатом поиска комнат
   */
  const fetch = useCallback(
    (skip: number, take: number) => {
      return roomsApi.search({
        onlyPublic: props.onlyPublic,
        filmId: props.filmId,
        skip: skip,
        take: take,
      });
    },
    [props.onlyPublic, props.filmId, roomsApi]
  );

  /** Хук для порционно-загружаемого списка комнат */
  const {
    items: rooms,
    isLoading,
    hasMore,
    fetchMore,
    error,
    retry,
  } = usePaginatedFetch<RoomShortResponse>(fetch, 20);

  if (isLoading) return <FilmsCatalogSkeleton />;
  if (error && rooms.length === 0)
    return <LoadError text="Не удалось загрузить комнаты" onRetry={retry} />;
  if (rooms.length === 0) return <NoData text="Комнаты не найдены" />;

  return (
    <RoomsCatalog
      rooms={rooms}
      getLink={(id) => routes.room(id)}
      next={fetchMore}
      hasMore={hasMore}
    />
  );
};

export default RoomsModule;
