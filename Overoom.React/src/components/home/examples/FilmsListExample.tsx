import { FilmShortDto } from '../../films/film-short-item/film-short.dto.ts';
import FilmsList from '../../films/films-list/FilmsList.tsx';

const films: FilmShortDto[] = [
  {
    score: 10,
    id: '93d9c224-d3fe-4ebe-9ae4-98223144b28e',
    title: 'Джентльмены',
    ratingKp: 8.6,
    ratingImdb: 8.6,
    posterUrl: 'img/examples/film_poster_010d4c9e-753b-494a-b17c-fd605e7c3905.jpg',
  },
  {
    score: 6,
    id: '486d5e84-9a29-4e90-86de-ce14023924fa',
    title: 'Стражи Галактики. Часть 3',
    ratingKp: 8.3,
    ratingImdb: 8.3,
    posterUrl: 'img/examples/film_poster_ee0bd4e5-c182-404c-b971-15f3b4488b0f.jpg',
  },
  {
    score: 10,
    id: 'b85a1a54-3e88-4997-802b-04296bd678c9',
    title: 'Алиса в Пограничье',
    ratingKp: 7.5,
    ratingImdb: 7.5,
    posterUrl: 'img/examples/film_poster_ffbce131-8a48-4fae-8d50-d07269ad1aaf.jpg',
  },
  {
    score: 9,
    id: '5748ff49-f4ef-4673-832b-dc96ce294b34',
    title: 'Гнев человеческий',
    ratingKp: 7.6,
    ratingImdb: 7.6,
    posterUrl: 'img/examples/film_poster_11ec63af-43f8-4f2d-a960-5727c38aaa8a.jpg',
  },
  {
    score: 7,
    id: 'b216703a-1fa9-4a87-8e03-ef45b3c463db',
    title: 'Властелин колец: Кольца власти',
    ratingKp: 6.2,
    ratingImdb: 6.2,
    posterUrl: 'img/examples/film_poster_637432ec-d984-4a71-9af3-5c5f93e3286e.jpg',
  },
];

const FilmsListExample = ({ onSelect }: { onSelect: (id: string) => void }) => {
  return <FilmsList films={films} onSelect={onSelect} />;
};

export default FilmsListExample;
