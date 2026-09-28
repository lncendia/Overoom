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
  return (
    <>
      <AuthorizeGuard showAuthPage={false}>
        <BlockTitle title="Ваши комнаты" />
        <UserRoomsModule />
      </AuthorizeGuard>

      <BlockTitle title="Все комнаты" sx={{ mt: 3 }} />
      <RoomsModule />
    </>
  );
};

export default RoomsPage;
