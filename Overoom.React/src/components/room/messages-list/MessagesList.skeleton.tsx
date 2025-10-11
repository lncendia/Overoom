import { Box, useTheme } from '@mui/material';
import { ReactElement } from 'react';

import MessageSkeleton from '../message/Message.skeleton.tsx';

/**
 * Компонент скелетона списка сообщений в чате
 * @returns {ReactElement} JSX элемент скелетона списка сообщений
 */
const MessagesListSkeleton = (): ReactElement => {
  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  // Массив для генерации заглушек сообщений
  const skeletonMessages = Array.from({ length: 15 }, (_, i) => ({
    id: i,
    isOutgoing: i % 2 === 0,
  }));

  return (
    <Box
      sx={{
        height: '70vh',
        display: 'flex',
        flexDirection: 'column-reverse',
        overflowY: 'auto',
        scrollbarWidth: 'thin',
        scrollbarColor: `${theme.palette.secondary.light} ${theme.palette.background.default}`,
      }}
    >
      {skeletonMessages.map((m) => (
        // Скелетон сообщения
        <MessageSkeleton key={m.id} isOutgoing={m.isOutgoing} />
      ))}
    </Box>
  );
};

export default MessagesListSkeleton;
