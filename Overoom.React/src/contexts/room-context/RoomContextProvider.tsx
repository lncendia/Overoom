import React, { ReactElement, ReactNode, useCallback } from 'react';

import { RoomContext } from './RoomContext.tsx';
import { ViewerStateDto } from '../../hooks/room-hook/room.dto.ts';
import useRoom from '../../hooks/room-hook/useRoom.ts';
import { RoomHub } from '../../services/room-hub/room.hub.ts';
import { useAuthentication } from '../authentication-context/useAuthentication.tsx';

/** Пропсы провайдера контекста комнаты */
interface RoomContextProviderProps {
  /** Идентификатор комнаты */
  id: string;
  /** Дочерние элементы*/
  children: ReactNode;
  /** Хаб для работы с комнатой */
  hub: RoomHub | null;
}

/**
 * Провайдер контекста комнаты
 * @param props - свойства компонента
 * @param props.hub - хаб комнаты
 * @param props.children - дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера комнаты
 */
export const RoomContextProvider: React.FC<RoomContextProviderProps> = ({
  hub,
  children,
}: RoomContextProviderProps): ReactElement => {
  /** Состояние комнаты и метод его обновления */
  const [room, setRoom] = useRoom(hub);

  /** Данные авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /**
   * Обновляет состояние текущего пользователя (viewer) в комнате
   * @param updater - функция, принимающая текущее состояние viewer и возвращающая обновлённое
   */
  const updateCurrentViewer = useCallback(
    (updater: (viewer: ViewerStateDto) => ViewerStateDto) => {
      setRoom((prev) => {
        // Если комнаты ещё нет или пользователь не авторизован → возвращаем предыдущее состояние без изменений
        if (!prev || !authorizedUser) return prev;

        // Копируем карту зрителей (viewerStates), чтобы не мутировать оригинал
        const newPlayers = new Map(prev.viewerStates);

        // Находим состояние текущего пользователя
        const current = newPlayers.get(authorizedUser.id);
        if (!current) return prev;

        // Обновляем состояние текущего пользователя через функцию updater
        newPlayers.set(authorizedUser.id, updater(current));

        // Возвращаем новое состояние комнаты с обновлённым viewerStates
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
      updateCurrentViewer((v) => ({ ...v, timeLine: ticks }));
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
      updateCurrentViewer((v) => ({ ...v, onPause: pause, timeLine: ticks }));
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
      updateCurrentViewer((v) => ({
        ...v,
        season,
        episode,
        timeLine: 0,
        onPause: true,
      }));
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
      updateCurrentViewer((v) => ({ ...v, fullScreen: fullscreen }));
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
      updateCurrentViewer((v) => ({ ...v, speed }));
      await hub?.setSpeed(speed);
    },
    [hub, updateCurrentViewer]
  );

  return (
    <RoomContext.Provider
      value={{
        room,
        hub,
        currentViewerId: authorizedUser!.id,
        setTimeLine,
        setPause,
        setEpisode,
        setFullscreen,
        setSpeed,
      }}
    >
      {children}
    </RoomContext.Provider>
  );
};
