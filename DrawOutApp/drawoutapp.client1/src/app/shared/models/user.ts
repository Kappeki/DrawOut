export class User {
    constructor(
        public nickname: string, 
        public icon: string, 
        public roles?: string[]) {
    }
}