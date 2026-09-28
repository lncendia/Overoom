import { Box, Link } from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement, useCallback, useEffect, useMemo, useState } from 'react';

import PlayerEventContainer from './handlers/events/player-event.container.ts';
import { IPlayerHandler } from './handlers/IPlayerHandler.ts';
import { PlayerJsHandler } from './handlers/PlayerJsHandler.ts';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import FilmPlayerModule from '../../films/film-player-module/FilmPlayerModule.tsx';

/**
 * Модуль плеера для синхронизированного просмотра в комнате.
 * Обеспечивает двустороннюю связь между видеоплеером и комнатой
 * @returns {ReactElement} JSX элемент модуля плеера
 */
const RoomPlayerModule = (): ReactElement => {
  const { room, currentViewerId, hub, setPause, setTimeLine, setEpisode, setFullscreen, setSpeed } =
    useRoom();

  const [playerInitialized, setPlayerInitialized] = useState(false);

  /** Хук для работы с уведомлениями */
  const { setNotification } = useNotify();

  /** Инициализация обработчика событий плеера (Player.js) */
  const handler = useMemo<IPlayerHandler>(() => {
    return new PlayerJsHandler('player');
  }, []);

  /** Обработка событий от плеера и синхронизация с комнатой */
  useEffect(() => {
    const eventHandler = async (e: PlayerEventContainer) => {
      if (e.pauseEvent) {
        await setPause(e.pauseEvent.onPause, e.pauseEvent.ticks, e.pauseEvent.buffering);
      }
      else if (e.seekEvent) {
        await setTimeLine(e.seekEvent.ticks);
      }
      else if (e.changeEpisodeEvent) {
        await setEpisode(e.changeEpisodeEvent.season, e.changeEpisodeEvent.episode);
      }
      else if (e.fullscreenEvent) {
        await setFullscreen(e.fullscreenEvent.fullscreen);
      }
      else if (e.speedEvent) {
        await setSpeed(e.speedEvent.speed);
      }
      else if (e.muteEvent) {
        await hub?.setMuted(e.muteEvent.muted);
      }
      else if (e.initEvent) {
        setPlayerInitialized(true);
      }
    };

    handler.mount();
    handler.addHandler(eventHandler);

    return () => {
      handler.unmount();
      handler.removeHandler(eventHandler);
    };
  }, [handler, hub, setEpisode, setFullscreen, setPause, setSpeed, setTimeLine]);

  /** Обработка событий от комнаты и применение их к плееру */
  useEffect(() => {
    if (!hub || !room?.ownerId || !playerInitialized) return;

    const eventHandler = (e: RoomEventContainer) => {
      if (e.pauseEvent) {
        handler.setPause(e.pauseEvent.pause);
      }
      else if (e.timeLineEvent) {
        handler.setTimeLine(e.timeLineEvent.timeLine);
      }
      else if (e.episodeEvent) {
        handler.setEpisode(e.episodeEvent.season, e.episodeEvent.episode);
      }
      else if (e.speedEvent) {
        handler.setSpeed(e.speedEvent.speed);
      }
    };

    hub.addHandler(eventHandler);

    return () => hub.removeHandler(eventHandler);
  }, [room?.ownerId, handler, hub, playerInitialized]);

  /** Синхронизация при загрузке медиа-плеера */
  const onSync = useCallback(() => {
    if (!hub) return;

    hub.sync().then();
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

  /** При подключении к комнате проверяет, нужно ли показывать уведомление синхронизации. */
  useEffect(() => {
    if (!room?.ownerId) return;

    if (!playerInitialized || room.ownerId === currentViewerId) return;
    showSyncNotification();
  }, [currentViewerId, playerInitialized, room?.ownerId, showSyncNotification]);

  return <FilmPlayerModule />;
};

export default RoomPlayerModule;
