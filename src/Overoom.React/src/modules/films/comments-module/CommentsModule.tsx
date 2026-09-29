import { Box, Link } from '@mui/material';
import Typography from '@mui/material/Typography';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useMemo } from 'react';

import AddCommentFormSkeleton from '../../../components/comments/add-comment-form/AddCommentForm.skeleton.tsx';
import AddCommentForm from '../../../components/comments/add-comment-form/AddCommentForm.tsx';
import { CommentItemDto } from '../../../components/comments/comment-item/comment-item.dto.ts';
import CommentsListSkeleton from '../../../components/comments/comments-list/CommentsList.skeleton.tsx';
import CommentsList from '../../../components/comments/comments-list/CommentsList.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../../services/auth/auth.api.ts';
import { CommentsApi } from '../../../services/comments/comments.api.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';
import { sortReactions, toggleReaction } from '../../../ui/reactions/reactions.ts';

/** Комментарий в том виде, в котором он хранится в модуле (без привязки к текущему пользователю) */
type LoadedComment = Omit<CommentItemDto, 'isUserComment'> & {
  /** Идентификатор автора комментария */
  userId: string;
};

/**
 * Модуль для работы с комментариями к фильму.
 * Включает форму добавления комментариев и список существующих комментариев.
 * @returns {ReactElement} JSX-элемент модуля комментариев
 */
const CommentsModule = (): ReactElement => {
  /** Сервис для работы с API комментариев */
  const commentsApi = useInjection<CommentsApi>('CommentsApi');

  /** Сервис для работы с API аутентификации */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Получение данных текущего фильма из контекста */
  const { film } = useFilm();

  /** Получение данных текущего авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /** Хук для работы с уведомлениями */
  const { setNotification } = useNotify();

  /**
   * Загружает комментарии для текущего фильма
   * @param skip - количество пропускаемых комментариев
   * @param take - количество загружаемых комментариев
   * @returns объект с массивом комментариев и общим количеством
   */
  const fetch = useCallback(
    async (skip: number, take: number) => {
      if (!film?.id) return null;

      const response = await commentsApi.get(film.id, { skip, take });

      const mappedComments = response.list.map<LoadedComment>((item) => ({
        id: item.id,
        text: item.text,
        userId: item.userId,
        userName: item.userName,
        photoUrl: item.photoUrl,
        createdAt: new Date(item.createdAt),
        reactions: sortReactions(
          (item.reactions ?? []).map((r) => ({
            code: r.reaction,
            count: r.count,
            reacted: r.reacted,
          }))
        ),
      }));

      return {
        list: mappedComments,
        totalCount: response.totalCount,
      };
    },
    [commentsApi, film?.id]
  );

  /** Хук для порционной загрузки комментариев */
  const {
    items: loadedComments,
    isLoading,
    hasMore,
    fetchMore,
    removeWhere,
    add,
    update,
    error,
    retry,
  } = usePaginatedFetch<LoadedComment>(fetch, 20);

  /** Комментарии с признаком принадлежности текущему пользователю */
  const comments = useMemo(
    () =>
      loadedComments.map<CommentItemDto>((comment) => ({
        ...comment,
        isUserComment: !!authorizedUser && authorizedUser.id === comment.userId,
      })),
    [loadedComments, authorizedUser]
  );

  /**
   * Удаляет комментарий. При ошибке API комментарий возвращается в список
   * @param comment - удаляемый комментарий
   */
  const removeComment = useSafeCallback(
    async (comment: CommentItemDto) => {
      if (!film?.id) return;

      const original = loadedComments.find((c) => c.id === comment.id);
      removeWhere((c) => c.id === comment.id);

      try {
        await commentsApi.delete(film.id, comment.id);
      } catch (e) {
        if (original) add(original);
        throw e;
      }
    },
    [commentsApi, film?.id, loadedComments, removeWhere, add]
  );

  /** Обработчик входа в аккаунт */
  const signIn = useSafeCallback(() => authApi.signIn(), [authApi]);

  /**
   * Отображает предупреждение о необходимости авторизации
   * @param text - текст предупреждения
   */
  const renderAuthWarning = useCallback(
    (text: string) => {
      const content = (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <Typography variant="body2">{text}</Typography>
          <Link component="button" variant="body2" onClick={() => signIn()}>
            Войти
          </Link>
        </Box>
      );
      setNotification({
        message: content,
        severity: 'warning',
      });
    },
    [signIn, setNotification]
  );

  /**
   * Ставит или снимает реакцию на комментарий. Счётчик меняется сразу, при ошибке API — откатывается
   * @param comment - комментарий
   * @param code - код реакции
   */
  const reactToComment = useSafeCallback(
    async (comment: CommentItemDto, code: string) => {
      if (!authorizedUser) {
        renderAuthWarning('Реакции могут ставить только авторизованные пользователи.');
        return;
      }

      if (!film?.id) return;

      const isSet = !comment.reactions.some((r) => r.code === code && r.reacted);
      const toggle = () =>
        update((c) =>
          c.id === comment.id ? { ...c, reactions: toggleReaction(c.reactions, code) } : c
        );

      toggle();

      try {
        await commentsApi.setReaction(film.id, comment.id, code, isSet);
      } catch (e) {
        toggle();
        throw e;
      }
    },
    [authorizedUser, film?.id, commentsApi, update, renderAuthWarning]
  );

  /**
   * Добавляет новый комментарий
   * @param text - текст комментария
   * @returns {Promise<boolean>} true, если комментарий успешно добавлен
   */
  const addComment = useSafeCallback(
    async (text: string): Promise<boolean> => {
      if (!authorizedUser) {
        renderAuthWarning('Комментарии могут оставлять только авторизованные пользователи.');
        return false;
      }

      if (!film?.id) return false;

      const id = await commentsApi.add(film.id, text);

      add({
        id,
        text,
        userId: authorizedUser.id,
        userName: authorizedUser.userName,
        photoUrl: authorizedUser.photoUrl,
        createdAt: new Date(),
        reactions: [],
      });

      return true;
    },
    [authorizedUser, film?.id, commentsApi, add, renderAuthWarning]
  );

  if (!film || isLoading)
    return (
      <>
        <AddCommentFormSkeleton />
        <CommentsListSkeleton />
      </>
    );

  return (
    <>
      <AddCommentForm callback={addComment} />

      {comments.length === 0 && !error && <NoData text="Комментариев пока нет" />}

      <CommentsList
        hasMore={hasMore}
        comments={comments}
        onRemove={removeComment}
        onReact={reactToComment}
        next={fetchMore}
      />

      {error && <LoadError text="Не удалось загрузить комментарии" onRetry={retry} />}
    </>
  );
};

export default CommentsModule;
