import { useInjection } from 'inversify-react';
import { ReactElement, ReactNode, useCallback, useEffect, useState } from 'react';

import { RoomContextProvider } from '../../../contexts/room-context/RoomContextProvider.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import { RoomHubFactory } from '../../../services/room-hub/room-hub.factory.ts';
import { RoomHub } from '../../../services/room-hub/room.hub.ts';

/** Пропсы компонента ConnectHubModule */
interface ConnectHubModuleProps {
  /** Идентификатор комнаты для подключения */
  id: string;
  /** Дочерние элементы компонента */
  children: ReactNode;
  /** Флаг, подключен ли пользователь к комнате */
  joined?: boolean;
}

/**
 * Модуль для подключения к комнате просмотра фильма.
 * Обрабатывает приватные комнаты с кодом доступа, управляет состоянием подключения
 * и предоставляет контексты для работы с фильмом и комнатой.
 * @param props - Пропсы компонента ConnectHubModule
 * @returns {ReactElement} Компонент модуля подключения к комнате
 */
const ConnectHubModule = (props: ConnectHubModuleProps): ReactElement => {
  /** Фабрика для создания экземпляров RoomHub */
  const roomHubFactory = useInjection<RoomHubFactory>('RoomHubFactory');

  /** Хук для управления подключением к комнате */
  const hub = useRoomConnection(roomHubFactory, props.id, props.joined ?? false);

  // При успешном подключении предоставляем контекст комнаты
  return (
    <RoomContextProvider id={props.id} hub={hub}>
      {props.children}
    </RoomContextProvider>
  );
};

export default ConnectHubModule;

/**
 * Хук для управления подключением к комнате (RoomHub).
 * @param roomHubFactory - фабрика для создания RoomHub
 * @param roomId - идентификатор комнаты, к которой надо подключиться
 * @param joined - флаг, говорит, что пользователь/клиент уже присоединился к комнате на уровне сервиса фильмов
 * @returns {RoomHub | null} — экземпляр hub, если подключение подтверждено; иначе null
 */
const useRoomConnection = (
  roomHubFactory: RoomHubFactory,
  roomId: string,
  joined: boolean
): RoomHub | null => {
  /** Экземпляр RoomHub, может быть null до создания или после дисконнекта */
  const [hub, setHub] = useState<RoomHub | null>(null);

  /** Флаг локального подтверждения, что мы успешно прошли handshake с hub (получили событие connected) */
  const [isConnected, setIsConnected] = useState(false);

  /**
   * Одна попытка подключения к hub:
   * - Подписываемся на события hub (handler).
   * - Вызываем hub.connect(roomId).
   * - Ждём либо события "connectEvent" → resolve, либо "errorNotificationEvent" → reject.
   */
  const connectHubOnce = useCallback(
    (hub: RoomHub): Promise<void> => {
      return new Promise((resolve, reject) => {
        // Обработчик входящих событий от хаба
        const handler = (e: RoomEventContainer) => {
          // Если пришло событие подтверждающее, что текущий клиент/пользователь успешно подключён к комнате
          if (e.connectEvent) {
            hub.removeHandler(handler);
            resolve();
            return;
          }

          // Если пришла ошибка — отписываемся и реджектим промис, чтобы верхний слой мог retry.
          if (e.errorNotificationEvent) {
            hub.removeHandler(handler);
            reject(new Error(e.errorNotificationEvent.message));
            return;
          }
        };

        // Подписываем handler на события hub
        hub.addHandler(handler);

        // Выполняем сам connect — не await'им здесь, потому что мы ждём события через handler.
        hub.connect(roomId).catch((err) => {
          hub.removeHandler(handler);
          reject(err);
        });
      });
    },
    [roomId]
  );

  /**
   * Высокоуровневая логика подключения с retry.
   * - Пытаемся выполнить connectHubOnce.
   * - При ошибке повторяем несколько раз с паузой.
   * - После успешного подключения устанавливаем isConnected = true.
   */
  const connectToHub = useSafeCallback(
    async (hub: RoomHub) => {
      const maxAttempts = 5; // максимальное количество попыток
      const baseDelay = 1000; // базовая задержка между повторами (мс)

      for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
          // ждем подтверждения от сервера (через события)
          await connectHubOnce(hub);
          // успешно — ставим локальный флаг и выходим
          setIsConnected(true);
          return;
        } catch (err) {
          // Если достигнут лимит попыток — пробрасываем ошибку дальше
          if (attempt === maxAttempts) throw err;

          // Вычисляем экспоненциальную задержку
          const delay = baseDelay * 2 ** (attempt - 1);

          console.warn(
            `Попытка ${attempt} подключения к хабу не удалась. Повтор через ${delay} мс.`,
            err
          );

          // Пауза перед повторной попыткой
          await new Promise((res) => setTimeout(res, delay));
        }
      }
    },
    [connectHubOnce]
  );

  /** Создаёт и стартует hub (один раз при монтировании). */
  const createHub = useSafeCallback(async () => {
    const hub = roomHubFactory.create();
    await hub.start();
    setHub(hub);
    return hub;
  }, [roomHubFactory]);

  /** Эффект: когда у нас есть hub и joined === true — запускаем логику подключения. */
  useEffect(() => {
    // не пытаемся ничего делать, пока пользователь не joined (не подключился к комнате)
    if (!joined || !hub) return;

    // вызов соединения
    connectToHub(hub).then();

    // cleanup — при размонтировании/изменении зависимостей сбрасываем флаг
    return () => {
      setIsConnected(false);
    };
  }, [connectToHub, joined, hub]);

  /** Эффект создания hub: создаём hub при монтировании и в cleanup — корректно отключаем и убираем hub. */
  useEffect(() => {
    let hubInstance: RoomHub | null = null;

    createHub().then((hub) => {
      hubInstance = hub;
    });

    return () => {
      // Отключаем hub и удаляем ссылку, чтобы избежать утечек
      setHub(null);
      hubInstance?.disconnect();
    };
  }, [createHub]);

  // возвращаем hub только когда подтверждена успешная handshake-логика (isConnected = true).
  // Это облегчает потребителям — они получают рабочий hub или null.
  return isConnected ? hub : null;
};
