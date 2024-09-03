import { TestBed } from '@angular/core/testing';

import { RoomHubService } from './room-hub.service';

describe('RoomSignalService', () => {
  let service: RoomHubService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RoomHubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
