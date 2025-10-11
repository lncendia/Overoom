import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsSliderSkeleton from '../../../components/films/films-slider/FilmsSlider.skeleton.tsx';
import FilmsSlider from '../../../components/films/films-slider/FilmsSlider.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { RoomShortResponse } from '../../../services/rooms/responses/room-short.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';

/**
 * Компонент для отображения пользовательских комнат в виде слайдера
 * @returns {ReactElement | null} JSX элемент слайдера комнат пользователя
 */
const UserRoomsModule = (): ReactElement | null => {
  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

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
  const { items: rooms, isLoading } = usePaginatedFetch<RoomShortResponse>(fetch, 20);

  /**
   * Обработчик выбора комнаты
   * @param id - ID выбранной комнаты
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/room', { state: { id } });
    },
    [navigate]
  );

  // Показ скелетона, если данные ещё загружаются
  if (isLoading) return <FilmsSliderSkeleton />;

  // Возвращаем null, если комнаты не найдены
  if (rooms.length === 0) return null;

  return <FilmsSlider films={rooms} onSelect={onSelect} />;
};

export default UserRoomsModule;
