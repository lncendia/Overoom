import { Paper, Button, Skeleton } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement } from 'react';

import { Configuration } from '../../../container/configuration.ts';
import { useProfile } from '../../../contexts/profile-context/useProfile.tsx';

/**
 * Компонент модуля настроек профиля.
 * Предоставляет доступ к настройкам аккаунта через внешний сервис аутентификации
 * @returns {ReactElement} JSX элемент модуля настроек профиля
 */
const ProfileSettingsModule = (): ReactElement => {
  /** Хук для работы с профилем пользователя */
  const { profile } = useProfile();

  /** Конфигурация приложения, содержащая настройки OIDC */
  const config = useInjection<Configuration>('Configuration');

  if (!profile)
    return (
      <Paper>
        <Skeleton variant="rounded" width="100%" height={48} sx={{ borderRadius: 1 }} />
      </Paper>
    );

  // Возвращает JSX элемент с кнопкой для перехода к настройкам аккаунта
  return (
    <Paper>
      {/** Кнопка для перехода к настройкам аккаунта */}
      <Button
        href={`${config.oidc.authority}/settings`}
        target="_blank"
        variant="contained"
        color="primary"
        fullWidth
        sx={{
          py: 1.5,
          fontWeight: 'bold',
          textTransform: 'none',
          fontSize: '1rem',
        }}
      >
        Настройки аккаунта
      </Button>
    </Paper>
  );
};

export default ProfileSettingsModule;
