﻿room.RegisterReceiveEvent('Connect', onConnectReceive)
room.RegisterReceiveEvent('Leave', onLeaveReceive)
room.RegisterReceiveEvent('Pause', onPauseReceive)
room.RegisterReceiveEvent('Seek', onSeekReceive)
room.RegisterReceiveEvent('Message', onMessageReceive)
room.RegisterReceiveEvent('Error', onErrorReceive)
room.RegisterReceiveEvent('ChangeSeries', onSeriesChangedReceive)
room.RegisterReceiveEvent('Type', onTypeReceive)
room.RegisterReceiveEvent('FullScreen', onFullScreenReceive)
room.RegisterReceiveEvent('Kick', onKickReceive)
room.RegisterReceiveEvent('Beep', onBeepReceive)
room.RegisterReceiveEvent('Scream', onScreamReceive)


room.RegisterUserEvent('Load', onLoadUser)
room.RegisterUserEvent('Pause', onPauseUser)
room.RegisterUserEvent('Seek', onSeekUser)
room.RegisterUserEvent('Message', onMessageUser)
room.RegisterUserEvent('ChangeSeries', onSeriesChangedUser)
room.RegisterUserEvent('Type', onTypeUser)
room.RegisterUserEvent('Send', onMessageUser)
room.RegisterUserEvent('FullScreen', onFullScreenUser)
room.RegisterUserEvent('Beep', onBeepUser)
room.RegisterUserEvent('Kick', onKickUser)
room.RegisterUserEvent('Scream', onScreamUser)
room.RegisterUserEvent('Sync', onSyncUser)


const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('/filmRoom', null).build();

hubConnection.onclose(() => setTimeout(async () => await hubConnection.start(), 5000));
hubConnection.on('Error', (id, message) => room.ProcessReceiveEvent(new ErrorReceiveEvent(id, message)));
hubConnection.on('Message', (id, message) => room.ProcessReceiveEvent(new MessageReceiveEvent(id, message)));
hubConnection.on('Pause', (id, pause, second) => room.ProcessReceiveEvent(new PauseReceiveEvent(id, pause, second)));
hubConnection.on('Seek', (id, second) => room.ProcessReceiveEvent(new SeekReceiveEvent(id, second)));
hubConnection.on('ChangeSeries', (id, season, series) => room.ProcessReceiveEvent(new ChangeSeriesReceiveEvent(id, season, series)));
hubConnection.on('Leave', (id) => room.ProcessReceiveEvent(new LeaveReceiveEvent(id)));
hubConnection.on('Type', (id) => room.ProcessReceiveEvent(new TypeReceiveEvent(id)));
hubConnection.on('Beep', (id, target) => room.ProcessReceiveEvent(new BeepReceiveEvent(id, target)));
hubConnection.on('Scream', (id, target) => room.ProcessReceiveEvent(new ScreamReceiveEvent(id, target)));
hubConnection.on('Kick', (id, target) => room.ProcessReceiveEvent(new KickReceiveEvent(id, target)));
hubConnection.on('FullScreen', (id, fullscreen) => room.ProcessReceiveEvent(new FullScreenReceiveEvent(id, fullscreen)));
hubConnection.on('Connect', (data) => {
    room.ProcessReceiveEvent(new ConnectFilmReceiveEvent(data.id, data.username, data.avatar, data.time, data.beep, data.scream, data.change, data.season, data.series))
});

hubConnection.start().then();
room.ProcessUserEvent(new LoadUserEvent())
