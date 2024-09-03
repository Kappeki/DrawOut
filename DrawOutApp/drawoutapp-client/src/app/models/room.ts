export class RoomListItem {
    constructor(
        public roomId: string,
        public roomName: string,
        public hasPassword: boolean,
        public roomState: string,
        public playerCount: number
    ) {
    }
}

//izmena da room ima id

export interface Room {
    roomName: string;
    passwordHash?: string;
    roomURL?: string;
    playerCount: number;
    roomAdminId?: string;
    players?: string[];
    customWords?: string[];
    selectedWordPack?: string;
    roomState?: string;
    roundTime: number;
    timeElapsed: Date;
}