import { ReactElement, useCallback, useState } from 'react';

import ConnectUrl from '../../../components/room/connect-url/ConnectUrl.tsx';
import useDelayedAction from '../../../hooks/delayed-action-hook/useDelayedAction.ts';
import { routes } from '../../../routes.ts';

/**
 * Компонент для генерации и копирования ссылки для подключения к комнате
 * @param {object} props - Свойства компонента
 * @param {string} props.id - Идентификатор комнаты
 * @param {string | null} [props.code] - Дополнительный код доступа (опционально)
 * @returns {ReactElement} JSX-элемент ссылки подключения к комнате
 */
const ConnectLink = ({ id, code }: { code: string | null; id: string }): ReactElement => {
  /** Состояние для отслеживания факта копирования ссылки */
  const [isClicked, setIsClicked] = useState(false);

  /** Хук для выполнения действия с задержкой */
  const handleClick = useDelayedAction(() => setIsClicked(false), 5000);

  /**
   * Обработчик клика по кнопке "Копировать ссылку"
   * Формирует URL с идентификатором комнаты и кодом доступа и копирует его в буфер обмена
   */
  const callback = useCallback(() => {
    const newUrl = `${window.location.origin}${routes.room(id, code)}`;
    navigator.clipboard
      .writeText(newUrl)
      .then(() => {
        setIsClicked(true);
        handleClick();
      })
      .catch((err) => console.warn('Не удалось скопировать ссылку', err));
  }, [code, handleClick, id]);

  return <ConnectUrl isClicked={isClicked} onClick={callback} />;
};

export default ConnectLink;
