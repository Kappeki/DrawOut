import { TestBed } from '@angular/core/testing';

import { GameSignalService } from './game-signal.service';

describe('GameSignalService', () => {
  let service: GameSignalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameSignalService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
