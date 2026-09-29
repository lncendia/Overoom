export default interface PausePlayerEvent {
  onPause: boolean;
  ticks: number;
  buffering: boolean;
}
