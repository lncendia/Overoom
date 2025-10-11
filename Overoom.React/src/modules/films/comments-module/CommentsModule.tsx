import { Box, Link } from '@mui/material';
import Typography from '@mui/material/Typography';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

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
      if (!film) return null;

      const response = await commentsApi.get(film.id, { skip, take });

      const mappedComments = response.list.map<CommentItemDto>((item) => ({
        ...item,
        isUserComment: authorizedUser?.id === item.userId,
        createdAt: new Date(item.createdAt),
      }));

      return {
        list: mappedComments,
        totalCount: response.totalCount,
      };
    },
    [commentsApi, film, authorizedUser?.id]
  );

  /** Хук для порционной загрузки комментариев */
  const {
    items: comments,
    isLoading,
    hasMore,
    fetchMore,
    removeWhere,
    add,
  } = usePaginatedFetch<CommentItemDto>(fetch, 20);

  /**
   * Удаляет комментарий
   * @param comment - удаляемый комментарий
   */
  const removeComment = useSafeCallback(
    async (comment: CommentItemDto) => {
      if (!film) return;
      removeWhere((c) => c.id !== comment.id);
      await commentsApi.delete(film.id, comment.id);
    },
    [commentsApi, film, removeWhere]
  );

  /**
   * Отображает предупреждение о необходимости авторизации
   */
  const renderAuthWarning = useCallback(() => {
    const content = (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography variant="body2">
          Комментарии могут оставлять только авторизованные пользователи.
        </Typography>
        <Link component="button" variant="body2" onClick={() => authApi.signIn()}>
          Войти
        </Link>
      </Box>
    );
    setNotification({
      message: content,
      severity: 'warning',
    });
  }, [authApi, setNotification]);

  /**
   * Добавляет новый комментарий
   * @param text - текст комментария
   */
  const addComment = useSafeCallback(
    async (text: string) => {
      // Если пользователь не авторизован, отображаем предупреждение
      if (!authorizedUser) {
        renderAuthWarning();
        return;
      }

      if (!film) return;

      const id = await commentsApi.add(film.id, text);

      const comment: CommentItemDto = {
        id,
        text,
        userName: authorizedUser.userName,
        photoUrl: authorizedUser.photoUrl,
        createdAt: new Date(),
        isUserComment: true,
      };
      add(comment);
    },
    [authorizedUser, commentsApi, film, add, renderAuthWarning]
  );

  // Отображаем скелетоны при загрузке
  if (isLoading)
    return (
      <>
        <AddCommentFormSkeleton />
        <CommentsListSkeleton />
      </>
    );

  // Основной рендер: форма добавления комментариев и список комментариев
  return (
    <>
      <AddCommentForm callback={addComment} />
      <CommentsList
        hasMore={hasMore}
        comments={comments}
        onRemove={removeComment}
        next={fetchMore}
      />
    </>
  );
};

export default CommentsModule;
