import {useCallback, useEffect, useState} from 'react';
import {useInjection} from "inversify-react";
import {IPlaylistsService} from "../../../services/PlaylistsService/IPlaylistsService.ts";
import {useNavigate} from "react-router-dom";
import PlaylistsCatalog from "../../../components/Playlists/PlaylistsCatalog/PlaylistsCatalog.tsx";
import {PlaylistItemData} from "../../../components/Playlists/PlaylistItem/PlaylistItemData.ts";
import NoData from "../../../UI/NoData/NoData.tsx";

interface PlaylistsModuleProps {
    genre?: string;
    className?: string
}

const PlaylistsModule = (props: PlaylistsModuleProps) => {

    const [playlists, setPlaylists] = useState<PlaylistItemData[]>([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const playlistsService = useInjection<IPlaylistsService>('PlaylistsService');

    // Навигационный хук
    const navigate = useNavigate();


    useEffect(() => {
        const processPlaylists = async () => {
            const response = await playlistsService.search({
                genre: props.genre
            })

            setPage(2);
            setHasMore(response.totalPages > 1)
            setPlaylists(response.list)
        };

        processPlaylists().then()
    }, [playlistsService, props]);

    const next = useCallback(async () => {
        const response = await playlistsService.search({
            genre: props.genre,
            page: page
        })
        setPage(page + 1);
        setHasMore(response.totalPages !== page)
        setPlaylists(prev => [...prev, ...response.list])
    }, [playlistsService, props.genre, page])

    const onSelect = useCallback((playlist: PlaylistItemData) => {
        navigate('/playlist', {state: {id: playlist.id}})
    }, [navigate])

    if (playlists.length === 0) return <NoData className={props.className} text="Подборки не найдены"/>

    return <PlaylistsCatalog className={props.className} hasMore={hasMore} next={next} genre={props.genre}
                             playlists={playlists} onSelect={onSelect}/>
};

export default PlaylistsModule;