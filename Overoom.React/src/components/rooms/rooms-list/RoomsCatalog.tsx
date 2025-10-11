import { Grid } from '@mui/material';
import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { RoomItemDto } from '../room-item/room-item.dto.ts';
import RoomItem from '../room-item/RoomItem.tsx';

/** Пропсы компонента RoomsCatalog */
export interface RoomsCatalogProps {
  /** Список комнат */
  rooms: RoomItemDto[];

  /** Обработчик выбора комнаты */
  onSelect: (id: string) => void;

  /** Флаг наличия дополнительных элементов для загрузки */
  hasMore: boolean;

  /** Функция загрузки следующих элементов */
  next: () => void;
}

/**
 * Компонент каталога комнат с бесконечной прокруткой
 * @param props - Пропсы компонента
 * @param props.rooms - Список комнат
 * @param props.onSelect - Обработчик выбора комнаты
 * @param props.hasMore - Флаг наличия дополнительных элементов
 * @param props.next - Функция загрузки следующих элементов
 * @returns {ReactElement} JSX элемент каталога комнат
 */
const RoomsCatalog = ({ rooms, onSelect, hasMore, next }: RoomsCatalogProps): ReactElement => {
  return (
    <InfiniteScroll
      dataLength={rooms.length}
      next={next}
      hasMore={hasMore}
      loader={<Spinner />}
      style={{ overflow: 'visible' }}
    >
      <Grid container spacing={5} sx={{ mt: 4 }}>
        {rooms.map((room) => (
          <Grid
            size={{ xs: 12, sm: 6, md: 4, lg: 6, xl: 4 }}
            key={room.id}
            sx={{ display: 'flex', justifyContent: 'center' }}
          >
            {/* Элемент комнаты */}
            <RoomItem room={room} onClick={() => onSelect(room.id)} />
          </Grid>
        ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default RoomsCatalog;
