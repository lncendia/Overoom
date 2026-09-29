import AddReactionOutlinedIcon from '@mui/icons-material/AddReactionOutlined';
import { Chip, IconButton, Popover, Stack, Tooltip, Typography } from '@mui/material';
import { MouseEvent, ReactElement, useState } from 'react';

import { findReaction, REACTIONS, ReactionSummary } from './reactions.ts';

/** Пропсы компонента ReactionsBar */
interface ReactionsBarProps {
  /** Сводка по реакциям */
  reactions: ReactionSummary[];
  /** Обработчик установки или снятия реакции. Без него реакции только отображаются */
  onReact?: (code: string) => void;
}

/**
 * Панель реакций: счётчики поставленных реакций и кнопка выбора новой
 * @param props - Пропсы компонента
 * @param props.reactions - Сводка по реакциям
 * @param props.onReact - Обработчик установки или снятия реакции
 * @returns {ReactElement} JSX элемент панели реакций
 */
const ReactionsBar = ({ reactions, onReact }: ReactionsBarProps): ReactElement | null => {
  /** Элемент, к которому привязан выбор реакции */
  const [pickerAnchor, setPickerAnchor] = useState<HTMLElement | null>(null);

  /**
   * Ставит или снимает реакцию и закрывает выбор реакции
   * @param code - Код реакции
   */
  const react = (code: string) => {
    setPickerAnchor(null);
    onReact?.(code);
  };

  if (reactions.length === 0 && !onReact) return null;

  return (
    <Stack direction="row" sx={{ flexWrap: 'wrap', alignItems: 'center', gap: 0.5 }}>
      {reactions.map((r) => {
        const option = findReaction(r.code);
        const label = option?.label ?? r.code;
        return (
          <Tooltip key={r.code} title={label}>
            <Chip
              size="small"
              label={`${option?.emoji ?? r.code} ${r.count}`}
              color={r.reacted ? 'primary' : 'default'}
              variant={r.reacted ? 'filled' : 'outlined'}
              onClick={onReact ? () => react(r.code) : undefined}
              aria-label={`${label}: ${r.count}`}
              aria-pressed={onReact ? r.reacted : undefined}
            />
          </Tooltip>
        );
      })}
      {onReact && (
        <>
          <IconButton
            size="small"
            aria-label="Поставить реакцию"
            onClick={(e: MouseEvent<HTMLElement>) => setPickerAnchor(e.currentTarget)}
          >
            <AddReactionOutlinedIcon fontSize="small" />
          </IconButton>
          <Popover
            open={pickerAnchor !== null}
            anchorEl={pickerAnchor}
            onClose={() => setPickerAnchor(null)}
            anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
            transformOrigin={{ vertical: 'bottom', horizontal: 'center' }}
          >
            <Stack direction="row" sx={{ p: 0.5 }}>
              {REACTIONS.map((r) => (
                <IconButton key={r.code} aria-label={r.label} onClick={() => react(r.code)}>
                  <Typography sx={{ fontSize: '1.25rem', lineHeight: 1 }}>{r.emoji}</Typography>
                </IconButton>
              ))}
            </Stack>
          </Popover>
        </>
      )}
    </Stack>
  );
};

export default ReactionsBar;
