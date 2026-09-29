import MenuIcon from '@mui/icons-material/Menu';
import { Box, Button, CssBaseline, Slide, styled } from '@mui/material';
import AppBar from '@mui/material/AppBar';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import useScrollTrigger from '@mui/material/useScrollTrigger';
import React, { ReactElement } from 'react';
import { Link as RouterLink } from 'react-router-dom';

import NavLogo from './NavLogo.tsx';
import SignIn from './SignIn.tsx';
import UserInfo from './UserInfo.tsx';
import { routes } from '../../../routes.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import ThemeSwitch from '../../../ui/theme-switch/ThemeSwitch.tsx';
import FilmSearch from '../films-search/FilmSearch.tsx';

/** Стилизованный SVG компонент */
const StyledSvg = styled('svg')({
  position: 'relative',
  bottom: '2px',
  marginRight: '2px',
  marginLeft: '2px',
});

/** Интерфейс свойств компонента HorizontalNavbar */
interface NavbarProps {
  /** Список фильмов для поиска */
  films: FilmShortResponse[];

  /** Тип темы - темная или светлая */
  darkMode: boolean;

  /** Обработчик переключения темы */
  toggleDarkMode: (value: boolean) => void;

  /** URL аватара авторизованного пользователя */
  photoUrl?: string;

  /** Имя авторизованного пользователя */
  userName?: string;

  /** Функция колбэк для входа в приложение */
  onLogin: () => void;

  /** Функция колбэк для выхода из приложения */
  onExit: () => void;

  /** Функция колбэк для поиска фильмов */
  onFilmSearch: (value: string) => void;

  /** Авторизован ли пользователь */
  isUserAuthorized: boolean;

  /** Является ли пользователь администратором */
  isAdmin?: boolean;
}

/**
 * Интерфейс страницы навигации
 */
interface Page {
  /** Название страницы */
  name: string;

  /** Адрес страницы */
  to: string;

  /** Иконка страницы */
  icon: React.ReactNode;
}

/**
 * Компонент горизонтальной навигационной панели
 * @param props - Пропсы компонента
 * @returns {ReactElement} JSX элемент навигационной панели
 */
