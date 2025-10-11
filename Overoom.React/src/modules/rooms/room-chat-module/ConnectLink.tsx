import { ReactElement, useCallback, useState } from 'react';

import ConnectUrl from '../../../components/room/connect-url/ConnectUrl.tsx';
import useDelayedAction from '../../../hooks/delayed-action-hook/useDelayedAction.ts';

/**
 * Компонент для генерации и копирования ссылки для подключения к комнате
 * @param {object} props - Свойства компонента
 * @param {string} props.id - Идентификатор комнаты
 * @param {string} props.endpoint - Конечная точка URL (путь)
 * @param {string | null} [props.code] - Дополнительный код доступа (опционально)
 * @returns {ReactElement} JSX-элемент ссылки подключения к комнате
 */
const ConnectLink = ({
  id,
  endpoint,
  code,
}: {
  code: string | null;
  id: string;
  endpoint: string;
}): ReactElement => {
  /** Состояние для отслеживания факта копирования ссылки */
  const [isClicked, setIsClicked] = useState(false);

  /** Хук для выполнения действия с задержкой */
  const handleClick = useDelayedAction(() => setIsClicked(false), 5000);

  /**
   * Обработчик клика по кнопке "Копировать ссылку"
   * Формирует URL с идентификатором комнаты и кодом доступа и копирует его в буфер обмена
   */
  const callback = useCallback(() => {
    // Создаем объект для работы с query-параметрами
    const searchParams = new URLSearchParams();

    // Добавляем обязательный параметр "id" комнаты
    searchParams.set('id', id);

    // Добавляем код доступа, если он предоставлен
    if (code) searchParams.set('code', code);

    // Формируем полный URL
    const newUrl = `${window.location.origin}/${endpoint}?${searchParams.toString()}`;

    // Копируем URL в буфер обмена
    navigator.clipboard.writeText(newUrl).then();

    // Устанавливаем флаг "скопировано"
    setIsClicked(true);

    // Сбрасываем флаг через 5 секунд
    handleClick();
  }, [code, endpoint, handleClick, id]);

  return <ConnectUrl isClicked={isClicked} onClick={callback} />;
};

export default ConnectLink;
