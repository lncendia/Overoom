import { ReactElement } from 'react';

import SignInSilentModule from '../modules/authorization/SignInSilentModule.tsx';

/**
 * Страница silent-входа пользователя.
 * Используется для фоновой авторизации без участия пользователя (внутри скрытого iframe).
 * @returns {ReactElement} JSX-элемент страницы silent-входа
 */
const SignInSilentPage = (): ReactElement => {
  return <SignInSilentModule />;
};

export default SignInSilentPage;
