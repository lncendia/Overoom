import { useInjection } from 'inversify-react';
import React, { ReactElement, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import ConnectRoomForm from '../../../components/rooms/connect-room-form/ConnectRoomForm.tsx';
import { FilmContextProvider } from '../../../contexts/film-context/FilmContextProvider.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomResponse } from '../../../services/rooms/responses/room.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import Drawer from '../../../ui/drawer/Drawer.tsx';

/** Пропсы компонента ConnectRoomModule */
interface ConnectRoomModuleProps {
  /** Идентификатор комнаты для подключения */
  id: string;
  /** Код доступа для приватной комнаты (опционально) */
  code?: string;
  /** Дочерние элементы компонента */
  children: ReactElement<{ joined?: boolean }>;
}

/**
 * Модуль для подключения к комнате просмотра фильма.
 * Обрабатывает приватные комнаты с кодом доступа, управляет состоянием подключения
 * и предоставляет контексты для работы с фильмом и комнатой.
 * @param props - Пропсы компонента
 * @returns {ReactElement} Компонент модуля подключения к комнате
 */
const ConnectRoomModule = (props: ConnectRoomModuleProps): ReactElement => {
  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /** Хук для получения данных о комнате */
  const room = useRoom(roomsApi, props.id);

  /** Хук для управления подключением к комнате */
  const { isConnecting, isConnected, connect } = useRoomConnection(roomsApi, room);

  const show = useMemo(() => {
    return room != null && !room.isUserIn && !isConnected;
  }, [isConnected, room]);

  const codeNeeded = useMemo(() => {
    return room != null && room.isPrivate;
  }, [room]);

  const joined = useMemo(() => {
    return room != null && (room.isUserIn || isConnected);
  }, [isConnected, room]);

  // Если не подключены к комнате, показываем форму подключения
  return (
    <>
      <Drawer title="Подключение к комнате" show={show} onClose={() => navigate(-1)}>
        <ConnectRoomForm
          onConnect={connect}
          code={props.code}
          codeNeeded={codeNeeded}
          isConnecting={isConnecting}
        />
      </Drawer>
      <FilmContextProvider filmId={room?.filmId}>
        {React.Children.map(props.children, (child) =>
          React.isValidElement(child) ? React.cloneElement(child, { joined: joined }) : child
        )}
      </FilmContextProvider>
    </>
  );
};

export default ConnectRoomModule;

/**
 * Хук для получения данных о комнате
 * @param roomsApi - Сервис для работы с API комнат
 * @param roomId - Идентификатор комнаты
 * @returns Объект с данными комнаты и состоянием загрузки
 */
const useRoom = (roomsApi: RoomsApi, roomId: string) => {
  /** Состояние данных о комнате */
  const [room, setRoom] = useState<RoomResponse | null>(null);

  /** Безопасный колбэк для получения данных о комнате */
  const getRoom = useSafeCallback(async () => {
    const roomData = await roomsApi.get(roomId);
    setRoom(roomData);
  }, [roomId, roomsApi]);

  /** Эффект для загрузки данных о комнате при монтировании компонента */
  useEffect(() => {
    getRoom().then();
    return () => {
      setRoom(null);
    };
  }, [getRoom]);

  return room;
};

/**
 * Хук для управления подключением к комнате
 * @param roomsApi - Сервис для работы с API комнат
 * @param room - Данные о комнате (опционально)
 * @returns Объект с данными подключения и методами управления
 */
const useRoomConnection = (roomsApi: RoomsApi, room: RoomResponse | null) => {
  /** Состояние процесса подключения */
  const [isConnecting, setIsConnecting] = useState(false);

  /** Состояние успешного подключения */
  const [isConnected, setIsConnected] = useState(false);

  /** Функция подключения к комнате с возможным кодом доступа */
  const connect = useSafeCallback(
    async (code?: string) => {
      if (!room?.id) return;
      setIsConnecting(true);
      try {
        await roomsApi.join(room.id, { code });
        setIsConnected(true);
      } finally {
        setIsConnecting(false);
      }
    },
    [room?.id, roomsApi]
  );

  return {
    isConnecting,
    isConnected,
    connect,
  };
};
