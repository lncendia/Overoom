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
  // Получаем контекст комнаты с состоянием и методами управления
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
    // Обработчик событий, приходящих от видеоплеера
    const eventHandler = async (e: PlayerEventContainer) => {
      // Обрабатываем событие паузы/воспроизведения
      if (e.pauseEvent) {
        await setPause(e.pauseEvent.onPause, e.pauseEvent.ticks, e.pauseEvent.buffering);
      }
      // Обрабатываем событие перемотки
      else if (e.seekEvent) {
        await setTimeLine(e.seekEvent.ticks);
      }
      // Обрабатываем событие смены эпизода
      else if (e.changeEpisodeEvent) {
        await setEpisode(e.changeEpisodeEvent.season, e.changeEpisodeEvent.episode);
      }
      // Обрабатываем событие полноэкранного режима
      else if (e.fullscreenEvent) {
        await setFullscreen(e.fullscreenEvent.fullscreen);
      }
      // Обрабатываем событие переключения скорости
      else if (e.speedEvent) {
        await setSpeed(e.speedEvent.speed);
      }
      // Обрабатываем событие переключения громкости
      else if (e.muteEvent) {
        await hub?.setMuted(e.muteEvent.muted);
      }
      // Обрабатываем событие переключения громкости
      else if (e.initEvent) {
        setPlayerInitialized(true);
      }
    };

    // Монтируем обработчик и подписываемся на события
    handler.mount();
    handler.addHandler(eventHandler);

    // Функция очистки - отписываемся от событий и демонтируем обработчик
    return () => {
      handler.unmount();
      handler.removeHandler(eventHandler);
    };
  }, [handler, hub, setEpisode, setFullscreen, setPause, setSpeed, setTimeLine]);

  /** Обработка событий от комнаты и применение их к плееру */
  useEffect(() => {
    // Если хаб еще не проинициализирован - ничего не делаем
    if (!hub || !room?.ownerId || !playerInitialized) return;

    // Обработчик событий, приходящих от комнаты (сервера)
    const eventHandler = (e: RoomEventContainer) => {
      // Применяем команду паузы/воспроизведения к плееру
      if (e.pauseEvent) {
        handler.setPause(e.pauseEvent.pause);
      }
      // Применяем команду перемотки к плееру
      else if (e.timeLineEvent) {
        handler.setTimeLine(e.timeLineEvent.timeLine);
      }
      // Применяем команду смены эпизода к плееру
      else if (e.episodeEvent) {
        handler.setEpisode(e.episodeEvent.season, e.episodeEvent.episode);
      }
      // Применяем команду изменения скорости воспроизведения
      else if (e.speedEvent) {
        handler.setSpeed(e.speedEvent.speed);
      }
    };

    // Подписываемся на события от хаба комнаты
    hub.addHandler(eventHandler);

    // Функция очистки - отписываемся от событий хаба
    return () => hub.removeHandler(eventHandler);
  }, [room?.ownerId, handler, hub, playerInitialized]);

  /** Синхронизация при загрузке медиа-плеера */
  const onSync = useCallback(() => {
    // Если хаб еще не проинициализирован - ничего не делаем
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
    // Если хаб еще не проинициализирован - ничего не делаем
    if (!room?.ownerId) return;

    if (!playerInitialized || room.ownerId === currentViewerId) return;
    showSyncNotification();
  }, [currentViewerId, playerInitialized, room?.ownerId, showSyncNotification]);

  // Рендерим модуль видеоплеера
  return <FilmPlayerModule />;
};

export default RoomPlayerModule;
