import Avatar from '@mui/material/Avatar';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import React, { ReactElement, useState } from 'react';

/**
 * Интерфейс пропсов компонента UserInfo
 */
interface UserInfoProps {
  /** URL аватара пользователя */
  photoUrl?: string | null;
  /** Коллбэк для выхода из приложения */
  onExit: () => void;
  /** Коллбэк для перехода на страницу профиля */
  onProfile: () => void;
}

/**
 * Компонент для отображения информации о пользователе в навбаре.
 * Показывает аватар и меню с действиями "Профиль" и "Выход".
 * @param props - Объект с данными пользователя и коллбэками действий
 * @returns {ReactElement} JSX элемент блока информации о пользователе
 */
const UserInfo = (props: UserInfoProps): ReactElement => {
  /** Хук для хранения состояния привязки меню к элементу DOM */
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  /**
   * Обработчик клика по аватару пользователя для открытия меню
   * @param event - Объект события мыши
   */
  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  /** Обработчик закрытия меню */
  const handleClose = () => {
    setAnchorEl(null);
  };

  // Рендерим JSX с аватаром и контекстным меню
  return (
    <>
      <IconButton onClick={handleClick}>
        <Avatar sx={{ width: 40, height: 40 }} src={props.photoUrl ?? undefined} />
      </IconButton>

      {/* Контекстное меню */}
      <Menu
        anchorEl={anchorEl}
        keepMounted
        open={Boolean(anchorEl)}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
      >
        {/* Элемент меню: переход в профиль */}
        <MenuItem
          onClick={() => {
            handleClose();
            props.onProfile();
          }}
        >
          Профиль
        </MenuItem>

        {/* Элемент меню: выход из приложения */}
        <MenuItem
          onClick={() => {
            handleClose();
            props.onExit();
          }}
        >
          Выйти
        </MenuItem>
      </Menu>
    </>
  );
};

export default UserInfo;
