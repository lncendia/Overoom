import { Grid } from '@mui/material';
import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { PlaylistItemDto } from '../playlist-item/playlist-item.dto.ts';
import PlaylistItem from '../playlist-item/PlaylistItem.tsx';

/** Пропсы компонента каталога плейлистов */
export interface PlaylistsCatalogProps {
  /** Список плейлистов для отображения */
  playlists: PlaylistItemDto[];
  /** Выбранный жанр (опционально) */
  genre?: string;
  /** Коллбэк для выбора плейлиста */
  onSelect: (id: string) => void;
  /** Флаг наличия дополнительных элементов */
  hasMore: boolean;
  /** Функция загрузки следующих элементов */
  next: () => void;
}

/**
 * Компонент каталога плейлистов с бесконечной прокруткой
 * @param props - Пропсы компонента
 * @param props.playlists - Список плейлистов
 * @param props.genre - Выбранный жанр для фильтрации
 * @param props.onSelect - Обработчик выбора плейлиста
 * @param props.hasMore - Флаг наличия дополнительных данных для загрузки
 * @param props.next - Функция загрузки следующей порции данных
 * @returns {ReactElement} JSX элемент каталога плейлистов
 */
const PlaylistsCatalog = ({
  playlists,
  genre,
  onSelect,
  hasMore,
  next,
}: PlaylistsCatalogProps): ReactElement => {
  return (
    <InfiniteScroll
      dataLength={playlists.length}
      next={next}
      hasMore={hasMore}
      loader={<Spinner />}
      style={{ overflow: 'visible' }}
    >
      <Grid container spacing={5} sx={{ mt: 4 }}>
        {playlists.map((playlist) => (
          <Grid
            size={{ xs: 12, sm: 6, md: 4, lg: 6, xl: 4 }}
            key={playlist.id}
            sx={{ display: 'flex', justifyContent: 'center' }}
          >
            {/* Элемент плейлиста */}
            <PlaylistItem
              selectedGenre={genre}
              playlist={playlist}
              onClick={() => onSelect(playlist.id)}
            />
          </Grid>
        ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default PlaylistsCatalog;
