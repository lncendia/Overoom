import CheckIcon from '@mui/icons-material/Check';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { ButtonBase } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/** Пропсы компонента ConnectUrl */
interface ConnectUrlProps {
  /** Коллбэк при клике на ссылку */
  onClick: () => void;
  /** Флаг, была ли ссылка уже скопирована */
  isClicked: boolean;
}

/**
 * Стилизованная кнопка для копирования ссылки подключения
 */
const StyledButton = styled(ButtonBase, {
  shouldForwardProp: (prop) => prop !== 'isClicked',
})<{ isClicked?: boolean }>(({ theme, isClicked }) => ({
  marginTop: '5px',
  marginLeft: '5px',
  padding: theme.spacing(0.25, 0.5),
  display: 'flex',
  alignItems: 'center',
  borderRadius: theme.shape.borderRadius,
  cursor: isClicked ? 'default' : 'pointer',
  transition: 'all 0.2s ease-in-out',
  color: isClicked ? theme.palette.success.main : theme.palette.text.primary,
  ...theme.typography.body1,
  '&.Mui-focusVisible': {
    outline: `2px solid ${theme.palette.primary.main}`,
  },
}));

/**
 * Компонент отображения ссылки для подключения с индикацией копирования
 * @param props - Пропсы компонента
 * @param props.onClick - Функция, вызываемая при клике на ссылку
 * @param props.isClicked - Флаг, указывающий, была ли ссылка скопирована
 * @returns {ReactElement} JSX элемент с иконкой и текстом
 */
const ConnectUrl = ({ onClick, isClicked }: ConnectUrlProps): ReactElement => {
  const title = isClicked ? 'Ссылка скопирована' : 'Ссылка для подключения';

  /** Обработчик клика по элементу */
  const handleClick = () => {
    if (isClicked) return;
    onClick();
  };

  return (
    <StyledButton
      isClicked={isClicked}
      onClick={handleClick}
      aria-label={isClicked ? undefined : 'Скопировать ссылку для подключения'}
      disableRipple={isClicked}
    >
      {isClicked ? (
        <CheckIcon sx={{ width: 16, height: 16, mr: 1 }} />
      ) : (
        <ContentCopyIcon sx={{ width: 16, height: 16, mr: 1 }} />
      )}
      <span aria-live="polite">{title}</span>
    </StyledButton>
  );
};

export default ConnectUrl;
