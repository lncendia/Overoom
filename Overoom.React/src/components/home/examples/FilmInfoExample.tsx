import { useCallback, useState } from 'react';

import Drawer from '../../../ui/drawer/Drawer.tsx';
import { FilmInfoDto } from '../../film/film-info/film-info.dto.ts';
import FilmInfo from '../../film/film-info/FilmInfo.tsx';
import CreateRoomForm from '../../rooms/create-room-form/CreateRoomForm.tsx';

const film: FilmInfoDto = {
  countries: ['США', 'Германия', 'Великобритания', 'ЮАР', 'Австралия'],
  directors: ['Кэрол Гриффитс', 'Фрэнсис Аннан', 'Л.Х. Адамс', 'Тим Дженкин'],
  screenWriters: ['Фрэнсис Аннан'],
  actors: [
    {
      name: 'Марк Леонард Винтер',
      role: 'Leonard Fontaine',
    },
    {
      name: 'Дэниэл Рэдклифф',
      role: 'Tim Jenkin',
    },
    {
      name: 'Иэн Харт',
      role: 'Denis Goldberg',
    },
    {
      name: 'Дэниэл Уэббер',
      role: 'Stephen Lee',
    },
    {
      name: 'Нэйтан Пейдж',
      role: 'Mongo',
    },
    {
      name: 'Грант Пиро',
      role: 'Schnepel',
    },
    {
      name: 'Ленни Фёрт',
      role: 'Vermellen',
    },
    {
      name: 'Лен Фёрт',
      role: 'Vermellen',
    },
    {
      name: 'Лайам Амор',
      role: 'Kitson',
    },
    {
      name: 'Адам Туоминен',
      role: 'Jeremy',
    },
  ],
  id: 'b8bc71b2-b380-44e4-b0ac-d041edaa837a',
  title: 'Побег из Претории',
  year: 2020,
  ratingKp: 6.7,
  ratingImdb: 6.9,
  description:
    'Двое борцов за свободу отбывают срок в одной из самых строгих тюрем мира – в «Претории». Вместе с другими узниками они планируют дерзкий и опасный побег. Но придумать план – это только первый шаг. Шаг второй – реализация плана.',
  isSerial: false,
  genres: ['Биография', 'Детектив', 'Криминал', 'Триллер'],
  posterUrl: 'img/examples/film_poster_07f9cd71-e165-4173-9d5e-54010b89e4ec.jpg',
};

/** Пропсы компонента информации о фильме */
export interface FilmInfoExampleProps {
  /** Обработчик выбора страны */
  onCountrySelect: (value: string) => void;
  /** Обработчик выбора жанра */
  onGenreSelect: (value: string) => void;
  /** Обработчик выбора персоны */
  onPersonSelect: (value: string) => void;
  /** Обработчик выбора типа */
  onTypeSelect: (value: string) => void;
  /** Обработчик выбора года */
  onYearSelect: (value: string) => void;
}

const FilmInfoExample = (props: FilmInfoExampleProps) => {
  /**
   * Заглушка без реализации, добавлена для полноты интерфейса.
   * @returns {void}
   */
  const handle = useCallback((): void => {}, []);

  /** Состояние для управления отображением формы создания комнаты */
  const [formOpen, setFormOpen] = useState(false);

  return (
    <>
      <FilmInfo
        {...props}
        film={film}
        isWatchlistEnabled={true}
        inWatchlist={false}
        buttonText="Создать комнату"
        onButtonClicked={() => setFormOpen(true)}
        onWatchlistToggle={handle}
      />
      <Drawer title="Создание комнаты" show={formOpen} onClose={() => setFormOpen(false)}>
        <CreateRoomForm callback={() => setFormOpen(false)} />
      </Drawer>
    </>
  );
};

export default FilmInfoExample;