const Navbar = (props: NavbarProps): ReactElement => {
  /** Список страниц навигации */
  const pages: Page[] = [
    {
      name: 'Каталог',
      icon: (
        <StyledSvg width={16} height={16} fill="currentColor" viewBox="0 0 16 16">
          <path d="M6 3a3 3 0 1 1-6 0 3 3 0 0 1 6 0z" />
          <path d="M9 6a3 3 0 1 1 0-6 3 3 0 0 1 0 6z" />
          <path d="M9 6h.5a2 2 0 0 1 1.983 1.738l3.11-1.382A1 1 0 0 1 16 7.269v7.462a1 1 0 0 1-1.406.913l-3.111-1.382A2 2 0 0 1 9.5 16H2a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h7z" />
        </StyledSvg>
      ),
      to: routes.catalog,
    },
    {
      name: 'Подборки',
      icon: (
        <StyledSvg width={16} height={16} fill="currentColor" viewBox="0 0 16 16">
          <path d="M4.5 1A1.5 1.5 0 0 0 3 2.5V3h4v-.5A1.5 1.5 0 0 0 5.5 1h-1zM7 4v1h2V4h4v.882a.5.5 0 0 0 .276.447l.895.447A1.5 1.5 0 0 1 15 7.118V13H9v-1.5a.5.5 0 0 1 .146-.354l.854-.853V9.5a.5.5 0 0 0-.5-.5h-3a.5.5 0 0 0-.5.5v.793l.854.853A.5.5 0 0 1 7 11.5V13H1V7.118a1.5 1.5 0 0 1 .83-1.342l.894-.447A.5.5 0 0 0 3 4.882V4h4zM1 14v.5A1.5 1.5 0 0 0 2.5 16h3A1.5 1.5 0 0 0 7 14.5V14H1zm8 0v.5a1.5 1.5 0 0 0 1.5 1.5h3a1.5 1.5 0 0 0 1.5-1.5V14H9zm4-11H9v-.5A1.5 1.5 0 0 1 10.5 1h1A1.5 1.5 0 0 1 13 2.5V3z" />
        </StyledSvg>
      ),
      to: routes.playlists,
    },
    {
      name: 'Комнаты',
      icon: (
        <StyledSvg width={16} height={16} fill="currentColor" viewBox="0 0 16 16">
          <path d="M7 14s-1 0-1-1 1-4 5-4 5 3 5 4-1 1-1 1H7Zm4-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm-5.784 6A2.238 2.238 0 0 1 5 13c0-1.355.68-2.75 1.936-3.72A6.325 6.325 0 0 0 5 9c-4 0-5 3-5 4s1 1 1 1h4.216ZM4.5 8a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Z" />
        </StyledSvg>
      ),
      to: routes.rooms,
    },
    ...(props.isAdmin
      ? [
          {
            name: 'Задачи',
            icon: (
              <StyledSvg width={16} height={16} fill="currentColor" viewBox="0 0 16 16">
                <path d="M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872l-.1-.34zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z" />
              </StyledSvg>
            ),
            to: routes.adminJobs,
          },
        ]
      : []),
  ];

  /** Хук состояния для элемента якоря меню навигации */
  const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null);

  /**
   * Обработчик открытия меню навигации
   * @param event - Объект события мыши
   */
  const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElNav(event.currentTarget);
  };

  /** Обработчик закрытия меню навигации */
  const handleCloseNavMenu = () => {
    setAnchorElNav(null);
  };

  /** Компонент информации о пользователе или кнопка входа */
  const userInfo = props.isUserAuthorized ? (
    <UserInfo
      photoUrl={props.photoUrl}
      userName={props.userName}
      onExit={props.onExit}
      profileHref={routes.profile}
    />
  ) : (
    <SignIn onLogin={props.onLogin} />
  );

  /** Хук для определения скролла страницы */
  const trigger = useScrollTrigger();

  return (
    <>
      <CssBaseline />
      <Slide appear={false} direction="down" in={!trigger}>
        <AppBar>
          <Toolbar>
            <NavLogo to={routes.home} sx={{ display: { xs: 'none', md: 'flex' }, mr: 2 }} />

            <Box sx={{ display: { xs: 'block', md: 'none' } }}>
              <IconButton
                size="large"
                aria-label="Открыть меню"
                aria-controls={anchorElNav ? 'menu-navigation' : undefined}
                aria-haspopup="true"
                aria-expanded={anchorElNav ? 'true' : undefined}
                onClick={handleOpenNavMenu}
                color="inherit"
              >
                <MenuIcon />
              </IconButton>
              <Menu
                id="menu-navigation"
                anchorEl={anchorElNav}
                keepMounted
                anchorOrigin={{
                  vertical: 'bottom',
                  horizontal: 'right',
                }}
                transformOrigin={{
                  vertical: 'top',
                  horizontal: 'right',
                }}
                open={Boolean(anchorElNav)}
                onClose={handleCloseNavMenu}
                sx={{ display: { xs: 'block', md: 'none' } }}
              >
                <MenuItem component={RouterLink} to={routes.home} onClick={handleCloseNavMenu}>
                  <Typography>Главная</Typography>
                </MenuItem>
                {pages.map((page) => (
                  <MenuItem
                    key={page.name}
                    component={RouterLink}
                    to={page.to}
                    onClick={handleCloseNavMenu}
                  >
                    <Typography>{page.name}</Typography>
                  </MenuItem>
                ))}
                <MenuItem onClick={() => props.toggleDarkMode(!props.darkMode)}>
                  Тёмный режим
                  <ThemeSwitch sx={{ ml: 1 }} checked={props.darkMode} />
                </MenuItem>
              </Menu>
            </Box>

            <Box sx={{ flexGrow: 1, gap: 2, display: { xs: 'none', md: 'block' } }}>
              {pages.map((page) => (
                <Button
                  component={RouterLink}
                  to={page.to}
                  key={page.name}
                  size="small"
                  color="inherit"
                >
                  {page.icon}
                  {page.name}
                </Button>
              ))}
            </Box>

            <FilmSearch
              films={props.films}
              onFilmSearch={props.onFilmSearch}
              getLink={routes.film}
            />

            <Box sx={{ display: { xs: 'none', md: 'block' } }}>
              <ThemeSwitch
                checked={props.darkMode}
                onChange={() => props.toggleDarkMode(!props.darkMode)}
              />
            </Box>

            {userInfo}
          </Toolbar>
        </AppBar>
      </Slide>
    </>
  );
};

export default Navbar;
