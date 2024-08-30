import { TestBed } from '@angular/core/testing';

import { RoomSignalService } from './room-signal.service';

describe('RoomSignalService', () => {
  let service: RoomSignalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RoomSignalService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
