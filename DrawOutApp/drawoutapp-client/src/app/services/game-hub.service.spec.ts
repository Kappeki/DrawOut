import { TestBed } from '@angular/core/testing';

import { GameHubService } from './game-hub.service';

describe('GameHubService', () => {
  let service: GameHubService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameHubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
