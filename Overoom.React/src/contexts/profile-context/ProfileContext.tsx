import { createContext } from 'react';

import { ProfileResponse } from '../../services/profile/responses/profile.response.ts';

/** Интерфейс контекста профиля пользователя */
export interface ProfileContextType {
  /** Данные профиля */
  profile: ProfileResponse | null;
  /** Изменение данных профиля */
  editProfile: (editFunc: (film: ProfileResponse | null) => ProfileResponse) => void;
}

/** Контекст профиля с undefined в качестве значения по умолчанию */
export const ProfileContext = createContext<ProfileContextType | undefined>(undefined);
