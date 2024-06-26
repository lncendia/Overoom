export interface FilmRoom {
    title: string;
    cdnName: string;
    cdnUrl: string;
    isSerial: boolean;
    id: string;
    ownerId: string;
    viewers: FilmViewer[];
}

export interface FilmViewer {
    season?: number;
    series?: number;
    id: string;
    username: string;
    photoUrl?: string;
    pause: boolean;
    fullScreen: boolean;
    online: boolean;
    second: number;
    allows: Allows;
}

export interface Allows {
    beep: boolean;
    scream: boolean;
    change: boolean;
}