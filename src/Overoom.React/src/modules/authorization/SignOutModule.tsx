import { useInjection } from 'inversify-react';
import { ReactElement, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

import Redirecting from '../../components/pages/Redirecting.tsx';
import { useSafeCallback } from '../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../services/auth/auth.api.ts';

/**
 * Модуль выхода из аккаунта
 * @returns {ReactElement} JSX элемент страницы перенаправления
 */
const SignOutModule = (): ReactElement => {
  /** Используем хук useNavigate для программной навигации между страницами */
  const navigate = useNavigate();

  /** Используем хук useInjection для получения экземпляра IAuthApi */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Флаг, исключающий повторную обработку callback */
  const handledRef = useRef(false);

  /** Колбэк для выполнения signOut callback и перенаправления */
  const handleSignOutCallback = useSafeCallback(async () => {
    try {
      await authApi.signOutCallback();
    } finally {
      navigate('/', { replace: true });
    }
  }, [authApi, navigate]);

  /** Выполнение при монтировании компонента */
  useEffect(() => {
    if (handledRef.current) return;
    handledRef.current = true;
    handleSignOutCallback().then();
  }, [handleSignOutCallback]);

  return <Redirecting />;
};

export default SignOutModule;
