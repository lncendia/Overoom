import React, { ReactElement, ReactNode, useCallback, useMemo } from 'react';

import { RoomConnectionStatus, RoomContext, RoomContextType } from './RoomContext.tsx';
import { applyViewerStatePatch } from '../../hooks/room-hook/room-event.handlers.ts';
import { ViewerStateDto } from '../../hooks/room-hook/room.dto.ts';
import useRoom from '../../hooks/room-hook/useRoom.ts';
import { RoomHub } from '../../services/room-hub/room.hub.ts';
import { useAuthentication } from '../authentication-context/useAuthentication.tsx';

/** Пропсы провайдера контекста комнаты */
interface RoomContextProviderProps {
  /** Дочерние элементы*/
  children: ReactNode;
  /** Хаб для работы с комнатой */
  hub: RoomHub | null;
  /** Состояние подключения к комнате */
  connectionStatus: RoomConnectionStatus;
  /** Количество успешных переподключений к комнате */
  reconnectCount: number;
}

/**
 * Провайдер контекста комнаты
 * @param props - свойства компонента
 * @param props.hub - хаб комнаты
 * @param props.connectionStatus - состояние подключения к комнате
 * @param props.reconnectCount - количество успешных переподключений к комнате
 * @param props.children - дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера комнаты
 */
export const RoomContextProvider: React.FC<RoomContextProviderProps> = ({
  hub,
  connectionStatus,
  reconnectCount,
  children,
}: RoomContextProviderProps): ReactElement => {
  /** Состояние комнаты и метод его обновления */
  const [room, setRoom] = useRoom(hub, reconnectCount);

  /** Данные авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /**
   * Обновляет состояние текущего пользователя (viewer) в комнате
   * @param patch - обновляемые поля состояния viewer
   */
  const updateCurrentViewer = useCallback(
    (patch: Partial<ViewerStateDto>) => {
      setRoom((prev) => {
        if (!prev || !authorizedUser) return prev;

        const newPlayers = new Map(prev.viewerStates);
        const current = newPlayers.get(authorizedUser.id);
        if (!current) return prev;

        newPlayers.set(authorizedUser.id, applyViewerStatePatch(current, patch));

        return { ...prev, viewerStates: newPlayers };
      });
    },
    [authorizedUser, setRoom]
  );

  /**
   * Обновляет временную метку просмотра
   * @param ticks - наносекунды, на которые установлено время просмотра
   */
  const setTimeLine = useCallback(
    async (ticks: number) => {
      updateCurrentViewer({ timeLine: ticks });
      await hub?.setTimeLine(ticks);
    },
    [hub, updateCurrentViewer]
  );

  /**
   * Устанавливает состояние паузы
   * @param pause - флаг паузы
   * @param ticks - наносекунды текущего времени
   * @param buffering - флаг буферизации
   */
  const setPause = useCallback(
    async (pause: boolean, ticks: number, buffering: boolean) => {
      updateCurrentViewer({ onPause: pause, timeLine: ticks });
      await hub?.setPause(pause, ticks, buffering);
    },
    [hub, updateCurrentViewer]
  );

  /**
   * Меняет текущую серию
   * @param season - номер сезона
   * @param episode - номер серии
   */
  const setEpisode = useCallback(
    async (season: number, episode: number) => {
      updateCurrentViewer({ season, episode, timeLine: 0, onPause: true });
      await hub?.setEpisode(season, episode);
    },
    [hub, updateCurrentViewer]
  );

  /**
   * Переключает состояние полноэкранного режима пользователя
   * @param fullscreen - флаг полноэкранного режима
   */
  const setFullscreen = useCallback(
    async (fullscreen: boolean) => {
      updateCurrentViewer({ fullScreen: fullscreen });
      await hub?.setFullScreen(fullscreen);
    },
    [hub, updateCurrentViewer]
  );

  /**
   * Устанавливает скорость воспроизведения
   * @param speed - скорость воспроизведения
   */
  const setSpeed = useCallback(
    async (speed: number) => {
      updateCurrentViewer({ speed });
      await hub?.setSpeed(speed);
    },
    [hub, updateCurrentViewer]
  );

  /** Идентификатор текущего зрителя */
  const currentViewerId = authorizedUser!.id;

  /** Мемоизированное значение контекста, чтобы потребители не перерисовывались без изменений */
  const value = useMemo<RoomContextType>(
    () => ({
      room,
      hub,
      connectionStatus,
      reconnectCount,
      currentViewerId,
      setTimeLine,
      setPause,
      setEpisode,
      setFullscreen,
      setSpeed,
    }),
    [
      room,
      hub,
      connectionStatus,
      reconnectCount,
      currentViewerId,
      setTimeLine,
      setPause,
      setEpisode,
      setFullscreen,
      setSpeed,
    ]
  );

  return <RoomContext.Provider value={value}>{children}</RoomContext.Provider>;
};
