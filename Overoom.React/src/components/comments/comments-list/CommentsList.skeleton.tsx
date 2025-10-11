import { ReactElement } from 'react';

import CommentItemSkeleton from '../comment-item/CommentItem.skeleton.tsx';

/**
 * Компонент скелетона списка комментариев
 * @returns {ReactElement} JSX элемент скелетона списка комментариев
 */
const CommentsListSkeleton = (): ReactElement => {
  /** Массив для генерации заглушек комментариев */
  const items = Array.from({ length: 6 }, (_, i) => i);

  return (
    <>
      {items.map((i) => (
        <CommentItemSkeleton key={i} />
      ))}
    </>
  );
};

export default CommentsListSkeleton;
