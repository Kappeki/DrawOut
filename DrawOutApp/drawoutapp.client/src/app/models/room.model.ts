export interface Room {
    id: string;
    name: string;
    isLocked: boolean;
    password?: string; // mozda je bolje da se ne stavlja ovde, vec da se radi na backendu
    playerCount: number;
    timeLeft: number; // time left in milliseconds, koliko je vremena ostalo do isteka sobe
  }