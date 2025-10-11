/* eslint-disable @typescript-eslint/no-explicit-any */
import { useInjection } from 'inversify-react';
import { User, UserManager } from 'oidc-client';
import React, { useState, useEffect, ReactNode, ReactElement } from 'react';

import { AuthenticationContext } from './AuthenticationContext.tsx';
import { AuthorizedUserDto } from './authorized-user.dto.ts';
import { Configuration } from '../../container/configuration.ts';

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

  /** Получаем экземпляр UserManager из контейнера зависимостей */
  const userManager = useInjection<UserManager>('UserManager');

  /** Получаем экземпляр Configuration из контейнера зависимостей */
  const configuration = useInjection<Configuration>('Configuration');

  /** Используем useEffect для подписки на события загрузки пользователя и инициализации состояния */
  useEffect(() => {
    /**
     * Обработчик события загрузки пользователя
     * @param user - Объект пользователя OIDC
     */
    const onUserLoaded = (user: User) => {
      setAuthorizedUser(mapUser(user, configuration));
    };

    // Подписка на событие UserManager
    userManager.events.addUserLoaded(onUserLoaded);

    // Получаем текущего пользователя и проверяем его сессию
    userManager.getUser().then((user) => {
      if (!user) {
        return;
      } else if (user.expired) {
        userManager.signinSilent().then();
      } else {
        userManager.events.load(user);
      }
    });

    // Очистка подписки при размонтировании
    return () => {
      userManager.events.removeUserLoaded(onUserLoaded);
    };
  }, [userManager, configuration]);

  // Возвращаем провайдер контекста с текущим авторизованным пользователем
  return (
    <AuthenticationContext.Provider value={{ authorizedUser }}>
      {children}
    </AuthenticationContext.Provider>
  );
};

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
