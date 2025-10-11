import { IconButton, Menu, MenuItem } from '@mui/material';
import Avatar from '@mui/material/Avatar';
import React, { ReactElement, useState } from 'react';

/** Интерфейс пропсов компонента SignIn */
interface SignInProps {
  /** Коллбэк для перехода на страницу авторизации */
  onLogin: () => void;
}

/**
 * Компонент, отображаемый вместо UserInfo, если пользователь не авторизован.
 * Показывает аватар с кнопкой, открывающей меню с возможностью входа.
 * @param props - Объект с коллбэком для авторизации
 * @returns {ReactElement} JSX элемент блока авторизации
 */
const SignIn = (props: SignInProps): ReactElement => {
  /** Хук для хранения состояния якоря меню */
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  /**
   * Обработчик клика по аватару для открытия меню
   * @param event - Объект события мыши
   */
  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  /**
   * Обработчик закрытия меню и вызова функции входа
   */
  const handleClose = () => {
    setAnchorEl(null);
    props.onLogin();
  };

  // Рендерим JSX с аватаром и контекстным меню
  return (
    <>
      <IconButton onClick={handleMenu}>
        <Avatar sx={{ width: 40, height: 40 }} />
      </IconButton>

      <Menu
        id="menu-appbar"
        keepMounted
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
        open={Boolean(anchorEl)}
        onClose={handleClose}
      >
        {/* Элемент меню для входа */}
        <MenuItem onClick={handleClose}>Войти</MenuItem>
      </Menu>
    </>
  );
};

export default SignIn;
