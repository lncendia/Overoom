import { Avatar, styled } from '@mui/material';
import { ReactElement } from 'react';

/** Свойства для компонента RoomAvatar */
export interface RoomAvatarProps {
  /** URL изображения аватара */
  src?: string;
  /** Флаг, указывающий является ли пользователь владельцем комнаты */
  owner: boolean;
}

/** Стилизованный аватар с учетом статуса владельца */
const StyledAvatar = styled(Avatar, {
  shouldForwardProp: (prop) => prop !== 'owner',
})<{ owner: boolean }>(({ theme, owner }) => ({
  width: 45,
  height: 45,
  ...(owner && {
    boxShadow: theme.shadows[4],
    border: `2px solid ${theme.palette.error.main}`,
  }),
}));

/**
 * Компонент аватара комнаты с индикатором владельца
 * @param props - Свойства компонента
 * @param props.src - URL изображения аватара
 * @param props.owner - Флаг владельца комнаты
 * @returns {ReactElement} JSX элемент аватара комнаты
 */
const RoomAvatar = ({ src, owner }: RoomAvatarProps): ReactElement => {
  return <StyledAvatar owner={owner} src={src} alt="Аватар" />;
};

export default RoomAvatar;
