import { ReactElement } from 'react';

import { FilmItemDto } from '../../films/film-item/film-item.dto.ts';
import FilmItem from '../../films/film-item/FilmItem.tsx';

const film: FilmItemDto = {
  description:
    'Себастьян опрометчиво решает познакомить отца с семьей невесты. Отвязная комедия с Робертом де Ниро',
  genres: ['Комедия'],
  id: 'c17c14a0-e59f-450e-8b9f-ac61d90d4835',
  isSerial: false,
  posterUrl: '/img/examples/film_poster_24fe5e09-8323-4179-99a3-ca4479a00a6e.jpg',
  ratingImdb: 6.2,
  ratingKp: 6.2,
  title: 'Уикенд с батей',
  year: 2023,
};

/**
 * Пример карточки фильма из каталога для главной страницы
 * @param props - Пропсы компонента
 * @param props.getLink - Функция построения адреса страницы фильма
 * @returns {ReactElement} JSX элемент карточки фильма
 */
const CatalogExample = ({ getLink }: { getLink: (id: string) => string }): ReactElement => {
  return <FilmItem film={film} typeSelected={false} href={getLink(film.id)} />;
};

export default CatalogExample;
