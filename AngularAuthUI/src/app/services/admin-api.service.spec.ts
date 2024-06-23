import { TestBed } from '@angular/core/testing';

import { AdminAPIService } from './admin-api.service';

describe('AdminAPIService', () => {
  let service: AdminAPIService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminAPIService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
