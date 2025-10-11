import { Grid } from '@mui/material';
import { ReactElement } from 'react';

import { ProfileContextProvider } from '../contexts/profile-context/ProfileContextProvider.tsx';
import AuthorizeGuard from '../modules/guards/AuthorizeGuard.tsx';
import ProfileSettingsModule from '../modules/profile/profile-settings-module/ProfileSettingsModule.tsx';
import UserHistoryModule from '../modules/profile/user-history-module/UserHistoryModule.tsx';
import UserInfoModule from '../modules/profile/user-info-module/UserInfoModule.tsx';
import UserNotificationSettingsModule from '../modules/profile/user-notification-settings-module/UserNotificationSettingsModule.tsx';
import UserRatingsModule from '../modules/profile/user-ratings-module/UserRatingsModule.tsx';
import UserWatchlistModule from '../modules/profile/user-watchlist-module/UserWatchlistModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Страница профиля пользователя.
 * Оборачивает содержимое в защиту авторизации и провайдер контекста профиля.
 * @returns {ReactElement} JSX-элемент страницы профиля пользователя
 */
const ProfilePage = (): ReactElement => {
  // Оборачиваем содержимое профиля в Guard и контекст
  return (
    <AuthorizeGuard>
      <ProfileContextProvider>
        <ProfilePageContent />
      </ProfileContextProvider>
    </AuthorizeGuard>
  );
};

/**
 * Внутреннее содержимое страницы профиля.
 * Отображает основную информацию о пользователе, историю, оценки, список "смотреть позже" и настройки.
 * @returns {ReactElement} JSX-элемент с контентом страницы профиля
 */
const ProfilePageContent = (): ReactElement => {
  // Используем Grid для разделения контента на основную и боковую колонки
  return (
    <Grid container columnSpacing={4}>
      {/* Основная колонка с информацией о пользователе и активностью */}
      <Grid size={{ xs: 12, lg: 8 }} order={{ xs: 2, lg: 1 }}>
        <UserInfoModule />

        <BlockTitle title="Смотреть позже" sx={{ mt: 5 }} />
        <UserWatchlistModule />

        <BlockTitle title="История" sx={{ mt: 5 }} />
        <UserHistoryModule />

        <BlockTitle title="Оценки" sx={{ mt: 5 }} />
        <UserRatingsModule />
      </Grid>

      {/* Боковая колонка с настройками профиля */}
      <Grid size={{ xs: 12, lg: 4 }} order={{ xs: 1, lg: 2 }}>
        <UserNotificationSettingsModule />
        <ProfileSettingsModule />
      </Grid>
    </Grid>
  );
};

export default ProfilePage;
