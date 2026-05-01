import { useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import type { TallyDto } from '../types';

export function useSignalR(
    pollId: number | null,
    onTallyUpdate: (tally: TallyDto) => void
): void {
    useEffect(() => {
        if (pollId == null) return;

        const connection = new HubConnectionBuilder()
            .withUrl('/hubs/tally')
            .build();

        let isStarted = false;

        connection.start()
            .then(() => connection.invoke('JoinPoll', pollId))
            .then(() => {
                isStarted = true;
                connection.on('ReceiveTallyUpdate', onTallyUpdate);
            })
            .catch(err => console.error('SignalR connection failed:', err));

        return () => {
            if (isStarted) {
                connection.invoke('LeavePoll', pollId).finally(() => connection.stop());
            } else {
                connection.stop();
            }
        };
    }, [pollId, onTallyUpdate]);
}
