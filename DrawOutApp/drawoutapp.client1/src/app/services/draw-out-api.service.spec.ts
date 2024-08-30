import { TestBed } from '@angular/core/testing';

import { DrawOutAPIService } from './draw-out-api.service';

describe('DrawOutAPIService', () => {
  let service: DrawOutAPIService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DrawOutAPIService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
