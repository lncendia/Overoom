import ChangeSeriesPlayerEvent from './change-series.event.ts';
import FullscreenPlayerEvent from './fullscreen.event.ts';
import VolumePlayerEvent from './muted.event.ts';
import PausePlayerEvent from './pause.event.ts';
import SeekPlayerEvent from './seek.event.ts';
import SpeedPlayerEvent from './speed.event.ts';

export default interface PlayerEventContainer {
  pauseEvent?: PausePlayerEvent;
  seekEvent?: SeekPlayerEvent;
  fullscreenEvent?: FullscreenPlayerEvent;
  muteEvent?: VolumePlayerEvent;
  speedEvent?: SpeedPlayerEvent;
  changeEpisodeEvent?: ChangeSeriesPlayerEvent;
  initEvent?: object;
}
