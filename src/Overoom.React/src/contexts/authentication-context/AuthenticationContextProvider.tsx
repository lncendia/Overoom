/* eslint-disable @typescript-eslint/no-explicit-any */
import { useInjection } from 'inversify-react';
import { User, UserManager } from 'oidc-client-ts';
import React, { useState, useEffect, ReactNode, ReactElement, useCallback, useMemo } from 'react';

import { AuthenticationContext, AuthenticationContextType } from './AuthenticationContext.tsx';
import { AuthorizedUserDto } from './authorized-user.dto.ts';
import { Configuration } from '../../container/configuration.ts';
import { AuthApi } from '../../services/auth/auth.api.ts';

/**
 * Пропсы компонента AuthenticationContextProvider
 */
interface AuthenticationContextProviderProps {
  /** Дочерний элемент, который будет обернут провайдером */
  children: ReactNode;
}

/**
 * Компонент провайдера контекста аутентификации
 * @param props - Пропсы компонента
 * @param props.children - Дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера аутентификации
 */
export const AuthenticationContextProvider: React.FC<AuthenticationContextProviderProps> = ({
  children,
}): ReactElement => {
  /** Используем useState для хранения текущего авторизованного пользователя */
  const [authorizedUser, setAuthorizedUser] = useState<AuthorizedUserDto | null>(null);

  /** Флаг начальной загрузки пользователя */
  const [isLoading, setIsLoading] = useState(true);

  /** Получаем экземпляр UserManager из контейнера зависимостей */
  const userManager = useInjection<UserManager>('UserManager');

  /** Получаем экземпляр AuthApi из контейнера зависимостей */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Получаем экземпляр Configuration из контейнера зависимостей */
  const configuration = useInjection<Configuration>('Configuration');

  /**
   * Обновляет пользователя в состоянии, сохраняя прежний объект, если данные не изменились
   * @param user - Объект пользователя OIDC или null
   */
  const updateUser = useCallback(
    (user: User | null) => {
      const next = user ? mapUser(user, configuration) : null;
      setAuthorizedUser((prev) => (isSameUser(prev, next) ? prev : next));
    },
    [configuration]
  );

  /** Используем useEffect для подписки на события пользователя и инициализации состояния */
  useEffect(() => {
    /** Флаг размонтирования компонента */
    let disposed = false;

    /**
     * Обработчик события загрузки пользователя
     * @param user - Объект пользователя OIDC
     */
    const onUserLoaded = (user: User) => {
      updateUser(user);
    };

    /** Обработчик выгрузки пользователя */
    const onUserUnloaded = () => {
      updateUser(null);
    };

    /** Обработчик выхода пользователя на стороне сервера авторизации */
    const onUserSignedOut = () => {
      updateUser(null);
      userManager.removeUser().catch(() => undefined);
    };

    /** Обработчик ошибки автоматического обновления токена */
    const onSilentRenewError = () => {
      updateUser(null);
      userManager.removeUser().catch(() => undefined);
    };

    /** Обработчик истечения срока действия токена */
    const onAccessTokenExpired = async () => {
      const user = await authApi.trySignInSilent();
      if (!user) {
        updateUser(null);
        await userManager.removeUser().catch(() => undefined);
      }
    };

    userManager.events.addUserLoaded(onUserLoaded);
    userManager.events.addUserUnloaded(onUserUnloaded);
    userManager.events.addUserSignedOut(onUserSignedOut);
    userManager.events.addSilentRenewError(onSilentRenewError);
    userManager.events.addAccessTokenExpired(onAccessTokenExpired);

    /** Начальная загрузка пользователя из хранилища */
    const init = async () => {
      const user = await userManager.getUser();

      if (!user) return;

      if (!user.expired) {
        updateUser(user);
        return;
      }

      // Событие userLoaded при успешном обновлении установит пользователя
      const renewed = await authApi.trySignInSilent();
      if (!renewed) {
        await userManager.removeUser().catch(() => undefined);
        updateUser(null);
      }
    };

    init()
      .catch(() => updateUser(null))
      .finally(() => {
        if (!disposed) setIsLoading(false);
      });

    return () => {
      disposed = true;
      userManager.events.removeUserLoaded(onUserLoaded);
      userManager.events.removeUserUnloaded(onUserUnloaded);
      userManager.events.removeUserSignedOut(onUserSignedOut);
      userManager.events.removeSilentRenewError(onSilentRenewError);
      userManager.events.removeAccessTokenExpired(onAccessTokenExpired);
    };
  }, [userManager, authApi, updateUser]);

  /** Мемоизированное значение контекста */
  const value = useMemo<AuthenticationContextType>(
    () => ({ authorizedUser, isLoading }),
    [authorizedUser, isLoading]
  );

  return <AuthenticationContext.Provider value={value}>{children}</AuthenticationContext.Provider>;
};

/**
 * Сравнивает данные двух пользователей
 * @param a - Первый пользователь
 * @param b - Второй пользователь
 * @returns {boolean} true, если данные пользователей совпадают
 */
function isSameUser(a: AuthorizedUserDto | null, b: AuthorizedUserDto | null): boolean {
  if (a === b) return true;
  if (!a || !b) return false;
  return (
    a.id === b.id &&
    a.userName === b.userName &&
    a.email === b.email &&
    a.locale === b.locale &&
    a.photoUrl === b.photoUrl &&
    a.roles.length === b.roles.length &&
    a.roles.every((role, index) => role === b.roles[index])
  );
}

/**
 * Преобразует объект пользователя OIDC в объект AuthorizedUserDto
 * @param {User} user - Пользователь из UserManager
 * @param {Configuration} config - Конфигурация приложения
 * @returns {AuthorizedUserDto} Объект с данными авторизованного пользователя
 */
function mapUser(user: User, config: Configuration): AuthorizedUserDto {
  const userClaims = user.profile as any;
  const userRoles = userClaims.role;

  let roles: string[] = [];
  if (userRoles) {
    if (typeof userRoles === 'string') roles = [userRoles];
    else roles = userRoles as Array<string>;
  }

  let profilePhoto: string | null = null;
  if (user.profile.picture) {
    profilePhoto = `${config.oidc.authority}${config.files.userThumbnailPrefix}${user.profile.picture}`;
  }

  return {
    photoUrl: profilePhoto,
    email: user.profile.email!,
    id: user.profile.sub,
    locale: user.profile.locale!,
    userName: user.profile.name!,
    roles: roles,
  };
}
