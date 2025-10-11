import { useTheme } from '@mui/material';
import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { MessageDto } from '../message/message.dto.ts';
import Message from '../message/Message.tsx';

/** Пропсы компонента MessagesList */
interface ChatProps {
  /** Список сообщений чата */
  messages: MessageDto[];
  /** Флаг наличия дополнительных сообщений для подгрузки */
  hasMore: boolean;
  /** Функция загрузки следующих сообщений */
  next: () => void;
}

/**
 * Компонент списка сообщений с бесконечной прокруткой
 * @param props - Пропсы компонента
 * @param props.messages - Массив сообщений для отображения
 * @param props.hasMore - Флаг наличия дополнительных сообщений
 * @param props.next - Функция загрузки следующих сообщений
 * @returns {ReactElement} JSX элемент списка сообщений
 */
const MessagesList = (props: ChatProps): ReactElement => {
  // Настройки для компонента InfiniteScroll
  const scrollProps = {
    dataLength: props.messages.length,
    next: props.next,
    hasMore: props.hasMore,
    inverse: true, // отобразить сообщения в обратном порядке (последнее внизу)
    scrollableTarget: 'scrollableDiv',
    loader: <Spinner />,
  };

  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  return (
    <InfiniteScroll
      height="70vh"
      style={{
        display: 'flex',
        flexDirection: 'column-reverse', // отображаем последние сообщения внизу
        overflowY: 'auto',
        scrollbarWidth: 'thin',
        scrollbarColor: `${theme.palette.secondary.light} ${theme.palette.background.default}`,
      }}
      {...scrollProps}
    >
      {/* Отображаем каждое сообщение */}
      {props.messages.map((m) => (
        <Message key={m.id} message={m} />
      ))}
    </InfiniteScroll>
  );
};

export default MessagesList;
