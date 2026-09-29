import { Paper } from '@mui/material';
import { useEffect, useMemo, ReactElement } from 'react';

import { SeasonDto, MediaDto } from './media.dto.ts';
import Player, { RawSeason, RawVersion } from './Player.tsx';

/** Пропсы компонента видеоплеера фильма или сериала */
interface FilmPlayerProps {
  /** Список сезонов сериала */
  seasons?: SeasonDto[];
  /** Объект медиаконтента (фильм или сериал) */
  content?: MediaDto;
}

/**
 * Компонент видеоплеера для фильма или сериала.
 * @param props - Свойства компонента
 * @param props.content - Объект медиаконтента (фильм или сериал)
 * @param props.seasons - Список сезонов сериала (если контент — сериал)
 * @returns {ReactElement | null} JSX элемент видеоплеера или null, если нет данных
 */
const FilmPlayer = ({ content, seasons }: FilmPlayerProps): ReactElement | null => {
  /** Загружаем внешний скрипт плеера */
  useScript('/playerjs.js');

  /** Мемоизированные сырые данные сезонов */
  const rawSeasons = useMemo(() => {
    if (!seasons) return undefined;
    return mapSeasonsToRaw(seasons);
  }, [seasons]);

  /** Мемоизированные сырые данные медиа */
  const rawMedia = useMemo(() => {
    if (!content) return undefined;
    return mapMediaToRaw(content);
  }, [content]);

  if (!rawMedia && !rawSeasons) return null;

  return (
    <Paper>
      <Player id="player" file={rawMedia ?? rawSeasons!} />
    </Paper>
  );
};

export default FilmPlayer;

/**
 * Преобразует сезоны сериала в сырой формат для плеера.
 * @param seasons - Список сезонов сериала
 * @returns {RawSeason[]} Сырые данные сезонов
 */
function mapSeasonsToRaw(seasons: SeasonDto[]): RawSeason[] {
  return seasons.map((season) => ({
    title: `Сезон ${season.number}`,
    folder: season.episodes.map((episode) => ({
      title: `Серия ${episode.number}`,
      folder: episode.versions.map((version) => ({
        title: `Серия ${episode.number} <span style='opacity:0.5'>${version.name}</span>`,
        file: version.src,
        id: `s${season.number}e${episode.number}_${version.name}`,
      })),
    })),
  }));
}

/**
 * Преобразует объект медиа в сырой формат для плеера.
 * @param media - Объект медиаконтента
 * @returns {RawVersion[]} Сырые данные медиафайлов
 */
function mapMediaToRaw(media: MediaDto): RawVersion[] {
  return media.versions.map((version) => ({
    title: version.name,
    file: version.src,
    id: version.name,
  }));
}

/**
 * Хук для однократной загрузки внешнего скрипта.
 * Скрипт добавляется в документ только один раз и не удаляется при размонтировании:
 * повторные монтирования переиспользуют уже загруженный скрипт (window.Playerjs).
 * @param url - Путь к JavaScript-файлу, который нужно загрузить
 * @returns {void}
 */
function useScript(url: string): void {
  useEffect(() => {
    if (window.Playerjs) return;
    if (document.querySelector(`script[data-src="${url}"]`)) return;

    const script = document.createElement('script');
    script.src = url;
    script.async = true;
    script.dataset.src = url;
    document.body.appendChild(script);
  }, [url]);
}
