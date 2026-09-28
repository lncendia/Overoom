import { ReactElement } from 'react';

import SignOutModule from '../modules/authorization/SignOutModule.tsx';

/**
 * Страница выхода пользователя из системы.
 * Осуществляет деавторизацию и перенаправление при необходимости.
 * @returns {ReactElement} JSX-элемент страницы выхода
 */
const SignOutPage = (): ReactElement => {
  return <SignOutModule />;
};

export default SignOutPage;
