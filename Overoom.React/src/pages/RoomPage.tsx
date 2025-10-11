import { Box, useMediaQuery, useTheme } from '@mui/material';
import { ReactElement } from 'react';
import { useLocation } from 'react-router-dom';

import FilmRatingModule from '../modules/films/film-rating-module/FilmRatingModule.tsx';
import AuthorizeGuard from '../modules/guards/AuthorizeGuard.tsx';
import BeepModule from '../modules/rooms/beep-module/BeepModule.tsx';
import ConnectHubModule from '../modules/rooms/connect-room-module/ConnectHubModule.tsx';
import ConnectRoomModule from '../modules/rooms/connect-room-module/ConnectRoomModule.tsx';
import DisconnectModule from '../modules/rooms/disconnect-module/DisconnectModule.tsx';
import NotificationModule from '../modules/rooms/notification-module/NotificationModule.tsx';
import RoomChatModule from '../modules/rooms/room-chat-module/RoomChatModule.tsx';
import RoomInfoModule from '../modules/rooms/room-info-module/RoomInfoModule.tsx';
import RoomPlayerModule from '../modules/rooms/room-player-module/RoomPlayerModule.tsx';
import RoomViewersModule from '../modules/rooms/room-viewers-module/RoomViewersModule.tsx';
import ScreamModule from '../modules/rooms/scream-module/ScreamModule.tsx';

/**
 * Основная страница комнаты для совместного просмотра
 * @returns {ReactElement} JSX элемент страницы комнаты
 */
const RoomPage = (): ReactElement => {
  /** Используем хук useLocation для получения состояния навигации (в т.ч. ID фильма) */
  const location = useLocation();
  const params = new URLSearchParams(location.search);

  // Получаем параметры комнаты из URL или состояния навигации
  const code = params.get('code') ?? '';
  const id = params.get('id') ?? location.state.id;

  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('xl'));

  return (
    <AuthorizeGuard>
      {/* Модуль подключения к комнате (внешний слой) */}
      <ConnectRoomModule id={id} code={code}>
        {/* Модуль подключения к хабу комнаты (внутренний слой) */}
        <ConnectHubModule id={id}>
          {/* Основной layout комнаты для маленьких экранов */}
          {isMobile && <RoomMobileLayout />}

          {/* Основной layout комнаты для больших экранов */}
          {!isMobile && <RoomDesktopLayout />}

          {/* Модуль обработки звуковых сигналов "бип" */}
          <BeepModule />

          {/* Модуль обработки скримеров */}
          <ScreamModule />

          {/* Модуль обработки отключения от комнаты */}
          <DisconnectModule />

          {/* Модуль системных уведомлений */}
          <NotificationModule />
        </ConnectHubModule>
      </ConnectRoomModule>
    </AuthorizeGuard>
  );
};

/**
 * Основная сетка страницы комнаты (десктоп-версия).
 * @returns {ReactElement} JSX элемент сетки страницы комнаты
 */
const RoomDesktopLayout = (): ReactElement => {
  return (
    <Box sx={{ display: 'flex', position: 'relative', columnGap: 4 }}>
      {/* Левая колонка - основной контент */}
      <Box sx={{ width: '100%' }}>
        {/* Информация о комнате */}
        <RoomInfoModule />

        {/* Плеер для просмотра контента */}
        <RoomPlayerModule />

        {/* Модуль рейтингов фильмов */}
        <FilmRatingModule sx={{ mb: 0 }} />
      </Box>

      {/* Правая колонка - боковая панель */}
      <Box sx={{ flexShrink: 2, width: '100%' }}>
        <Box sx={{ position: 'sticky', top: 0, zIndex: 1 }}>
          {/* Список зрителей комнаты */}
          <RoomViewersModule />

          {/* Чат комнаты */}
          <RoomChatModule />
        </Box>
      </Box>
    </Box>
  );
};

/**
 * Основная сетка страницы комнаты (мобильная версия).
 * @returns {ReactElement} JSX элемент сетки страницы комнаты
 */
const RoomMobileLayout = (): ReactElement => {
  return (
    <Box>
      {/* Информация о комнате */}
      <RoomInfoModule />

      {/* Список зрителей комнаты */}
      <RoomViewersModule />

      {/* Плеер для просмотра контента */}
      <RoomPlayerModule />

      {/* Чат комнаты */}
      <RoomChatModule />

      {/* Модуль рейтингов фильмов */}
      <FilmRatingModule />
    </Box>
  );
};

export default RoomPage;
