import axios, { AxiosInstance } from 'axios';
import { Container } from 'inversify';
import { UserManager, UserManagerSettings, WebStorageStateStore } from 'oidc-client';

import { Configuration } from './configuration.ts';
import { AuthApi } from '../services/auth/auth.api.ts';
import { CommentsApi } from '../services/comments/comments.api.ts';
import { FilmsApi } from '../services/films/films.api.ts';
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

/**
 * Настраивает Axios для автоматической подстановки токена авторизации в заголовок
 * @param axiosInstance - Экземпляр Axios
 * @param container - Контейнер Inversify для получения UserManager
 */
function configureAxiosAuthorization(axiosInstance: AxiosInstance, container: Container): void {
  axiosInstance.interceptors.request.use(async (config) => {
    const userManager = container.get<UserManager>('UserManager');
    const user = await userManager.getUser();

    if (user && user.access_token) {
      config.headers.Authorization = `Bearer ${user.access_token}`;
    }

    return config;
  });
}

/**
 * Асинхронная функция для получения токена доступа текущего пользователя
 * @param container - Контейнер Inversify для получения UserManager
 * @returns {Promise<string>} - Возвращает токен доступа или пустую строку
 */
async function tokenFactory(container: Container): Promise<string> {
  const userManager = container.get<UserManager>('UserManager');
  const user = await userManager.getUser();

  if (user && user.access_token) {
    return user.access_token;
  }

  return '';
}

export default createContainer;
