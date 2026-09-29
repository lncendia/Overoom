import { Alert, Box, Button, Typography } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, ReactNode, useCallback, useEffect, useState } from 'react';

import { RoomConnectionStatus } from '../../../contexts/room-context/RoomContext.tsx';
import { RoomContextProvider } from '../../../contexts/room-context/RoomContextProvider.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import { RoomHubFactory } from '../../../services/room-hub/room-hub.factory.ts';
import { RoomHub, RoomHubStatus } from '../../../services/room-hub/room.hub.ts';

/** Пропсы компонента ConnectHubModule */
interface ConnectHubModuleProps {
  /** Идентификатор комнаты для подключения */
  id: string;
  /** Дочерние элементы компонента */
  children: ReactNode;
  /** Флаг, подключен ли пользователь к комнате */
  joined?: boolean;
}

/** Максимальное время ожидания подтверждения подключения к комнате (мс) */
const CONNECT_TIMEOUT = 10_000;

/** Максимальное количество попыток подключения к комнате */
const MAX_CONNECT_ATTEMPTS = 5;

/** Базовая задержка между попытками подключения (мс) */
const BASE_RETRY_DELAY = 1000;

/**
 * Сообщения сервера, при которых повторять подключение бессмысленно
 * (см. HubExceptionFilter на сервере)
 */
const NON_TRANSIENT_ERRORS = new Set([
  'Комната не найдена',
  'Зритель не найден',
  'Действие не разрешено',
  'Данные указаны некорректно или в неверном формате',
]);

/** Ошибка подключения к комнате */
class RoomConnectError extends Error {
  /** Флаг временной ошибки, после которой имеет смысл повторить попытку */
  readonly transient: boolean;

  /**
   * Создает ошибку подключения к комнате
   * @param message - Текст ошибки
   * @param transient - Флаг временной ошибки
   */
  constructor(message: string, transient: boolean) {
    super(message);
    this.transient = transient;
  }
}

/**
 * Модуль подключения к SignalR хабу комнаты.
 * Управляет жизненным циклом подключения (включая автоматическое переподключение),
 * показывает состояние подключения и предоставляет контекст комнаты.
 * @param props - Пропсы компонента ConnectHubModule
 * @returns {ReactElement} Компонент модуля подключения к комнате
 */
const ConnectHubModule = (props: ConnectHubModuleProps): ReactElement => {
  /** Фабрика для создания экземпляров RoomHub */
  const roomHubFactory = useInjection<RoomHubFactory>('RoomHubFactory');

  /** Хук для управления подключением к комнате */
  const { hub, status, error, reconnectCount, retry } = useRoomConnection(
    roomHubFactory,
    props.id,
    props.joined ?? false
  );

  if (status === 'error') {
    return (
      <Box sx={{ py: 6, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
        <Typography variant="h6">Не удалось подключиться к комнате</Typography>
        {error && <Typography color="text.secondary">{error}</Typography>}
        <Button variant="contained" onClick={retry}>
          Повторить
        </Button>
      </Box>
    );
  }

  return (
    <RoomContextProvider hub={hub} connectionStatus={status} reconnectCount={reconnectCount}>
      {status === 'reconnecting' && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Соединение потеряно. Переподключаемся…
        </Alert>
      )}
      {status === 'disconnected' && (
        <Alert
          severity="error"
          sx={{ mb: 2 }}
          action={
            <Button color="inherit" size="small" onClick={retry}>
              Переподключиться
            </Button>
          }
        >
          {error ?? 'Соединение с комнатой потеряно'}
        </Alert>
      )}
      {props.children}
    </RoomContextProvider>
  );
};

export default ConnectHubModule;

/** Внутреннее состояние подключения: к статусам контекста добавляется ошибка первичного подключения */
type ConnectionState = RoomConnectionStatus | 'error';

/**
 * Одна попытка подключения к комнате на уже запущенном hub:
 * вызывает Connect и ждет события connectEvent (успех) либо errorNotificationEvent (ошибка).
 * @param hub - Запущенный экземпляр RoomHub
 * @param roomId - Идентификатор комнаты
 * @returns {Promise<void>} Promise, который разрешается после подтверждения подключения
 */
const connectHubOnce = (hub: RoomHub, roomId: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    /** Завершает ожидание: отписывается от событий и снимает таймер */
    const finish = () => {
      hub.removeHandler(handler);
      clearTimeout(timer);
    };

    const handler = (e: RoomEventContainer) => {
      if (e.connectEvent) {
        finish();
        resolve();
      } else if (e.errorNotificationEvent) {
        finish();
        const message = e.errorNotificationEvent.message;
        reject(new RoomConnectError(message, !NON_TRANSIENT_ERRORS.has(message)));
      }
    };

    const timer = setTimeout(() => {
      finish();
      reject(new RoomConnectError('Сервер не ответил на запрос подключения', true));
    }, CONNECT_TIMEOUT);

    hub.addHandler(handler);

    // Не ждем здесь результат: подтверждение приходит событием через handler
    hub.connect(roomId).catch((err) => {
      finish();
      reject(err);
    });
  });
};

