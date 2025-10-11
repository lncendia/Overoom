import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useMemo } from 'react';

import { ViewerTagDto } from '../../../components/room/viewer/viewer-tag.dto.ts';
import { ViewerDto as ComponentViewerDto } from '../../../components/room/viewer/viewer.dto.ts';
import ViewerSkeleton from '../../../components/room/viewer/Viewer.skeleton.tsx';
import ViewersList from '../../../components/room/viewers-list/ViewersList.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { ViewerDto, ViewerStateDto } from '../../../hooks/room-hook/room.dto.ts';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';

/**
 * Компонент списка зрителей комнаты.
 * Разделяет вычисление базовой информации о зрителях и наложение динамических данных плеера.
 * @returns {ReactElement} JSX элемент модуля списка зрителей.
 */
const RoomViewersModule = (): ReactElement => {
  /** Хук для получения данных комнаты и хаба */
  const { room, currentViewerId, hub } = useRoom();

  /** Сервис для работы с API комнат */
  const roomsApi = useInjection<RoomsApi>('RoomsApi');

  /**
   * Обработчик отправки сигнала "бип" зрителю
   * @param id - ID зрителя, которому отправляется сигнал
   * @returns {Promise<void>} Promise, разрешающийся после отправки сигнала
   */
  const handleBeep = useCallback(
    async (id: string) => {
      await hub?.beep(id);
    },
    [hub]
  );

  /**
   * Обработчик отправки сигнала "крик" зрителю
   * @param id - ID зрителя, которому отправляется сигнал
   * @returns {Promise<void>} Promise, разрешающийся после отправки сигнала
   */
  const handleScream = useCallback(
    async (id: string) => {
      await hub?.scream(id);
    },
    [hub]
  );

  /**
   * Обработчик синхронизации состояния медиа-плеера с текущим состоянием комнаты
   * @returns {Promise<void>} Promise, разрешающийся после завершения синхронизации
   */
  const handleSync = useCallback(async () => {
    await hub?.sync();
  }, [hub]);

  /**
   * Обработчик исключения зрителя из комнаты
   * @param id - ID зрителя, которого необходимо исключить из комнаты
   * @returns {Promise<void>} Promise, разрешающийся после завершения операции исключения
   */
  const handleKick = useSafeCallback(
    async (id: string) => {
      if (!room?.id) return;
      await roomsApi.kick(room.id, id);
    },
    [room?.id, roomsApi]
  );

  // 1. Мемоизированная базовая информация о зрителях (стабильные поля + теги)
  const baseViewers = useMappedViewers(currentViewerId, room?.viewers, room?.ownerId);

  // 2. Мемоизированное наложение динамических данных плеера
  const enrichedViewers = useEnrichViewersWithStateInfo(baseViewers, room?.viewerStates);

  if (enrichedViewers.length === 0) return <ViewerSkeleton />;

  return (
    <ViewersList
      viewers={enrichedViewers}
      onKick={handleKick}
      onBeep={handleBeep}
      onScream={handleScream}
      onSync={handleSync}
    />
  );
};

/**
 * Хук для вычисления базовой информации о зрителях (статические поля, теги, права)
 * @param currentViewerId - ID текущего пользователя
 * @param viewers - Map зрителей комнаты
 * @param ownerId - ID владельца комнаты
 * @returns {ComponentViewerDto[]} Массив зрителей с базовой информацией
 */
function useMappedViewers(
  currentViewerId: string,
  viewers?: Map<string, ViewerDto>,
  ownerId?: string
): ComponentViewerDto[] {
  return useMemo(() => {
    if (!viewers) return [];
    const currentViewer = viewers.get(currentViewerId);
    const isCurrentOwner = currentViewerId === ownerId;

    return Array.from(viewers.entries()).map<ComponentViewerDto>(([id, viewer]) => {
      const isCurrent = id === currentViewerId;

      // Формируем теги зрителя
      const tags: ViewerTagDto[] = viewer.tags.map((t) => {
        return {
          name: t.name,
          description: t.description,
        };
      });

      const canBeep =
        viewer.online &&
        viewer.settings.beep &&
        (currentViewer?.settings.beep ?? false) &&
        !isCurrent;

      const canScream =
        viewer.online &&
        viewer.settings.screamer &&
        (currentViewer?.settings.screamer ?? false) &&
        !isCurrent;

      const canKick = isCurrentOwner && !isCurrent;

      const canSync = viewer.online && !isCurrentOwner && id === ownerId && !isCurrent;

      return {
        id,
        userName: viewer.userName,
        photoUrl: viewer.photoUrl,
        tags,
        online: viewer.online,

        // Статусные флаги
        isOwner: id === ownerId,
        isCurrent,

        // Разрешения / действия
        canBeep: canBeep,
        canScream: canScream,
        canKick: canKick,
        canSync: canSync,

        // Дефолтные поля плеера (будут наложены вторым этапом)
        typing: false,
        fullScreen: false,
        onPause: false,
        timeLine: 0,
        season: null,
        episode: null,
      };
    });
  }, [viewers, currentViewerId, ownerId]);
}

/**
 * Хук для наложения динамических данных плеера на базовую информацию о зрителях
 * @param baseViewers - Массив зрителей с базовой информацией
 * @param viewerStates - Map состояний зрителей (динамические данные плеера)
 * @returns {ComponentViewerDto[]} Массив зрителей с полной информацией
 */
function useEnrichViewersWithStateInfo(
  baseViewers: ComponentViewerDto[],
  viewerStates?: Map<string, ViewerStateDto>
): ComponentViewerDto[] {
  return useMemo(() => {
    if (!viewerStates) return baseViewers;
    return baseViewers.map((v) => {
      const state = viewerStates.get(v.id);

      // Если нет информации о плеере — возвращаем как есть
      if (!state) return v;

      return {
        ...v,
        fullScreen: state.fullScreen,
        onPause: state.onPause,
        timeLine: state.timeLine,
        season: state.season,
        episode: state.episode,
        typing: state.typing,
      };
    });
  }, [baseViewers, viewerStates]);
}

export default RoomViewersModule;
