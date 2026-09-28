import { ReactElement, ReactNode, useEffect, useState } from 'react';

import VideoWrapper from '../../../ui/video-wrapper/VideoWrapper.tsx';

/**
 * Модуль для воспроизведения случайного видео из списка.
 * @param props - Пропсы компонента
 * @param props.children - дочерние элементы, которые отображаются поверх видео
 * @returns {ReactElement | null} JSX-элемент модуля со случайным видео
 */
const RandomVideoModule = ({ children }: { children: ReactNode }): ReactElement | null => {
  /** Состояние для хранения URL случайного видео */
  const [randomVideo, setRandomVideo] = useState<string>('');

  /** Выбор случайного видео при монтировании компонента */
  useEffect(() => {
    const videoList = [
      'video/trailer1.mp4',
      'video/trailer2.mp4',
      'video/trailer3.mp4',
      'video/trailer4.mp4',
      'video/trailer5.mp4',
    ];

    const randomVideo = videoList[Math.floor(Math.random() * videoList.length)];
    setRandomVideo(randomVideo);
  }, []);

  if (!randomVideo) return null;

  return <VideoWrapper src={randomVideo}>{children}</VideoWrapper>;
};

export default RandomVideoModule;