/**
 * Подключение к комнате с повторами для временных ошибок
 * @param hub - Запущенный экземпляр RoomHub
 * @param roomId - Идентификатор комнаты
 * @param isCancelled - Функция, сообщающая, что подключение больше не нужно
 * @returns {Promise<void>} Promise, который разрешается после подключения или отмены
 */
const connectWithRetry = async (
  hub: RoomHub,
  roomId: string,
  isCancelled: () => boolean
): Promise<void> => {
  for (let attempt = 1; attempt <= MAX_CONNECT_ATTEMPTS; attempt++) {
    try {
      await connectHubOnce(hub, roomId);
      return;
    } catch (err) {
      if (isCancelled()) return;
      if (err instanceof RoomConnectError && !err.transient) throw err;
      if (attempt === MAX_CONNECT_ATTEMPTS) throw err;

      const delay = BASE_RETRY_DELAY * 2 ** (attempt - 1);
      console.warn(
        `Попытка ${attempt} подключения к комнате не удалась. Повтор через ${delay} мс.`,
        err
      );
      await new Promise((res) => setTimeout(res, delay));
      if (isCancelled()) return;
    }
  }
};

/**
 * Возвращает текст ошибки для отображения пользователю
 * @param err - Ошибка
 * @returns {string} Текст ошибки
 */
const errorMessage = (err: unknown): string =>
  err instanceof RoomConnectError ? err.message : 'Проверьте подключение к интернету';

/**
 * Хук для управления подключением к комнате (RoomHub).
 * Создает hub, подключается к комнате, повторно подключается к комнате после
 * автоматического переподключения транспорта и позволяет переподключиться вручную.
 * @param roomHubFactory - фабрика для создания RoomHub
 * @param roomId - идентификатор комнаты, к которой надо подключиться
 * @param joined - флаг, говорит, что пользователь уже присоединился к комнате на уровне сервиса комнат
 * @returns Экземпляр hub (null, пока подключение не подтверждено), состояние подключения,
 * текст ошибки, счетчик переподключений и функция повторного подключения
 */
const useRoomConnection = (roomHubFactory: RoomHubFactory, roomId: string, joined: boolean) => {
  /** Экземпляр RoomHub, доступный только после подтверждения подключения к комнате */
  const [hub, setHub] = useState<RoomHub | null>(null);

  /** Состояние подключения */
  const [status, setStatus] = useState<ConnectionState>('connecting');

  /** Текст последней ошибки подключения */
  const [error, setError] = useState<string | null>(null);

  /** Количество успешных переподключений текущего hub */
  const [reconnectCount, setReconnectCount] = useState(0);

  /** Номер попытки ручного подключения, его изменение пересоздает hub */
  const [attempt, setAttempt] = useState(0);

  /** Повторное подключение с созданием нового hub */
  const retry = useCallback(() => setAttempt((a) => a + 1), []);

  useEffect(() => {
    if (!joined) return;

    /** Флаг отмены: компонент размонтирован или начата новая попытка */
    let cancelled = false;
    const isCancelled = () => cancelled;
    const instance = roomHubFactory.create();

    setStatus('connecting');
    setError(null);
    setReconnectCount(0);

    /**
     * Обработчик изменения состояния транспорта.
     * После автоматического переподключения сервер не помнит комнату соединения,
     * поэтому заново вызываем Connect
     * @param hubStatus - новое состояние транспорта
     */
    const statusHandler = (hubStatus: RoomHubStatus) => {
      if (cancelled) return;

      if (hubStatus === 'reconnecting') {
        setStatus('reconnecting');
      } else if (hubStatus === 'disconnected') {
        setStatus('disconnected');
      } else {
        connectWithRetry(instance, roomId, isCancelled)
          .then(() => {
            if (cancelled) return;
            setStatus('connected');
            setReconnectCount((c) => c + 1);
          })
          .catch((err) => {
            if (cancelled) return;
            setError(errorMessage(err));
            setStatus('disconnected');
          });
      }
    };

    instance.addStatusHandler(statusHandler);

    const start = async () => {
      try {
        await instance.start();
        if (cancelled) {
          await instance.disconnect();
          return;
        }

        await connectWithRetry(instance, roomId, isCancelled);
        if (cancelled) return;

        setHub(instance);
        setStatus('connected');
      } catch (err) {
        if (cancelled) return;
        console.warn('Не удалось подключиться к комнате', err);
        setError(errorMessage(err));
        setStatus('error');
        instance.disconnect().catch(() => {});
      }
    };

    start().then();

    return () => {
      cancelled = true;
      instance.removeStatusHandler(statusHandler);
      instance.disconnect().catch(() => {});
      setHub(null);
    };
  }, [roomHubFactory, roomId, joined, attempt]);

  return { hub, status, error, reconnectCount, retry };
};
