import { useInjection } from 'inversify-react';
import React, { ReactElement, ReactNode, useEffect, useState } from 'react';

import { ProfileContext } from './ProfileContext.tsx';
import { useSafeCallback } from '../../hooks/safe-callback-hook/useSafeCallback.ts';
import { ProfileApi } from '../../services/profile/profile.api.ts';
import { ProfileResponse } from '../../services/profile/responses/profile.response.ts';

/** Пропсы для компонента ProfileContextProvider */
interface ProfileContextProviderProps {
  /** Дочерние элементы */
  children: ReactNode;
}

/**
 * Провайдер контекста профиля пользователя
 * @param props - Пропсы компонента
 * @param props.children - Дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера профиля
 */
export const ProfileContextProvider: React.FC<ProfileContextProviderProps> = ({
  children,
}): ReactElement => {
  /** Состояние профиля пользователя */
  const [profile, setProfile] = useState<ProfileResponse | null>(null);

  /** Получаем экземпляр ProfileApi из контейнера зависимостей */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Функция загрузки профиля пользователя */
  const loadProfile = useSafeCallback(async () => {
    const response = await profileApi.getProfile();
    setProfile(response);
  }, [profileApi]);

  /** Загружаем данные профиля при монтировании компонента */
  useEffect(() => {
    loadProfile().then();
    return () => {
      setProfile(null);
    };
  }, [loadProfile, profileApi]);

  // Возвращаем провайдер контекста с профилем и функцией редактирования
  return (
    <ProfileContext.Provider value={{ profile, editProfile: setProfile }}>
      {children}
    </ProfileContext.Provider>
  );
};
