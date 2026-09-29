import { Box } from '@mui/material';
import { ReactElement } from 'react';
import { Navigate, useParams, useSearchParams } from 'react-router-dom';

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
import { routes } from '../routes.ts';

/**
 * Основная страница комнаты для совместного просмотра.
 * Идентификатор комнаты берется из пути (/room/:id), код доступа — из query-параметра code
 * @returns {ReactElement} JSX элемент страницы комнаты
 */
const RoomPage = (): ReactElement => {
  /** Идентификатор комнаты из пути */
  const { id } = useParams<{ id: string }>();

  /** Query-параметры адреса (код доступа к приватной комнате) */
  const [searchParams] = useSearchParams();
  const code = searchParams.get('code') ?? undefined;

  if (!id) return <Navigate to={routes.rooms} replace />;

  return (
    <AuthorizeGuard>
      <ConnectRoomModule key={id} id={id} code={code}>
        <ConnectHubModule id={id}>
          <RoomLayout />

          <BeepModule />

          <ScreamModule />

          <DisconnectModule />

          <NotificationModule />
        </ConnectHubModule>
      </ConnectRoomModule>
    </AuthorizeGuard>
  );
};

/**
 * Сетка страницы комнаты.
 * Одно дерево для всех размеров экрана: при смене ширины меняется только расположение
 * областей CSS grid, поэтому плеер и чат не пересоздаются.
 * На узких экранах блок зрителей и чата "растворяется" (display: contents), и его части
 * встают в общую колонку; на широких — это прилипающая правая колонка
 * @returns {ReactElement} JSX элемент сетки страницы комнаты
 */
const RoomLayout = (): ReactElement => {
  return (
    <Box
      sx={{
        display: 'grid',
        position: 'relative',
        gridTemplateColumns: { xs: 'minmax(0, 1fr)', lg: 'minmax(0, 2fr) minmax(0, 1fr)' },
        gridTemplateAreas: {
          xs: '"info" "viewers" "player" "chat" "rating"',
          lg: '"info side" "player side" "rating side"',
        },
        gridTemplateRows: { lg: 'auto auto 1fr' },
        columnGap: 4,
      }}
    >
      <Box sx={{ gridArea: 'info' }}>
        <RoomInfoModule />
      </Box>

      <Box sx={{ gridArea: 'player' }}>
        <RoomPlayerModule />
      </Box>

      <Box sx={{ gridArea: 'rating' }}>
        <FilmRatingModule sx={{ mb: { lg: 0 } }} />
      </Box>

      <Box
        sx={{
          display: { xs: 'contents', lg: 'block' },
          gridArea: { lg: 'side' },
          alignSelf: 'start',
          position: { lg: 'sticky' },
          top: { lg: 0 },
          zIndex: { lg: 1 },
        }}
      >
        <Box sx={{ gridArea: { xs: 'viewers', lg: 'auto' } }}>
          <RoomViewersModule />
        </Box>

        <Box sx={{ gridArea: { xs: 'chat', lg: 'auto' } }}>
          <RoomChatModule />
        </Box>
      </Box>
    </Box>
  );
};

export default RoomPage;
