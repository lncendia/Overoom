import { Paper } from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement } from 'react';

/** Свойства для компонента NoData */
export interface NoDataProps {
  /** Текст сообщения, отображаемого при отсутствии данных */
  text: string;
}

/**
 * Компонент для отображения сообщения о пустом состоянии
 * @param props - Свойства компонента
 * @param props.text - Текст сообщения
 * @returns {ReactElement} JSX элемент с сообщением о пустом состоянии
 */
const NoData = ({ text }: NoDataProps): ReactElement => {
  return (
    <Paper>
      {/* Отображение текста сообщения */}
      <Typography>{text}</Typography>
    </Paper>
  );
};

export default NoData;
