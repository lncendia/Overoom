import { Box, Link } from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement, useCallback, useEffect, useMemo, useRef, useState } from 'react';

import PlayerEventContainer from './handlers/events/player-event.container.ts';
import { IPlayerHandler } from './handlers/IPlayerHandler.ts';
import { PlayerJsHandler } from './handlers/PlayerJsHandler.ts';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { getCurrentTimeLine } from '../../../hooks/room-hook/room-event.handlers.ts';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import FilmPlayerModule from '../../films/film-player-module/FilmPlayerModule.tsx';

/** Позиция (в тиках), начиная с которой просмотр на паузе считается начатым (5 секунд) */
const STARTED_TIMELINE_THRESHOLD = 5 * 10_000_000;

/**
 * Модуль плеера для синхронизированного просмотра в комнате.
 * Обеспечивает двустороннюю связь между видеоплеером и комнатой
 * @returns {ReactElement} JSX элемент модуля плеера
 */
const RoomPlayerModule = (): ReactElement => {
  const {
    room,
    currentViewerId,
    hub,
    reconnectCount,
    setPause,
    setTimeLine,
    setEpisode,
    setFullscreen,
    setSpeed,
  } = useRoom();

  const [playerInitialized, setPlayerInitialized] = useState(false);

  /** Хук для работы с уведомлениями */
  const { setNotification } = useNotify();

  /** Инициализация обработчика событий плеера (Player.js) */
  const handler = useMemo<IPlayerHandler>(() => {
    return new PlayerJsHandler('player');
  }, []);

  /** Обработка событий от плеера и синхронизация с комнатой */
  useEffect(() => {
    /**
     * Применяет событие плеера к комнате
     * @param e - Событие плеера
     */
    const handleEvent = async (e: PlayerEventContainer) => {
      if (e.pauseEvent) {
        await setPause(e.pauseEvent.onPause, e.pauseEvent.ticks, e.pauseEvent.buffering);
      } else if (e.seekEvent) {
        await setTimeLine(e.seekEvent.ticks);
      } else if (e.changeEpisodeEvent) {
        await setEpisode(e.changeEpisodeEvent.season, e.changeEpisodeEvent.episode);
      } else if (e.fullscreenEvent) {
        await setFullscreen(e.fullscreenEvent.fullscreen);
      } else if (e.speedEvent) {
        await setSpeed(e.speedEvent.speed);
      } else if (e.muteEvent) {
        await hub?.setMuted(e.muteEvent.muted);
      } else if (e.initEvent) {
        setPlayerInitialized(true);
      }
    };

    /**
     * Обработчик событий плеера: ошибки отправки в хаб (например, во время
     * переподключения) не должны приводить к необработанным отклонениям промисов
     * @param e - Событие плеера
     */
    const eventHandler = (e: PlayerEventContainer) => {
      handleEvent(e).catch((err) => console.warn('Не удалось передать событие плеера', err));
    };

    handler.mount();
    handler.addHandler(eventHandler);

    // unmount снимает все обработчики, отдельный removeHandler не нужен
    return () => handler.unmount();
  }, [handler, hub, setEpisode, setFullscreen, setPause, setSpeed, setTimeLine]);

  /** Обработка событий от комнаты и применение их к плееру */
  useEffect(() => {
    if (!hub || !room?.ownerId || !playerInitialized) return;

    const eventHandler = (e: RoomEventContainer) => {
      if (e.pauseEvent) {
        handler.setPause(e.pauseEvent.pause);
      } else if (e.timeLineEvent) {
        handler.setTimeLine(e.timeLineEvent.timeLine);
      } else if (e.episodeEvent) {
        handler.setEpisode(e.episodeEvent.season, e.episodeEvent.episode);
      } else if (e.speedEvent) {
        handler.setSpeed(e.speedEvent.speed);
      }
    };

    hub.addHandler(eventHandler);

    return () => hub.removeHandler(eventHandler);
  }, [room?.ownerId, handler, hub, playerInitialized]);

  /** Синхронизация при загрузке медиа-плеера */
  const onSync = useCallback(() => {
    if (!hub) return;

    hub.sync().catch((err) => console.warn('Не удалось синхронизироваться с комнатой', err));
  }, [hub]);

  /** Показывает уведомление с предложением синхронизироваться с текущим просмотром комнаты. */
  const showSyncNotification = useCallback(() => {
    const content = (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography variant="body2">
          В комнате уже идет просмотр. Синхронизируйтесь с владельцем, чтобы догнать остальных.
        </Typography>
        <Link component="button" variant="body2" onClick={onSync}>
          Синхронизироваться
        </Link>
      </Box>
    );

    setNotification({
      message: content,
      severity: 'info',
    });
  }, [onSync, setNotification]);

  /** Флаг: уведомление о синхронизации уже показано */
  const syncNotified = useRef(false);

  /** Флаг: владелец комнаты онлайн и действительно смотрит (не стоит на паузе в начале) */
  const ownerIsWatching = useMemo(() => {
    if (!room?.ownerId || room.ownerId === currentViewerId) return false;
    if (!room.viewers.get(room.ownerId)?.online) return false;

    const ownerState = room.viewerStates.get(room.ownerId);
    if (!ownerState) return false;

    return !ownerState.onPause || getCurrentTimeLine(ownerState) > STARTED_TIMELINE_THRESHOLD;
  }, [currentViewerId, room?.ownerId, room?.viewers, room?.viewerStates]);

  /** После инициализации плеера один раз предлагает синхронизироваться, если владелец смотрит */
  useEffect(() => {
    if (syncNotified.current || !playerInitialized || !ownerIsWatching) return;
    syncNotified.current = true;
    showSyncNotification();
  }, [ownerIsWatching, playerInitialized, showSyncNotification]);

  /** После переподключения зритель (не владелец) заново синхронизируется с владельцем */
  useEffect(() => {
    if (reconnectCount === 0 || !playerInitialized) return;
    if (!room?.ownerId || room.ownerId === currentViewerId) return;
    hub?.sync().catch((err) => console.warn('Не удалось синхронизироваться с комнатой', err));
    // Синхронизация нужна только при новом переподключении
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [reconnectCount]);

  return <FilmPlayerModule />;
};

export default RoomPlayerModule;
