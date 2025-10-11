import { useInjection } from 'inversify-react';
import { ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';

import CreateRoomForm from '../../../components/rooms/create-room-form/CreateRoomForm.tsx';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import Drawer from '../../../ui/drawer/Drawer.tsx';

/** Пропсы для компонента CreateRoomModule */
interface CreateFilmRoomModuleProps {
  /** Флаг, указывающий открыт ли модуль */
  open: boolean;
  /** Callback, вызываемый при закрытии модуля */
  onClose: () => void;
}

/**
 * Компонент для создания комнаты просмотра фильма.
 * Предоставляет интерфейс для создания новой комнаты с выбором CDN.
 * @param props - Пропсы компонента
 * @returns {ReactElement} Компонент модуля создания комнаты
 */
const CreateRoomModule = (props: CreateFilmRoomModuleProps): ReactElement => {
  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /** Данные текущего фильма */
  const { film } = useFilm();

  /** Хук для программной навигации */
  const navigate = useNavigate();

  /**
   * Создает новую комнату для просмотра фильма
   * @param cdn - название CDN для трансляции
   * @param open - флаг, указывающий является ли комната открытой для всех
   */
  const createRoom = useSafeCallback(
    async (open: boolean) => {
      if (!film) return;
      // Создаем комнату через API
      const id = await roomsApi.create({
        open: open,
        filmId: film.id,
      });

      // Перенаправляем пользователя на страницу комнаты
      navigate('/room', { state: { id: id } });
    },
    [roomsApi, film, navigate]
  );

  return (
    <Drawer title="Создание комнаты" show={props.open} onClose={props.onClose}>
      <CreateRoomForm callback={createRoom} />
    </Drawer>
  );
};

export default CreateRoomModule;
