import { ReactElement, useCallback } from 'react';

import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { ViewerDto } from '../../room/viewer/viewer.dto.ts';
import ViewersList from '../../room/viewers-list/ViewersList.tsx';

/**
 * Моковые данные зрителей для демонстрации списка.
 * Используются для примера компонента ViewersListExample.
 */
const viewers: ViewerDto[] = [
  {
    id: 'b835b3ee-360a-4251-aac2-37fda8b1f4f4',
    userName: 'Егор',
    photoUrl: 'img/examples/user_thumbnail_b835b3ee-360a-4251-aac2-37fda8b1f4f4.jpg',
    tags: [
      { name: '🧑‍✈️ Ведущий', description: 'Тот, кто рулит просмотром. Наш капитан.' },
      { name: '⏱️ Стоп-Хам', description: 'Ставил видео на паузу более 5 раз. У него свои ритмы.' },
      { name: '📢 Бипер', description: 'Бипает чаще, чем дышит. Всегда напомнит о себе.' },
    ],
    online: true,
    isOwner: true,
    isCurrent: true,
    canBeep: false,
    canScream: false,
    canKick: false,
    canSync: false,
    typing: false,
    fullScreen: true,
    onPause: true,
    timeLine: 18660408380,
    season: 1,
    episode: 2,
  },
  {
    id: '89cfdb4d-e4e2-4977-bf23-4d5a666cbf40',
    userName: 'Astrey',
    photoUrl: 'img/examples/user_thumbnail_89cfdb4d-e4e2-4977-bf23-4d5a666cbf40.jpg',
    tags: [
      { name: '🎟️ Премиум', description: 'Всегда первый в комнате. Легенда.' },
      {
        name: '🙉 Забибиканный',
        description: 'Постоянно получает бипы. Когда же его оставят в покое?',
      },
      {
        name: '🗯️ Болтун',
        description: 'Отправил более 50 сообщений. Может стоит посмотреть фильм?',
      },
      { name: '🕵️‍♂️ Иноагент', description: 'Имя написано не кириллицей. Под подозрением.' },
    ],
    online: true,
    isOwner: false,
    isCurrent: false,
    canBeep: true,
    canScream: true,
    canKick: true,
    canSync: false,
    typing: false,
    fullScreen: false,
    onPause: true,
    timeLine: 21915446220,
    season: 1,
    episode: 2,
  },
];

/**
 * Компонент-пример, демонстрирующий работу ViewersList с моковыми данными.
 * Подключает контекст уведомлений и эмулирует действия пользователя.
 * @returns {ReactElement} JSX-разметка с примером списка зрителей.
 */
const ViewersListExample = (): ReactElement => {
  const { setNotification } = useNotify();

  /**
   * Возвращает имя зрителя по его идентификатору.
   * @param {string} viewerId — Идентификатор зрителя.
   * @returns {string} Имя зрителя или "Неизвестный", если не найден.
   */
  const getViewerUserName = useCallback((viewerId: string): string => {
    return viewers.find((v) => v.id === viewerId)?.userName ?? 'Неизвестный';
  }, []);

  /**
   * Обработчик действия "Выгнать зрителя".
   * Отображает уведомление об удалении пользователя из комнаты.
   * @param {string} id — Идентификатор зрителя.
   * @returns {void}
   */
  const handleKick = useCallback(
    (id: string): void => {
      const userName = getViewerUserName(id);
      setNotification({
        message: `Вы выгнали ${userName}`,
        severity: 'error',
      });
    },
    [getViewerUserName, setNotification]
  );

  /**
   * Обработчик действия "Бипнуть зрителя".
   * Отображает уведомление о посланном звуковом сигнале.
   * @param {string} id — Идентификатор зрителя.
   * @returns {void}
   */
  const handleBeep = useCallback(
    (id: string): void => {
      const userName = getViewerUserName(id);
      setNotification({
        message: `Вы разбудили ${userName}`,
        severity: 'info',
      });
    },
    [getViewerUserName, setNotification]
  );

  /**
   * Обработчик действия "Крикнуть на зрителя".
   * Отображает предупреждающее уведомление.
   * @param {string} id — Идентификатор зрителя.
   * @returns {void}
   */
  const handleScream = useCallback(
    (id: string): void => {
      const userName = getViewerUserName(id);
      setNotification({
        message: `Вы напугали ${userName}`,
        severity: 'warning',
      });
    },
    [getViewerUserName, setNotification]
  );

  /**
   * Заглушка без реализации, добавлена для полноты интерфейса.
   * @returns {void}
   */
  const handleSync = useCallback((): void => {}, []);

  return (
    <ViewersList
      viewers={viewers}
      isCollapsed={true}
      onKick={handleKick}
      onScream={handleScream}
      onBeep={handleBeep}
      onSync={handleSync}
    />
  );
};

export default ViewersListExample;
