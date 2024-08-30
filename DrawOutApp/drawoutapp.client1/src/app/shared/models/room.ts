export class RoomListItem {
    constructor(
        public roomId: string,
        public roomName: string,
        public hasPassword: boolean,
        public gameState: string,
        public playerCount: number
    ) {
    }
}

export class Room {
    constructor(
        public roomId: string,
        public roomName: string,
        public playerCount: number,
        public roomAdminId: string,
        public gameState: string,
        public roundTime: number,
        public timeElapsed: Date,
        public passwordHash?: string,
        public roomURL?: string,
        public players?: string[],
        public customWords?: string[],
        public selectedWordPack?: string
      ) {
    }
}