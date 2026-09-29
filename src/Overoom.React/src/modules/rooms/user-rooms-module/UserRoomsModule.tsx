import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsSliderSkeleton from '../../../components/films/films-slider/FilmsSlider.skeleton.tsx';
import FilmsSlider from '../../../components/films/films-slider/FilmsSlider.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { RoomShortResponse } from '../../../services/rooms/responses/room-short.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Компонент для отображения пользовательских комнат в виде слайдера
 * @returns {ReactElement} JSX элемент слайдера комнат пользователя
 */
const UserRoomsModule = (): ReactElement => {
  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /**
   * Функция загрузки комнат текущего пользователя
   * @returns Promise с объектом { list, totalCount }
   */
  const fetch = useCallback(async () => {
    const rooms = await roomsApi.getMy();
    return {
      list: rooms,
      totalCount: rooms.length,
    };
  }, [roomsApi]);

  /** Хук для порционно-загружаемого списка комнат */
  const { items: rooms, isLoading, error, retry } = usePaginatedFetch<RoomShortResponse>(fetch, 20);

  if (isLoading) return <FilmsSliderSkeleton />;
  if (error && rooms.length === 0)
    return <LoadError text="Не удалось загрузить комнаты" onRetry={retry} />;
  if (rooms.length === 0) return <NoData text="Комнаты не найдены" />;

  return <FilmsSlider films={rooms} getLink={(id) => routes.room(id)} />;
};

export default UserRoomsModule;
