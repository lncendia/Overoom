import { useInjection } from 'inversify-react';
import { useEffect } from 'react';

import { AuthApi } from '../../services/auth/auth.api.ts';

/**
 * Модуль silent аутентификации пользователя.
 * Выполняется внутри скрытого iframe, поэтому ничего не отображает и не выполняет навигацию.
 * @returns {null} Ничего не отображает
 */
const SignInSilentModule = (): null => {
  /** Используем хук useInjection для получения экземпляра IAuthApi */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Передаем результат тихой аутентификации родительскому окну при монтировании компонента */
  useEffect(() => {
    authApi.signInSilentCallback().catch(() => undefined);
  }, [authApi]);

  return null;
};

export default SignInSilentModule;
