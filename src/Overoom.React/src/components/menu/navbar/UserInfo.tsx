import Avatar from '@mui/material/Avatar';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import React, { ReactElement, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';

/**
 * Интерфейс пропсов компонента UserInfo
 */
interface UserInfoProps {
  /** URL аватара пользователя */
  photoUrl?: string | null;
  /** Имя пользователя */
  userName?: string;
  /** Коллбэк для выхода из приложения */
  onExit: () => void;
  /** Адрес страницы профиля */
  profileHref: string;
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

  return (
    <>
      <IconButton
        onClick={handleClick}
        aria-label="Меню пользователя"
        aria-controls={anchorEl ? 'menu-user' : undefined}
        aria-haspopup="true"
        aria-expanded={anchorEl ? 'true' : undefined}
      >
        <Avatar
          sx={{ width: 40, height: 40 }}
          src={props.photoUrl ?? undefined}
          alt={props.userName ? `Аватар: ${props.userName}` : 'Аватар пользователя'}
        />
      </IconButton>

      <Menu
        id="menu-user"
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
        <MenuItem component={RouterLink} to={props.profileHref} onClick={handleClose}>
          Профиль
        </MenuItem>

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
