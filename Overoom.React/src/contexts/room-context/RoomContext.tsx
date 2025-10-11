import React, { createContext } from 'react';

import RoomDto from '../../hooks/room-hook/room.dto.ts';
import { RoomHub } from '../../services/room-hub/room.hub.ts';

/** Интерфейс контекста комнаты для управления состоянием просмотра */
export interface RoomContextType {
  /** Данные текущей комнаты */
  room: RoomDto | null;
  /** Хаб для работы с комнатой */
  hub: RoomHub | null;
  /** Идентификатор текущего зрителя */
  currentViewerId: string;
  /** Функция обновления времени просмотра */
  setTimeLine: (ticks: number) => Promise<void>;
  /** Функция управления паузой */
  setPause: (pause: boolean, ticks: number, buffering: boolean) => Promise<void>;
  /** Функция смены серии */
  setEpisode: (season: number, episode: number) => Promise<void>;
  /** Функция переключения полноэкранного режима */
  setFullscreen: (fullscreen: boolean) => Promise<void>;
  /** Функция изменения скорости воспроизведения */
  setSpeed: (speed: number) => Promise<void>;
}

/** Контекст комнаты с undefined в качестве значения по умолчанию */
export const RoomContext: React.Context<RoomContextType | undefined> = createContext<
  RoomContextType | undefined
>(undefined);
