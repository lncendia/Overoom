import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import UserInfoSkeleton from '../../../components/profile/user-info/UserInfo.skeleton.tsx';
import UserInfo from '../../../components/profile/user-info/UserInfo.tsx';
import { useProfile } from '../../../contexts/profile-context/useProfile.tsx';

/**
 * Модуль отображения информации о пользователе.
 * Включает основные данные пользователя и его предпочтения по жанрам.
 * @returns {ReactElement} JSX элемент модуля информации о пользователе
 */
const UserInfoModule = (): ReactElement => {
  /** Хук для получения профиля пользователя */
  const { profile } = useProfile();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Обработчик выбора жанра
   * @param genre - выбранный жанр
   */
  const onGenreSelect = useCallback(
    (genre: string) => {
      navigate('/search', { state: { genre: genre } });
    },
    [navigate]
  );

  if (!profile) return <UserInfoSkeleton />;

  return <UserInfo onGenreSelect={onGenreSelect} {...profile} />;
};

export default UserInfoModule;
