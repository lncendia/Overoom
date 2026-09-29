import { useInjection } from 'inversify-react';
import { ReactNode, useCallback, useMemo } from 'react';

import NotEnoughRights from '../../components/pages/NotEnoughRights.tsx';
import { useAuthentication } from '../../contexts/authentication-context/useAuthentication.tsx';
import { AuthApi } from '../../services/auth/auth.api.ts';
import Spinner from '../../ui/spinners/Spinner.tsx';

/**
 * Модуль-guard для проверки авторизации пользователя и его роли.
 * Отображает дочерние элементы, если пользователь авторизован и соответствует требуемой роли.
 * Пока идет начальная проверка сессии, отображает спиннер.
 * @param props - Пропсы компонента
 * @param props.role - Роль пользователя для проверки доступа
 * @param props.children - JSX-элементы, которые рендерятся при успешной авторизации
 * @param props.showAuthPage - Флаг, указывающий, показывать ли страницу с уведомлением о недостатке прав
 * @returns {ReactNode | null} Дочерние элементы при доступе или компонент "Нет прав"
 */
const AuthorizeGuard = ({
  role,
  children,
  showAuthPage = true,
}: {
  role?: string;
  children: ReactNode;
  showAuthPage?: boolean;
}): ReactNode | null => {
  /** Хук для получения данных авторизованного пользователя */
  const { authorizedUser, isLoading } = useAuthentication();

  /** Инъекция сервиса аутентификации */
  const authApi = useInjection<AuthApi>('AuthApi');

  /**
   * Функция для инициирования входа пользователя через authApi
   */
  const login = useCallback(() => {
    authApi.signIn().then();
  }, [authApi]);

  /**
   * Определяет, показывать ли дочерние элементы
   * @returns true, если пользователь авторизован и соответствует роли, иначе false
   */
  const show = useMemo(() => {
    if (!authorizedUser) return false; // Если пользователь не авторизован
    if (!role) return true; // Если роль не указана, разрешаем доступ
    return authorizedUser.roles.includes(role); // Проверяем роль пользователя
  }, [authorizedUser, role]);

  if (show) return children;

  if (isLoading) return showAuthPage ? <Spinner /> : null;

  return showAuthPage ? <NotEnoughRights action={login} /> : null;
};

export default AuthorizeGuard;
