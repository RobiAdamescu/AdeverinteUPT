import { TestBed } from '@angular/core/testing';

import { StudentResponseApiService } from './student-response-api.service';

describe('StudentResponseApiService', () => {
  let service: StudentResponseApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StudentResponseApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
