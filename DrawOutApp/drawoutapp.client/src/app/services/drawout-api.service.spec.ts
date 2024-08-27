import { TestBed } from '@angular/core/testing';

import { DrawoutApiService } from './drawout-api.service';

describe('DrawoutApiService', () => {
  let service: DrawoutApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DrawoutApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
