import { Box, Paper } from '@mui/material';
import { ReactElement, useCallback, useState } from 'react';

import useDelayedAction from '../../../hooks/delayed-action-hook/useDelayedAction.ts';
import TypingIndicator from '../../../ui/typing-indicator/TypingIndicator.tsx';
import ConnectUrl from '../../room/connect-url/ConnectUrl.tsx';
import { MessageDto } from '../../room/message/message.dto.ts';
import Message from '../../room/message/Message.tsx';
import SendMessageForm from '../../room/send-message-form/SendMessageForm.tsx';

/**
 * Примерный набор сообщений для демонстрации компонента ChatExample.
 * Используется для имитации истории чата.
 * @type {MessageDto[]}
 */
const messages: MessageDto[] = [
  {
    id: '7a508ca2-8f4c-4176-93c2-40a4a26767c3',
    isOutgoing: true,
    isOwner: true,
    sentAt: new Date('2025-10-07T23:01:51'),
    photoUrl: '/img/examples/user_thumbnail_b835b3ee-360a-4251-aac2-37fda8b1f4f4.jpg',
    text: 'Он реально выжил после этого?! 😂',
    userName: 'Егор',
    reactions: [{ code: 'laugh', emoji: '😂', count: 1, reacted: false, userNames: ['Astrey'] }],
  },
  {
    id: 'fd18d289-dbe5-4262-ae9a-509b08c069df',
    isOutgoing: false,
    isOwner: false,
    sentAt: new Date('2025-10-07T23:01:39'),
    photoUrl: '/img/examples/user_thumbnail_89cfdb4d-e4e2-4977-bf23-4d5a666cbf40.jpg',
    text: 'Сцена с погоней вообще топ 🔥',
    userName: 'Astrey',
    reactions: [{ code: 'fire', emoji: '🔥', count: 1, reacted: true, userNames: ['Егор'] }],
  },
];

/**
 * Компонент-пример, демонстрирующий работу чата комнаты.
 * Включает список сообщений, индикатор набора текста, форму отправки и компонент для копирования ссылки.
 * @returns {ReactElement} JSX-разметка примера чата.
 */
const ChatExample = (): ReactElement => {
  /**
   * Состояние, указывающее, была ли недавно нажата кнопка копирования ссылки.
   */
  const [isClicked, setIsClicked] = useState(false);

  /**
   * Хук с отложенным выполнением действия.
   * Используется для сброса состояния копирования через 5 секунд.
   * @returns {() => void} Функция запуска отложенного действия.
   */
  const handleClick = useDelayedAction(() => setIsClicked(false), 5000);

  /**
   * Обработчик нажатия на элемент копирования ссылки.
   * Устанавливает состояние "скопировано" и запускает таймер на сброс.
   * @returns {void}
   */
  const callback = useCallback((): void => {
    setIsClicked(true);
    handleClick();
  }, [handleClick]);

  /**
   * Заглушка без реализации, добавлена для полноты интерфейса.
   * @returns {void}
   */
  const handle = useCallback((): void => {}, []);

  return (
    <Paper>
      <Box sx={{ display: 'flex', flexDirection: 'column-reverse' }}>
        {messages.map((m) => (
          <Message key={m.id} message={m} />
        ))}
      </Box>

      <TypingIndicator message={'Astrey печатает'} />

      <SendMessageForm onSend={handle} onTyping={handle} />

      <ConnectUrl isClicked={isClicked} onClick={callback} />
    </Paper>
  );
};

export default ChatExample;
