import { createContext } from 'react';

import { AuthorizedUserDto } from './authorized-user.dto.ts';

/** Интерфейс контекста аутентификации */
export interface AuthenticationContextType {
  /** Данные авторизованного пользователя или null если пользователь не авторизован */
  authorizedUser: AuthorizedUserDto | null;
}

/** Контекст аутентификации приложения */
export const AuthenticationContext = createContext<AuthenticationContextType | undefined>(
  undefined
);
