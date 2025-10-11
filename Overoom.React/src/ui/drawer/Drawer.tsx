import { Close } from '@mui/icons-material';
import {
  Drawer as MuiDrawer,
  IconButton,
  Typography,
  Stack,
  Box,
  Divider,
  SxProps,
  alpha,
} from '@mui/material';
import { ReactElement, ReactNode } from 'react';

/** Свойства для компонента выдвижной панели Drawer */
interface DrawerProps {
  /** Заголовок боковой панели */
  title: string;
  /** Флаг видимости панели */
  show: boolean;
  /** Функция-обработчик закрытия панели */
  onClose: () => void;
  /** Дочерние элементы панели */
  children: ReactNode;
  /** Позиция панели: слева, справа, сверху или снизу (по умолчанию 'left') */
  anchor?: 'left' | 'right' | 'top' | 'bottom';
  /** Ширина панели для левого/правого расположения (по умолчанию 400) */
  width?: number;
  /** Дополнительные стили панели */
  sx?: SxProps;
}

/**
 * Компонент выдвижной панели с заголовком, телом и кнопкой закрытия
 * @param props - Свойства компонента
 * @param props.title - Заголовок панели
 * @param props.show - Флаг видимости панели
 * @param props.onClose - Обработчик закрытия панели
 * @param props.children - Дочерние элементы панели
 * @param props.anchor - Позиция панели (left, right, top, bottom)
 * @param props.width - Ширина панели
 * @param props.sx - Дополнительные стили
 * @returns {ReactElement} JSX элемент выдвижной панели
 */
const Drawer = ({
  title,
  show,
  onClose,
  children,
  anchor = 'left',
  width = 400,
  sx,
}: DrawerProps): ReactElement => {
  return (
    <MuiDrawer
      anchor={anchor}
      open={show}
      onClose={onClose}
      sx={{
        '& .MuiDrawer-paper': {
          backgroundColor: (theme) => alpha(theme.palette.background.paper, 1),
          width: ['left', 'right'].includes(anchor) ? width : 'auto',
        },
        ...sx,
      }}
    >
      {/* Шапка панели с заголовком и кнопкой закрытия */}
      <Box sx={{ p: 2 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h6" component="h2">
            {title}
          </Typography>
          <IconButton onClick={onClose} sx={{ color: 'inherit' }}>
            <Close />
          </IconButton>
        </Stack>
      </Box>

      <Divider />

      {/* Тело панели с содержимым */}
      <Box sx={{ p: 2, flex: 1, overflow: 'auto' }}>{children}</Box>
    </MuiDrawer>
  );
};

export default Drawer;
