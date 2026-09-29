import axios, { AxiosInstance, InternalAxiosRequestConfig, isAxiosError } from 'axios';
import { Container } from 'inversify';
import { User, UserManager, UserManagerSettings, WebStorageStateStore } from 'oidc-client-ts';

import { Configuration } from './configuration.ts';
import { AuthApi } from '../services/auth/auth.api.ts';
import { CommentsApi } from '../services/comments/comments.api.ts';
import { FilmsApi } from '../services/films/films.api.ts';
import { JobsApi } from '../services/jobs/jobs.api.ts';
import { PlaylistsApi } from '../services/playlists/playlists.api.ts';
import { ProfileApi } from '../services/profile/profile.api.ts';
import { RoomHubFactory } from '../services/room-hub/room-hub.factory.ts';
import { RoomsApi } from '../services/rooms/rooms.api.ts';

/**
 * Создает контейнер зависимостей для приложения
 * @returns {Promise<Container>} - Возвращает промис, который резолвится в настроенный контейнер Inversify
 */
const createContainer = async (): Promise<Container> => {
  const response = await axios.get<Configuration>('/configuration.json');
  const config = response.data;
  const container = new Container();
  const filmsAxiosInstance = axios.create(config.services.films);
  configureAxiosAuthorization(filmsAxiosInstance, container);
  const uploaderAxiosInstance = axios.create(config.services.uploader);
  configureAxiosAuthorization(uploaderAxiosInstance, container);

  const userManagerSettings: UserManagerSettings = {
    ...config.oidc,
    userStore: new WebStorageStateStore({ store: localStorage }),
  };

  const posterUrlFormat = `${config.services.films.baseURL}${config.files.filmThumbnailPrefix}`;
  const userThumbnailUrlFormat = `${config.oidc.authority}${config.files.userThumbnailPrefix}`;

  container
    .bind<Configuration>('Configuration')
    .toDynamicValue(() => config)
    .inSingletonScope();

  container
    .bind<UserManager>('UserManager')
    .toDynamicValue(() => new UserManager(userManagerSettings))
    .inSingletonScope();

  container
    .bind<AuthApi>('AuthApi')
    .toDynamicValue(() => new AuthApi(container.get('UserManager')))
    .inSingletonScope();

  container
    .bind<FilmsApi>('FilmsApi')
    .toDynamicValue(() => new FilmsApi(filmsAxiosInstance, posterUrlFormat))
    .inSingletonScope();

  container
    .bind<PlaylistsApi>('PlaylistsApi')
    .toDynamicValue(() => new PlaylistsApi(filmsAxiosInstance, posterUrlFormat))
    .inSingletonScope();

  container
    .bind<ProfileApi>('ProfileApi')
    .toDynamicValue(
      () => new ProfileApi(filmsAxiosInstance, posterUrlFormat, userThumbnailUrlFormat)
    )
    .inSingletonScope();

  container
    .bind<RoomsApi>('RoomsApi')
    .toDynamicValue(() => new RoomsApi(filmsAxiosInstance, posterUrlFormat, userThumbnailUrlFormat))
    .inSingletonScope();

  container
    .bind<CommentsApi>('CommentsApi')
    .toDynamicValue(() => new CommentsApi(filmsAxiosInstance, userThumbnailUrlFormat))
    .inSingletonScope();

  container
    .bind<JobsApi>('JobsApi')
    .toDynamicValue(() => new JobsApi(uploaderAxiosInstance))
    .inSingletonScope();

  container
    .bind<RoomHubFactory>('RoomHubFactory')
    .toDynamicValue(
      () =>
        new RoomHubFactory(
          () => tokenFactory(container),
          config.services.rooms.baseURL,
          userThumbnailUrlFormat
        )
    )
    .inSingletonScope();

  return container;
};

/** Конфигурация запроса с флагом повторной попытки после обновления токена */
interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  /** Запрос уже был повторен после попытки обновления токена */
  _authRetried?: boolean;
}

/**
 * Получает актуального пользователя: если срок действия токена истек,
 * пытается тихо обновить сессию (ошибки обновления игнорируются)
 * @param container - Контейнер Inversify для получения UserManager и AuthApi
 * @returns {Promise<User | null>} Пользователь с действующим токеном или null
 */
async function getActualUser(container: Container): Promise<User | null> {
  const userManager = container.get<UserManager>('UserManager');
  const user = await userManager.getUser();

  if (!user) return null;
  if (!user.expired) return user;

  const renewed = await container.get<AuthApi>('AuthApi').trySignInSilent();
  return renewed && !renewed.expired ? renewed : null;
}

/**
 * Настраивает Axios для автоматической подстановки токена авторизации в заголовок
 * и однократного повтора запроса после обновления токена при ответе 401
 * @param axiosInstance - Экземпляр Axios
 * @param container - Контейнер Inversify для получения UserManager
 */
function configureAxiosAuthorization(axiosInstance: AxiosInstance, container: Container): void {
  axiosInstance.interceptors.request.use(async (config) => {
    const user = await getActualUser(container);

    if (user?.access_token) {
      config.headers.Authorization = `Bearer ${user.access_token}`;
    }

    return config;
  });

  axiosInstance.interceptors.response.use(undefined, async (error: unknown) => {
    if (!isAxiosError(error) || error.response?.status !== 401 || !error.config) {
      throw error;
    }

    const config = error.config as RetryableRequestConfig;

    // Запрос без токена или уже повторенный запрос не обновляем, чтобы избежать зацикливания
    if (config._authRetried || !config.headers.Authorization) {
      throw error;
    }

    config._authRetried = true;

    const userManager = container.get<UserManager>('UserManager');
    const renewed = await container.get<AuthApi>('AuthApi').trySignInSilent();

    if (!renewed?.access_token || renewed.expired) {
      await userManager.removeUser().catch(() => undefined);
      throw error;
    }

    config.headers.Authorization = `Bearer ${renewed.access_token}`;
    return axiosInstance.request(config);
  });
}

/**
 * Асинхронная функция для получения токена доступа текущего пользователя
 * @param container - Контейнер Inversify для получения UserManager
 * @returns {Promise<string>} - Возвращает токен доступа или пустую строку
 */
async function tokenFactory(container: Container): Promise<string> {
  const user = await getActualUser(container);
  return user?.access_token ?? '';
}

export default createContainer;
