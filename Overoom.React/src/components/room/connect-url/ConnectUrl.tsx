import CheckIcon from '@mui/icons-material/Check';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { Box } from '@mui/material';
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
 * Стилизованный контейнер для отображения ссылки подключения
 */
const StyledContainer = styled(Box, {
  shouldForwardProp: (prop) => prop !== 'isClicked',
})<{ isClicked?: boolean }>(({ theme, isClicked }) => ({
  marginTop: '5px',
  marginLeft: '5px',
  display: 'flex',
  alignItems: 'center',
  cursor: isClicked ? 'default' : 'pointer',
  transition: 'all 0.2s ease-in-out',
  color: isClicked ? theme.palette.success.main : theme.palette.text.primary,
  ...theme.typography.body1,
}));

/**
 * Компонент отображения ссылки для подключения с индикацией копирования
 * @param props - Пропсы компонента
 * @param props.onClick - Функция, вызываемая при клике на ссылку
 * @param props.isClicked - Флаг, указывающий, была ли ссылка скопирована
 * @returns {ReactElement} JSX элемент с иконкой и текстом
 */
const ConnectUrl = ({ onClick, isClicked }: ConnectUrlProps): ReactElement => {
  // Текст, отображаемый в зависимости от состояния
  const title = isClicked ? 'Ссылка скопирована' : 'Ссылка для подключения';

  /** Обработчик клика по элементу */
  const handleClick = () => {
    if (isClicked) return;
    onClick();
  };

  return (
    <StyledContainer isClicked={isClicked} onClick={handleClick}>
      {/* Иконка состояния: скопировано / копировать */}
      {isClicked ? (
        <CheckIcon sx={{ width: 16, height: 16, mr: 1 }} />
      ) : (
        <ContentCopyIcon sx={{ width: 16, height: 16, mr: 1 }} />
      )}
      {title}
    </StyledContainer>
  );
};

export default ConnectUrl;
