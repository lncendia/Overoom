import { ReactElement } from 'react';

import AuthorizeGuard from '../modules/guards/AuthorizeGuard.tsx';
import RoomsModule from '../modules/rooms/rooms-module/RoomsModule.tsx';
import UserRoomsModule from '../modules/rooms/user-rooms-module/UserRoomsModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Страница комнат.
 * Отображает список всех доступных комнат и, при авторизации, — персональные комнаты пользователя.
 * @returns {ReactElement} JSX-элемент страницы со списками комнат
 */
const RoomsPage = (): ReactElement => {
  // Если пользователь авторизован — показываем блок с его личными комнатами
  // Если нет — блок не отображается, но страница остаётся доступной
  return (
    <>
      <AuthorizeGuard showAuthPage={false}>
        <BlockTitle title="Ваши комнаты" />
        <UserRoomsModule />
      </AuthorizeGuard>

      {/* Заголовок и модуль отображения всех комнат */}
      <BlockTitle title="Все комнаты" sx={{ mt: 3 }} />
      <RoomsModule />
    </>
  );
};

export default RoomsPage;
