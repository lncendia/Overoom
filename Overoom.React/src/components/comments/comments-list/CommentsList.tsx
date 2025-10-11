import { ReactElement } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';

import Spinner from '../../../ui/spinners/Spinner.tsx';
import { CommentItemDto } from '../comment-item/comment-item.dto.ts';
import CommentItem from '../comment-item/CommentItem.tsx';

/** Пропсы компонента списка комментариев */
interface CommentListParams {
  /** Массив комментариев */
  comments: CommentItemDto[];
  /** Функция удаления комментария */
  onRemove: (comment: CommentItemDto) => void;
  /** Флаг наличия дополнительных комментариев */
  hasMore: boolean;
  /** Функция загрузки следующих комментариев */
  next: () => void;
}

/**
 * Компонент списка комментариев с бесконечной прокруткой
 * @param props - Пропсы компонента
 * @returns {ReactElement} JSX элемент списка комментариев
 */
const CommentsList = (props: CommentListParams): ReactElement => {
  const scrollProps = {
    style: { overflow: 'visible' },
    dataLength: props.comments.length,
    next: props.next,
    hasMore: props.hasMore,
    loader: <Spinner />,
  };

  return (
    <InfiniteScroll {...scrollProps}>
      {props.comments.map((comment) => (
        <CommentItem
          key={comment.id}
          comment={comment}
          removeComment={() => props.onRemove(comment)}
        />
      ))}
    </InfiniteScroll>
  );
};

export default CommentsList;
