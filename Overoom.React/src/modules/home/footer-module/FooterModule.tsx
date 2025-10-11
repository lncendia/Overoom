import { Container, Box, Typography, Link } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Компонент футера сайта.
 * Отображает копирайт и ссылку для правообладателей.
 * @returns {ReactElement} JSX-элемент футера
 */
const FooterModule = (): ReactElement => {
  // Основной контейнер футера
  return (
    <Box
      component="footer"
      sx={(theme) => ({
        backgroundColor: theme.palette.background.default,
        padding: theme.spacing(2, 0),

        // Для прижатия футера к низу страницы
        marginTop: 'auto',
      })}
    >
      <Container maxWidth="xl">
        <Box
          sx={{
            display: 'flex',
            flexDirection: { xs: 'column', sm: 'row' },
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 2,
          }}
        >
          {/* Копирайт с текущим годом */}
          <Typography variant="body2" color="text.secondary">
            &copy; {new Date().getFullYear()} - Overoom
          </Typography>

          {/* Ссылка для правообладателей */}
          <Link
            href="#"
            variant="body2"
            color="text.secondary"
            sx={{ textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
          >
            Правообладателям
          </Link>
        </Box>
      </Container>
    </Box>
  );
};

export default FooterModule;
