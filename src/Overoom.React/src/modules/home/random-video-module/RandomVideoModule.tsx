import { ReactElement, ReactNode, useState } from 'react';

import VideoWrapper from '../../../ui/video-wrapper/VideoWrapper.tsx';

/** Список доступных фоновых видео */
const videoList = [
  '/video/trailer1.mp4',
  '/video/trailer2.mp4',
  '/video/trailer3.mp4',
  '/video/trailer4.mp4',
  '/video/trailer5.mp4',
];

/**
 * Модуль для воспроизведения случайного видео из списка.
 * @param props - Пропсы компонента
 * @param props.children - дочерние элементы, которые отображаются поверх видео
 * @returns {ReactElement} JSX-элемент модуля со случайным видео
 */
const RandomVideoModule = ({ children }: { children: ReactNode }): ReactElement => {
  /** Случайное видео, выбирается один раз при первом рендере */
  const [randomVideo] = useState<string>(
    () => videoList[Math.floor(Math.random() * videoList.length)]
  );

  return <VideoWrapper src={randomVideo}>{children}</VideoWrapper>;
};

export default RandomVideoModule;
