import { Container, Box, Typography, Link } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Компонент футера сайта.
 * Отображает копирайт и ссылку для правообладателей.
 * @returns {ReactElement} JSX-элемент футера
 */
const FooterModule = (): ReactElement => {
  return (
    <Box
      component="footer"
      sx={(theme) => ({
        backgroundColor: theme.palette.background.default,
        padding: theme.spacing(2, 0),

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
          <Typography variant="body2" color="text.secondary">
            &copy; {new Date().getFullYear()} - Overoom
          </Typography>

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
