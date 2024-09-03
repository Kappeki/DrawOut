import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  private sessionId: string | null = null;

  constructor() {
    this.sessionId = localStorage.getItem('sessionId');
  }

  setSessionId(sessionId: string): void {
    this.sessionId = sessionId;
    localStorage.setItem('sessionId', sessionId);
  }

  getSessionId(): string | null {
    return this.sessionId;
  }

  clearSessionId(): void {
    this.sessionId = null;
    localStorage.removeItem('sessionId');
  }
}
