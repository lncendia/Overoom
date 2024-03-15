export interface Ratings {
    ratings: Rating[];
    countPages: number;
}

export interface Rating {
    filmId: string;
    title: string;
    year: number;
    posterUrl: string;
    score: number;
    ratingKp?: number;
    ratingImdb?: number;
}