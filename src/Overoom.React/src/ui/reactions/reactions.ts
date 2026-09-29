/** Реакция, которую можно поставить на сообщение или комментарий */
export interface ReactionOption {
  /** Код реакции, совпадающий с кодом на сервере */
  code: string;
  /** Эмодзи для отображения */
  emoji: string;
  /** Название реакции для экранных дикторов */
  label: string;
}

/** Доступные реакции в порядке отображения */
export const REACTIONS: readonly ReactionOption[] = [
  { code: 'like', emoji: '👍', label: 'Нравится' },
  { code: 'love', emoji: '❤️', label: 'Люблю' },
  { code: 'laugh', emoji: '😂', label: 'Смешно' },
  { code: 'wow', emoji: '😮', label: 'Удивительно' },
  { code: 'sad', emoji: '😢', label: 'Грустно' },
  { code: 'fire', emoji: '🔥', label: 'Огонь' },
];

/** Сводка по одной реакции */
export interface ReactionSummary {
  /** Код реакции */
  code: string;
  /** Количество поставивших реакцию */
  count: number;
  /** Поставил ли реакцию текущий пользователь */
  reacted: boolean;
}

/**
 * Возвращает описание реакции по коду
 * @param code - Код реакции
 * @returns {ReactionOption | undefined} Описание реакции или undefined, если реакция неизвестна
 */
export const findReaction = (code: string): ReactionOption | undefined =>
  REACTIONS.find((r) => r.code === code);

/**
 * Сортирует реакции в порядке отображения, неизвестные коды — в конец
 * @param reactions - Сводка по реакциям
 * @returns {ReactionSummary[]} Отсортированная сводка
 */
export const sortReactions = (reactions: ReactionSummary[]): ReactionSummary[] => {
  const order = (code: string) => {
    const index = REACTIONS.findIndex((r) => r.code === code);
    return index === -1 ? REACTIONS.length : index;
  };
  return [...reactions].sort((a, b) => order(a.code) - order(b.code));
};

/**
 * Переключает реакцию текущего пользователя в сводке
 * @param reactions - Сводка по реакциям
 * @param code - Код реакции
 * @returns {ReactionSummary[]} Новая сводка без реакций с нулевым количеством
 */
export const toggleReaction = (reactions: ReactionSummary[], code: string): ReactionSummary[] => {
  const existing = reactions.find((r) => r.code === code);

  if (!existing) return sortReactions([...reactions, { code, count: 1, reacted: true }]);

  const delta = existing.reacted ? -1 : 1;
  return reactions
    .map((r) => (r.code === code ? { ...r, count: r.count + delta, reacted: !r.reacted } : r))
    .filter((r) => r.count > 0);
};
