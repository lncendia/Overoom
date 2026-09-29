import { keyframes } from '@emotion/react';
import { Box, Typography, styled } from '@mui/material';
import { ReactElement } from 'react';

/** Анимация появления логотипа */
const fadeIn = keyframes`
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
`;

/** Стилизованный контейнер логотипа */
const LogoContainer = styled(Box)(({ theme }) => ({
  textAlign: 'center',
  animation: `${fadeIn} 0.8s ease-out`,
  width: '70%',
  margin: '30px auto 0',

  [theme.breakpoints.up('lg')]: {
    width: '35%',
    marginLeft: '15%',
    marginTop: '50px',
  },

  [theme.breakpoints.up('xl')]: {
    width: '35%',
    marginLeft: '10%',
  },
}));

/** Стилизованная разделительная линия */
const LogoLine = styled(Box)(({ theme }) => ({
  width: 250,
  margin: '0 auto 10px',
  borderTop: `4px solid ${theme.palette.text.primary}`,

  [theme.breakpoints.up('lg')]: {
    width: 300,
  },

  [theme.breakpoints.up('xl')]: {
    width: 400,
  },
}));

/** Стилизованная иконка очков */
const GlassesIcon = styled('svg')(({ theme }) => ({
  width: 50,
  height: 50,
  fill: 'currentColor',
  color: theme.palette.text.primary,

  [theme.breakpoints.up('lg')]: {
    width: 70,
    height: 70,
  },

  [theme.breakpoints.up('xl')]: {
    width: 70,
    height: 70,
  },
}));

/** Стилизованный основной текст логотипа */
const MainText = styled(Typography)(({ theme }) => ({
  fontSize: '60px',
  fontWeight: 600,
  display: 'inline-block',
  color: theme.palette.text.primary,

  [theme.breakpoints.up('lg')]: {
    fontSize: '70px',
    fontWeight: 700,
  },

  [theme.breakpoints.up('xl')]: {
    fontSize: '80px',
  },
}));

/** Стилизованный дополнительный текст логотипа */
const SideText = styled(Typography)(({ theme }) => ({
  fontSize: '25px',
  fontWeight: 400,
  color: theme.palette.text.primary,

  [theme.breakpoints.up('lg')]: {
    fontSize: '30px',
    fontWeight: 500,
  },

  [theme.breakpoints.up('xl')]: {
    fontSize: '40px',
  },
}));

/**
 * Компонент логотипа приложения
 * @returns {ReactElement} JSX элемент логотипа приложения
 */
const Logo = (): ReactElement => {
  return (
    <LogoContainer>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <MainText>Over</MainText>
        <GlassesIcon viewBox="0 0 500 400">
          <path d="M160,0c35.622-.7,99.805,39.235,72,73-14.661,17.8-42.593-7.6-64-1-23.526,7.255-53.218,65.441-59,92H392v-1c-13.412-30.072-27.337-79.882-59-91-21.586-7.58-42.917,17.092-61,4-15.05-10.9-6.777-34.677,2-46,16.088-20.756,58.284-41.262,90-22,38.939,23.649,125.567,138.851,136,190V346c-2.4,10.114-9.574,20.662-17,26-15.05,10.818-45.206,8-71,8H332c-11.974-2.9-71.042-48.193-80-58l-81,57H32c-9.552-2.692-19.766-9.791-25-17-13.427-18.494-7-88.009-7-120V198C11.164,146.59,94,37.9,132,11,140.247,5.161,149.684,4.521,160,0ZM49,212V331H157c21.428-8.719,73.666-68.53,98-64,28.706,5.344,63.036,55.29,89,64H451V212H49Zm12,20c23.283-.342,132.629-4.085,143,3,7.889,5.389,11.813,18.953,12,32l-70,50H102l-41-1V232Zm378,0q-0.5,42.5-1,85H353l-69-51c0.39-13.258,5.161-27.208,14-32C311.17,226.86,414.9,231.663,439,232Z" />
        </GlassesIcon>
        <MainText>m</MainText>
      </Box>
      <LogoLine />
      <SideText>Место для новой истории</SideText>
    </LogoContainer>
  );
};

export default Logo;
