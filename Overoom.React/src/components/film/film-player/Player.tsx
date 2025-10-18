/* eslint-disable @typescript-eslint/no-explicit-any */
import { styled } from '@mui/material';
import { ReactElement, useEffect } from 'react';

/**
 * Конфигурация плеера
 */
export interface PlayerConfig {
  /** Идентификатор плеера */
  id: string;
  /** Файлы для воспроизведения: может быть один файл или несколько сезонов */
  file: RawVersion[] | RawSeason[];
}

/**
 * Сырая версия медиа
 */
export interface RawVersion {
  /** Название версии */
  title: string;
  /** Путь к файлу медиа */
  file: string;
  /** Идентификатор версии */
  id: string;
}

/**
 * Сырой эпизод
 */
export interface RawEpisode {
  /** Название эпизода */
  title: string;
  /** Папка с версиями эпизода */
  folder: RawVersion[];
}

/**
 * Сырой сезон
 */
export interface RawSeason {
  /** Название сезона */
  title: string;
  /** Папка с эпизодами */
  folder: RawEpisode[];
}

/**
 * Компонент видео плеера
 * @param props - Пропсы компонента
 * @param props.id - идентификатор контейнера плеера
 * @param props.file - файлы для воспроизведения
 * @returns {ReactElement} JSX элемент контейнера плеера
 */
export default function Player({ id, file }: PlayerConfig): ReactElement {
  /** Хук useEffect для инициализации плеера при изменении файлов или id */
  useEffect(() => {
    CreatePlayer({ id, file });
    // Очистка при размонтировании
    return () => {
      if (window.pljssglobal && window.pljssglobal.length > 0) {
        // Удаление плеера из глобального массива
        window.pljssglobal = window.pljssglobal.filter((player: any) => player.api('id') !== id);
      }
    };
  }, [file, id]);

  return <PlayerDiv id={id} />;
}

/**
 * Стилизованный контейнер для плеера
 */
const PlayerDiv = styled('div')(({ theme }) => ({
  width: '100%',
  [theme.breakpoints.up('xs')]: {
    height: '40vh',
  },
  [theme.breakpoints.up('md')]: {
    height: '50vh',
  },
  [theme.breakpoints.up('xl')]: {
    height: '75vh',
  },
  borderRadius: theme.shape.borderRadius,
  boxShadow: theme.shadows[1],
  border: 'none',
  transition: 'box-shadow 0.3s ease',
}));

/**
 * Глобальные объявления для работы с Playerjs
 */
declare global {
  interface Window {
    Playerjs: any;
    pjscnfgs: any;
    PlayerjsAsync: any;
    pljssglobal: any;
    PlayerjsEvents: ((event: string, id: string, info: any) => void) | undefined;
  }
}

/**
 * Функция создания плеера
 * @param config - объект конфигурации плеера
 * @returns {void}
 */
function CreatePlayer(config: any): void {
  if (window.Playerjs) {
    new window.Playerjs(config);
  } else {
    if (!window.pjscnfgs) {
      window.pjscnfgs = {};
    }
    window.pjscnfgs[config.id] = config;
  }
}

/**
 * Асинхронная функция инициализации всех плееров
 * @returns {void}
 */
window.PlayerjsAsync = function (): void {
  if (window.pjscnfgs) {
    Object.entries(window.pjscnfgs).map(([, value]) => {
      return new window.Playerjs(value);
    });
  }
  window.pjscnfgs = {};
};
