import {
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Button,
  alpha,
} from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useMemo, useState } from 'react';

import FilmInfoSkeleton from '../../../components/film/film-info/FilmInfo.skeleton.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import FilmInfoModule from '../../films/film-module/FilmModule.tsx';

/**
 * Компонент для отображения информации о комнате и управления ею
 * @returns {ReactElement} JSX элемент карточки комнаты
 */
const RoomInfoModule = (): ReactElement => {
  /** Получение данных комнаты из контекста */
  const { room, currentViewerId } = useRoom();

  /** Состояние видимости диалога подтверждения действия */
  const [alertOpen, setAlertOpen] = useState(false);

  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /** Определение текста кнопки в зависимости от роли пользователя */
  const buttonText = useMemo(() => {
    if (!room?.ownerId) return null;
    if (room.ownerId === currentViewerId) return 'Удалить комнату';
    return 'Покинуть комнату';
  }, [currentViewerId, room?.ownerId]);

  /** Текст предупреждения в диалоге в зависимости от роли пользователя */
  const alertText = useMemo(() => {
    if (!room?.ownerId) return null;
    if (room?.ownerId === currentViewerId) {
      return (
        'Вы уверены, что хотите удалить комнату?\n' +
        'Все участники будут автоматически отключены и комната будет полностью удалена.\n' +
        'Это действие невозможно отменить.'
      );
    }
    return 'Вы уверены, что хотите покинуть комнату? Вы сможете присоединиться снова, если у вас есть приглашение или ссылка на комнату.';
  }, [currentViewerId, room?.ownerId]);

  /** Заголовок диалога в зависимости от действия */
  const alertTitle = useMemo(() => {
    if (!room?.ownerId) return null;
    if (room.ownerId === currentViewerId) {
      return 'Удаление комнаты';
    }
    return 'Выход из комнаты';
  }, [currentViewerId, room?.ownerId]);

  /** Обработчик подтверждения действия в диалоге */
  const onApplyClicked = useSafeCallback(async () => {
    if (!room?.ownerId || !room?.id) return;
    try {
      if (room.ownerId === currentViewerId) {
        await roomsApi.delete(room?.id);
      } else {
        await roomsApi.leave(room?.id);
      }
    } finally {
      setAlertOpen(false);
    }
  }, [room?.ownerId, room?.id, currentViewerId, roomsApi]);

  // Отображает скелетон пока данные комнаты не получены
  if (!room) return <FilmInfoSkeleton />;

  return (
    <>
      <FilmInfoModule
        buttonText={buttonText ?? undefined}
        onButtonClicked={() => setAlertOpen(true)}
      />

      {/* Диалог подтверждения действия */}
      <Dialog
        open={alertOpen}
        onClose={() => setAlertOpen(false)}
        sx={{
          '& .MuiDialog-paper': {
            backgroundColor: (theme) => alpha(theme.palette.background.paper, 1),
          },
        }}
      >
        <DialogTitle id="alert-dialog-title">{alertTitle}</DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">{alertText}</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAlertOpen(false)} color="secondary">
            Отмена
          </Button>
          <Button
            onClick={onApplyClicked}
            color={room.ownerId === currentViewerId ? 'error' : 'secondary'}
            autoFocus
          >
            {room.ownerId === currentViewerId ? 'Удалить' : 'Покинуть'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default RoomInfoModule;
