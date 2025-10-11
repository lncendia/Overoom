import { Container, Grid, Paper, Typography } from '@mui/material';
import { styled } from '@mui/system';
import { ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';

import CatalogExample from '../components/home/examples/CatalogExample.tsx';
import ChatExample from '../components/home/examples/ChatExample.tsx';
import FilmInfoExample from '../components/home/examples/FilmInfoExample.tsx';
import FilmsListExample from '../components/home/examples/FilmsListExample.tsx';
import ViewersListExample from '../components/home/examples/ViewersListExample.tsx';
import Logo from '../components/home/logo/Logo.tsx';
import Wishes from '../components/home/wishes/Wishes.tsx';
import FooterModule from '../modules/home/footer-module/FooterModule.tsx';
import NavbarModule from '../modules/home/navbar-module/NavbarModule.tsx';
import RandomVideoModule from '../modules/home/random-video-module/RandomVideoModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/** Текст информации о каталоге */
const catalogInfo =
  'На главной странице сайта вы можете найти список фильмов, которые доступны для просмотра. Выберите фильм, нажав на его обложку, и начните просмотр.';

/** Текст информации о комнатах */
const roomsInfo =
  'Если вы хотите создать комнату для совместного просмотра фильма, нажмите кнопку "Создать комнату". Вы можете пригласить друзей, отправив им ссылку на комнату или сделать её открытой, дав возможность другим пользователям сайта подключится к вам.';

/** Текст информации о чате */
const chatInfo =
  'Во время просмотра фильма или видео в комнате вы можете обмениваться сообщениями с другими участниками. Для этого введите текст в поле ввода и нажмите "Отправить".';

/** Текст информации о профиле */
const profileInfo =
  'В своем профиле вы можете посмотреть историю просмотров и оценок, а также список фильмов, которые вы добавили в "Смотреть позже".';

/** Текст информации о действиях */
const actionsInfo =
  'Во время просмотра фильма или видео в комнате вы можете издать звуковой сигнал или активировать скример, нажав соответствующие кнопки. Эти функции можно отключить в настройках.';

/** Стилизованный компонент текста */
const StyledTypography = styled(Typography)(({ theme }) => ({
  margin: 'auto',
  lineHeight: 1.6,
  fontSize: '1rem',
  color: theme.palette.text.primary,
  [theme.breakpoints.up('md')]: {
    padding: theme.spacing(2),
    fontSize: '1.3rem',
  },
}));

/**
 * Главная страница приложения
 * @returns {ReactElement} JSX элемент главной страницы
 */
const HomePage = (): ReactElement => {
  /** Используем хук useNavigate для навигации между страницами */
  const navigate = useNavigate();

  return (
    <>
      {/* Модуль случайного видео с навигацией и логотипом */}
      <RandomVideoModule>
        <NavbarModule />
        <Logo />
      </RandomVideoModule>

      {/* Основной контейнер контента */}
      <Container maxWidth={false} className="background-container">
        <Container maxWidth="xl">
          {/* Секция каталога */}
          <BlockTitle sx={{ mt: 5 }} title="Каталог" />
          <Paper>
            <Grid container spacing={2}>
              <Grid size={{ md: 5, lg: 6 }}>
                <CatalogExample onSelect={(id) => navigate('/film', { state: { id: id } })} />
              </Grid>
              <Grid size={{ md: 7, lg: 6 }}>
                <StyledTypography>{catalogInfo}</StyledTypography>
              </Grid>
            </Grid>
          </Paper>

          {/* Секция комнат */}
          <BlockTitle sx={{ mt: 5 }} title="Комнаты" />
          <Paper>
            <FilmInfoExample
              onCountrySelect={(value) => navigate('/search', { state: { country: value } })}
              onGenreSelect={(value) => navigate('/search', { state: { genre: value } })}
              onPersonSelect={(value) => navigate('/search', { state: { person: value } })}
              onYearSelect={(value) => navigate('/search', { state: { year: value } })}
              onTypeSelect={(value) =>
                navigate('/search', { state: { serial: value === 'Сериал' } })
              }
            />
            <StyledTypography>{roomsInfo}</StyledTypography>
          </Paper>

          {/* Секция чата */}
          <BlockTitle sx={{ mt: 5 }} title="Чат" />
          <Paper>
            <Grid container spacing={2}>
              <Grid size={{ md: 5, lg: 6 }}>
                <StyledTypography>{chatInfo}</StyledTypography>
              </Grid>
              <Grid size={{ md: 7, lg: 6 }} sx={{ width: '100%' }}>
                <ChatExample />
              </Grid>
            </Grid>
          </Paper>

          {/* Секция действий */}
          <BlockTitle sx={{ mt: 5 }} title="Действия" />
          <Paper>
            <Grid container spacing={2}>
              <Grid size={{ md: 7, lg: 6 }} sx={{ width: '100%' }}>
                <ViewersListExample />
              </Grid>
              <Grid size={{ md: 5, lg: 6 }}>
                <StyledTypography>{actionsInfo}</StyledTypography>
              </Grid>
            </Grid>
          </Paper>

          {/* Секция профиля */}
          <BlockTitle sx={{ mt: 5 }} title="Профиль" />
          <Paper>
            <Grid container spacing={2}>
              <Grid size={{ md: 5, lg: 3 }}>
                <StyledTypography>{profileInfo}</StyledTypography>
              </Grid>
              <Grid size={{ md: 7, lg: 8 }} sx={{ width: '100%' }}>
                <FilmsListExample onSelect={(id) => navigate('/film', { state: { id: id } })} />
              </Grid>
            </Grid>
          </Paper>

          {/* Пожелание */}
          <Wishes sx={{ mt: 5 }} text="Приятного просмотра" />
        </Container>
      </Container>

      {/* Подвал страницы */}
      <FooterModule />
    </>
  );
};

export default HomePage;
