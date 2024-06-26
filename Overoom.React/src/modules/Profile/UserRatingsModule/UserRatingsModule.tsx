import FilmsList from "../../../components/Films/FilmsList/FilmsList.tsx";
import {useNavigate} from "react-router-dom";
import {useInjection} from "inversify-react";
import {useCallback, useEffect, useState} from "react";
import {IProfileService} from "../../../services/ProfileService/IProfileService.ts";
import {FilmShortData} from "../../../components/Films/FilmShortItem/FilmShortData.ts";
import NoData from "../../../UI/NoData/NoData.tsx";

const UserRatingsModule = ({className}: { className?: string }) => {

    const [ratings, setRatings] = useState<FilmShortData[]>([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);

    const profileService = useInjection<IProfileService>('ProfileService');

    // Навигационный хук
    const navigate = useNavigate();


    useEffect(() => {
        const processRatings = async () => {
            const response = await profileService.ratings({})

            setPage(2);
            setHasMore(response.totalPages > 1)
            setRatings(response.list)
        };

        processRatings().then()
    }, [profileService]);

    const next = useCallback(async () => {
        const response = await profileService.ratings({
            page: page
        })
        setPage(page + 1);
        setHasMore(response.totalPages !== page)
        setRatings(prev => [...prev, ...response.list])

    }, [profileService, page])

    const onSelect = useCallback((film: FilmShortData) => {
        navigate('/film', {state: {id: film.id}})
    }, [navigate])

    if (ratings.length == 0) return <NoData text="Пусто"/>

    return <FilmsList className={className} hasMore={hasMore} next={next} films={ratings} onSelect={onSelect}/>
};

export default UserRatingsModule;