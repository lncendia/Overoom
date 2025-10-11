import {
  Paper,
  FormControlLabel,
  Switch,
  FormGroup,
  Typography,
  Skeleton,
  Stack,
} from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement } from 'react';

import { useProfile } from '../../../contexts/profile-context/useProfile.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomSettingsResponse } from '../../../services/common/room-settings.response.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';

/**
 * Модуль настроек уведомлений пользователя.
 * Позволяет управлять различными типами уведомлений.
 * @returns {ReactElement} JSX элемент модуля настроек уведомлений пользователя.
 */
const UserNotificationSettingsModule = (): ReactElement => {
  /** Хук для работы с профилем пользователя */
  const { profile, editProfile } = useProfile();

  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /**
   * Обработчик переключения настроек уведомлений
   * @param setting - ключ настройки для изменения
   */
  const handleToggle = useSafeCallback(
    async (setting: keyof RoomSettingsResponse) => {
      const newSettings = {
        ...profile!.roomSettings,
        [setting]: !profile!.roomSettings[setting],
      };

      // Оптимистичное обновление
      editProfile((prev) => ({
        ...prev!,
        roomSettings: newSettings,
      }));

      // Отправка на сервер
      await profileApi.updateRoomSettings(newSettings);
    },
    [profile, editProfile, profileApi]
  );

  if (!profile)
    return (
      <Paper>
        <Stack spacing={2}>
          {/* Заголовок */}
          <Skeleton variant="text" width="50%" height={30} />

          {/* Переключатели */}
          <Skeleton variant="rounded" width="100%" height={28} />
          <Skeleton variant="rounded" width="100%" height={28} />
        </Stack>
      </Paper>
    );

  return (
    <Paper>
      <Typography variant="h6" gutterBottom>
        Настройки уведомлений
      </Typography>

      <FormGroup>
        <FormControlLabel
          control={
            <Switch
              checked={profile!.roomSettings.beep}
              onChange={() => handleToggle('beep')}
              name="beep"
            />
          }
          label="Звуковой сигнал"
        />

        <FormControlLabel
          control={
            <Switch
              checked={profile!.roomSettings.screamer}
              onChange={() => handleToggle('screamer')}
              name="scream"
            />
          }
          label="Скример"
        />
      </FormGroup>
    </Paper>
  );
};

export default UserNotificationSettingsModule;
